using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

namespace IntoTheWoods.Characters {
    public delegate void MoveEventHandler(Vector2 newPosition, bool ignoreDistance = false, bool inTransferZone = false);

    public delegate void TransferEventHandler(Vector2 moveVector);


    /// <summary>
    /// Any character that can walk, not necessarily controled by a human.
    /// </summary>
    public class Walker : MonoBehaviour {
        // Hand-crafted constants
        private const float BackLaneScaleReduction = 0.2f;
        private const float FrontLaneScale = 1f;
        private const float BackLaneScale = FrontLaneScale - BackLaneScaleReduction;

        // Cached property indices for animator for efficiency
        private static readonly int Walking = Animator.StringToHash("walking");

        // configurable fields
        [SerializeField] private float speed = 1.7f;

        // internal setup fields
        private Animator _animator;
        private TransferDetector _transferDetector;
        private SortingGroup _sortingGroup;

        // states
        [ShowInInspector] public bool IsWalking { get; private set; }
        [ShowInInspector] private Vector2 _walkingDirection;
        [ShowInInspector] private readonly Dictionary<int, Collider2D> _inTransferZones = new();

        public bool CanTransfer => _inTransferZones.Count > 0;

        /// <summary>
        /// true = back lane, false = front lane
        /// </summary>
        public bool BackLane { get; private set; }

        /// <summary>
        /// only set when actually transfering
        /// </summary>
        private Vector2 _currentTransferTarget;
        /// <summary>
        /// always set when a transfer collider is entered
        /// </summary>
        private Vector2 _nextTransferTarget;
        /// <summary>
        /// The position of the transfer target the walker is currently standing on.
        /// Not valid when <see cref="CanTransfer"/> is false.
        /// </summary>
        private Vector2 _inTransferTarget;

        public bool IsTransfering { get; private set; }

        // Step Audio
        private FootstepController _footstepController;

        // events
        public event PlayerWillMoveEventHandler WillMove;
        public event MoveEventHandler Moved;
        public event TransferEventHandler Transfering;

        private void Awake() {
            _sortingGroup = GetComponentInChildren<SortingGroup>();
            Assert.IsNotNull(_sortingGroup);
            _animator = GetComponentInChildren<Animator>();
            Assert.IsNotNull(_animator);
            _transferDetector = GetComponentInChildren<TransferDetector>();
            _footstepController = GetComponentInChildren<FootstepController>();
        }

        private void OnEnable() {
            if (_transferDetector != null) {
                _transferDetector.TransferZoneEntered += OnTransferZoneEntered;
                _transferDetector.TransferZoneLeft += OnTransferZoneLeft;
            }
        }

        private void OnDisable() {
            if (_transferDetector != null) {
                _transferDetector.TransferZoneEntered -= OnTransferZoneEntered;
                _transferDetector.TransferZoneLeft -= OnTransferZoneLeft;
            }
        }

        private void Update() {
            if (IsWalking) {
                if (GameState.Instance.kidsAreDoomed) {
                    StopWalking();
                    return;
                }
                // By sending the event before moving, we have the game manager check if we are going out of bounds
                // the order is relevant to prevent glitching outside
                Vector3 posDelta = new(speed * Time.deltaTime * _walkingDirection.x, 0);

                if (_walkingDirection.y == 0) {
                    // actual player-controled walking has y = 0 set manually
                    if (!WillMove?.Invoke(transform.position + posDelta, _walkingDirection) ?? false) {
                        StopWalking();
                        return;
                    }

                    transform.position += posDelta;
                    Moved?.Invoke(transform.position, inTransferZone: CanTransfer);
                }
                else {
                    // if y is != 0, the animation was turned on for the move to foreground / background animation
                    transform.position += new Vector3(speed * Time.deltaTime * _walkingDirection.x, speed * Time.deltaTime * _walkingDirection.y, 0);
                    float distance = new Vector2(transform.position.x - _currentTransferTarget.x, transform.position.y - _currentTransferTarget.y).magnitude;
                    float scaleScalar;
                    if ((_currentTransferTarget.y > TransferZone.BackFrontThreshold // back lane transfer zone 
                         && transform.position.y >= _currentTransferTarget.y) // going up
                        || (_currentTransferTarget.y < TransferZone.BackFrontThreshold
                            && transform.position.y <= _currentTransferTarget.y)
                       ) {
                        // close enough
                        Vector3 pos = transform.position;
                        pos.y = _currentTransferTarget.y;
                        transform.position = pos;
                        StopWalking();
                        IsTransfering = false;
                        // determine final scale
                        scaleScalar = _walkingDirection.y > 0 ? BackLaneScale : FrontLaneScale;
                    }
                    else {
                        // calculate scale
                        if (_walkingDirection.y > 0) {
                            // walking towards the back, become smaller
                            // (1f - distance) * BackLaneScaleReduction is what reduces up to BackLaneScaleReduction the further back the position is
                            // (1f - distance) assumes that distance goes from 0 to 1, which it doesn't,
                            // but the animation is so fast and the differences are so small that it doesn't matter.
                            // This will break though should we ever create transfer zones that are further away than 1.
                            scaleScalar = FrontLaneScale - (1f - distance) * BackLaneScaleReduction;
                        }
                        else {
                            scaleScalar = BackLaneScale + (1f - distance) * BackLaneScaleReduction;
                        }
                    }

                    transform.localScale = new(scaleScalar * Math.Sign(transform.localScale.x), scaleScalar, scaleScalar);
                }
            }
        }

        private void OnTransferZoneEntered(Collider2D obj) {
            _inTransferZones.TryAdd(obj.GetInstanceID(), obj);
            TransferZone transferZone = obj.GetComponent<TransferZone>();
            _nextTransferTarget = transferZone.partner.position;
            _inTransferTarget = transferZone.transform.position;
        }

        private void OnTransferZoneLeft(Collider2D obj) {
            _inTransferZones.Remove(obj.GetInstanceID());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="moveVector">digitized movement direction</param>
        /// <param name="doNotFlip">Hack for witch, which is oriented the other way</param>
        public void ActivateWalking(Vector2 moveVector, bool doNotFlip = false) {
            IsWalking = true;
            _walkingDirection = moveVector;
            _animator.SetBool(Walking, true);
            _footstepController.StartWalking();

            // face the right direction
            if (!doNotFlip) {
                Vector3 scale = transform.localScale;
                if (Math.Sign(scale.x) != Math.Sign(moveVector.x)) {
                    scale.x *= -1;
                    transform.localScale = scale;
                }
            }
        }

        public void ActivateTransfer(Vector2 inputVector, bool isPlayer = false) {
            // check if possible direction matches
            Vector2 moveVector = _nextTransferTarget - (Vector2)transform.position;
            if (Math.Sign(moveVector.y) != Math.Sign(inputVector.y)) {
                return;
            }

            _currentTransferTarget = _nextTransferTarget;
            IsTransfering = true;
            BackLane = !BackLane;
            //Change the order of the layer
            if (BackLane) {
                _sortingGroup.sortingLayerName = "PlayerBack";
            }
            else {
                _sortingGroup.sortingLayerName = "PlayerFront";
            }

            ActivateWalking(_currentTransferTarget - (Vector2)transform.position);

            // if this is the leading character (player), send a forced move event so the follower reaches the transfer zone
            if (isPlayer) {
                Moved?.Invoke(_inTransferTarget, true);
            }

            Transfering?.Invoke(inputVector);
        }

        public void StopWalking() {
            IsWalking = false;
            _animator.SetBool(Walking, false);
            _footstepController.StopWalking();
        }

        public bool IsBusy() {
            return IsTransfering;
        }
    }
}
