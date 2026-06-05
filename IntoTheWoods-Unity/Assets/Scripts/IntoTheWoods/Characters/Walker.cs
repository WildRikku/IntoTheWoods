using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace IntoTheWoods.Characters {
    /// <summary>
    /// Any character that can walk, not necessarily controled by a human.
    /// </summary>
    public class Walker : MonoBehaviour {
        // Cached property indices for animator for efficiency
        private static readonly int Walking = Animator.StringToHash("walking");

        // configurable fields
        [SerializeField] private float speed = 1.7f;

        // internal setup fields
        private Animator _animator;
        private TransferDetector _transferDetector;

        // states
        private bool _walking;
        private Vector2 _walkingDirection;
        private bool _canTransfer;
        /// <summary>
        /// only set when actually transfering
        /// </summary>
        private Vector2 _currentTransferTarget;
        /// <summary>
        /// always set when a transfer collider is entered
        /// </summary>
        private Vector2 _nextTransferTarget;

        public bool IsTransfering { get; private set; }

        // events
        public event PlayerWillMoveEventHandler WillMove;

        private void Awake() {
            _animator = GetComponentInChildren<Animator>();
            Assert.IsNotNull(_animator);
            _transferDetector = GetComponentInChildren<TransferDetector>();
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
            if (_walking) {
                // By sending the event before moving, we have the game manager check if we are going out of bounds
                // the order is relevant to prevent glitching outside
                if (!WillMove?.Invoke(this, _walkingDirection) ?? false) {
                    StopWalking();
                    return;
                }

                if (_walkingDirection.y == 0) {
                    // actual player-controled walking has y = 0 set manually    
                    transform.position += new Vector3(speed * Time.deltaTime * _walkingDirection.x, 0);
                }
                else {
                    // if y is != 0, the animation was turned on for the move to foreground / background animation
                    transform.position += new Vector3(speed * Time.deltaTime * _walkingDirection.x, speed * Time.deltaTime * _walkingDirection.y, 0);
                    float distance = new Vector2(transform.position.x - _currentTransferTarget.x, transform.position.y - _currentTransferTarget.y).magnitude;
                    float scaleScalar;

                    if (distance < 0.05f) {
                        // close enough
                        StopWalking();
                        IsTransfering = false;
                        // determine final scale
                        scaleScalar = _walkingDirection.y > 0 ? 0.8f : 1f;
                    }
                    else {
                        // calculate scale
                        if (_walkingDirection.y > 0) {
                            // walking towards the back, become smaller
                            // (1f - distance) * 0.2f is what reduces up to 0.2 the further back the position is
                            scaleScalar = 1f - (1f - distance) * 0.2f;
                        }
                        else {
                            scaleScalar = 0.8f + (1f - distance) * 0.2f;
                        }
                    }

                    transform.localScale = new(scaleScalar * Math.Sign(transform.localScale.x), scaleScalar, scaleScalar);
                }
            }
        }

        private void OnTransferZoneEntered(Collider2D obj) {
            _canTransfer = true;
            _nextTransferTarget = obj.GetComponent<TransferZone>().partner.position;
        }

        private void OnTransferZoneLeft(Collider2D obj) {
            _canTransfer = false;
        }

        public void ActivateWalking(Vector2 moveVector) {
            _walking = true;
            _walkingDirection = moveVector;
            _animator.SetBool(Walking, true);

            // face the right direction
            Vector3 scale = transform.localScale;
            if (Math.Sign(scale.x) != Math.Sign(moveVector.x)) {
                scale.x *= -1;
                transform.localScale = scale;
            }
        }

        public void ActivateTransfer(Vector2 inputVector) {
            // check if possible direction matches
            Vector2 moveVector = _nextTransferTarget - (Vector2)transform.position;
            if (Math.Sign(moveVector.y) != Math.Sign(inputVector.y)) {
                return;
            }

            _currentTransferTarget = _nextTransferTarget;
            IsTransfering = true;
            ActivateWalking(_currentTransferTarget - (Vector2)transform.position);
        }

        public void StopWalking() {
            _walking = false;
            _animator.SetBool(Walking, false);
        }

        public bool IsBusy() {
            return IsTransfering;
        }

        public bool CanTransfer() {
            return _canTransfer;
        }
    }
}
