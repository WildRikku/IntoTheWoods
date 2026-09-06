using System;
using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

namespace IntoTheWoods.Characters {
    public class Falcon : MonoBehaviour {
        #region Hashes and constants

        private static readonly int Attacking = Animator.StringToHash("attacking");
        private static readonly int Captured = Animator.StringToHash("captured");
        private static readonly int Landed = Animator.StringToHash("landed");
        private static readonly int Rising = Animator.StringToHash("rising");
        private const float DefaultHeight = 1.495f;
        private const float WaitOnGroundTime = 1.5f;
        private const float ApproachDistance = 2f;

        #endregion

        #region State Machine

        private abstract class State {
            public Action Done { get; set; }
            public Animator animator;

            public virtual void Exit() {
            }
        }

        private abstract class PassiveState : State {
            public abstract PassiveState Enter();
            public abstract bool UpdateState();
        }

        private abstract class MoveState : State {
            public virtual MoveState Enter(Falcon falcon) {
                return this; // TODO why?
            }

            public abstract bool UpdateState(Falcon falcon, out Vector3 deltaPos);
        }

        private abstract class MoveToTargetState : MoveState {
            protected Vector3 currentTarget;

            protected MoveToTargetState(Vector3 currentTarget) {
                this.currentTarget = currentTarget;
            }
        }

        private class PatrolState : MoveState {
            private Vector2 _flyingDirection = new(-1, 0);
            public float leftEnd;
            public float rightEnd;

            public override bool UpdateState(Falcon falcon, out Vector3 deltaPos) {
                // Change flying and facing direction (scale) according to bounds
                // This also fixes things when falcon is spawned out of bounds
                if (falcon.transform.position.x < leftEnd) {
                    _flyingDirection = new(1, 0);
                    Vector3 scale = falcon.transform.localScale;
                    scale.x = -1;
                    falcon.transform.localScale = scale;
                }
                else if (falcon.transform.position.x > rightEnd) {
                    _flyingDirection = new(-1, 0);
                    Vector3 scale = falcon.transform.localScale;
                    scale.x = 1;
                    falcon.transform.localScale = scale;
                }
                deltaPos = new(falcon.speed * Time.deltaTime * _flyingDirection.x, 0);
                return false; // hacky since this is the only state that does not have an end
            }
        }

        /// <summary>
        /// Fly towards target until reaching <see cref="Falcon.ApproachDistance"/> on x axis.
        /// Target will not be updated once state is initialized.
        /// If you want to change the target, you need to update <see cref="MoveToTargetState.currentTarget"/> yourself.
        /// </summary>
        /// <seealso cref="ObserveState"/>
        private class ApproachingState : MoveToTargetState {
            private Vector2 _flyingDirection;

            public ApproachingState(Vector3 currentTarget) : base(currentTarget) {
            }

            public override MoveState Enter(Falcon falcon) {
                _flyingDirection = new(-Math.Sign(falcon.transform.localScale.x), 0); // keep direction
                return this;
            }

            public override bool UpdateState(Falcon falcon, out Vector3 deltaPos) {
                deltaPos = new(falcon.speed * Time.deltaTime * _flyingDirection.x, 0);
                return Math.Abs((currentTarget - falcon.transform.position).x) <= ApproachDistance;
            }
        }

        private class AttackLandingState : MoveToTargetState {
            public AttackLandingState(Vector3 currentTarget) : base(currentTarget) {
            }

            public override MoveState Enter(Falcon falcon) {
                animator.SetBool(Attacking, true);
                return this;
            }

            public override bool UpdateState(Falcon falcon, out Vector3 deltaPos) {
                deltaPos = (currentTarget - falcon.transform.position).normalized * (falcon.speed * Time.deltaTime);
                return Math.Abs((currentTarget - falcon.transform.position).magnitude) < 0.1f;
            }

            public override void Exit() {
                animator.SetBool(Attacking, false);
            }
        }

        private class OnFloorState : PassiveState {
            private float _waitedTime;

            public override PassiveState Enter() {
                animator.SetBool(Landed, true);
                _waitedTime = 0;
                return this;
            }

            public override bool UpdateState() {
                _waitedTime += Time.deltaTime;
                return _waitedTime > WaitOnGroundTime;
            }

            public override void Exit() {
                animator.SetBool(Landed, false);
            }
        }

        private class RisingState : MoveToTargetState {
            public RisingState(Falcon falcon) : base(
                new(falcon.transform.position.x + 5 * -Math.Sign(falcon.transform.localScale.x), DefaultHeight, 0)) {
                // TODO do not hardcode x distance (use approach distance?)
            }

            public override MoveState Enter(Falcon falcon) {
                animator.SetBool(Rising, true);
                return this;
            }

            public override bool UpdateState(Falcon falcon, out Vector3 deltaPos) {
                deltaPos = (currentTarget - falcon.transform.position).normalized * (falcon.speed * Time.deltaTime);
                return Math.Abs((currentTarget - falcon.transform.position).magnitude) < 0.1f;
            }

            public override void Exit() {
                animator.SetBool(Rising, false);
            }
        }

        /// <summary>
        /// Fake Falcon returning to base with a captured mouse.
        /// It will fly until it is out of sight, then continue for <see cref="Screen.ScreenWidth"/>/<see cref="Falcon.speed"/>.
        /// </summary>
        private class ReturnBaseState : MoveState {
            protected Vector2 _FlyingDirection;
            protected bool _HasLeftScreen;
            protected float _TimeOutOfSight;

            protected float _TimeOutOfSightMax;

            public ReturnBaseState(Falcon falcon) {
                _TimeOutOfSightMax = Screen.ScreenWidth / falcon.speed;
            }

            public override MoveState Enter(Falcon falcon) {
                _FlyingDirection = new(-Math.Sign(falcon.transform.localScale.x), 0); // keep direction
                return this;
            }

            public override bool UpdateState(Falcon falcon, out Vector3 deltaPos) {
                deltaPos = new(falcon.speed * Time.deltaTime * _FlyingDirection.x, 0);
                // only check one end since we know in which direction we are flying.
                bool onScreen = _FlyingDirection.x > 0
                    ? GameState.Instance.GetCurrentScreen().PositionInScreen(new Vector3(falcon.transform.position.x - falcon.extentBack, falcon.transform.position.y, 0))
                    : GameState.Instance.GetCurrentScreen().PositionInScreen(new Vector3(falcon.transform.position.x + falcon.extentBack, falcon.transform.position.y, 0));

                if (!onScreen && !_HasLeftScreen) {
                    _HasLeftScreen = true;
                    _TimeOutOfSight = 0;
                }
                else if (!onScreen) {
                    _TimeOutOfSight += Time.deltaTime;
                }
                else {
                    _HasLeftScreen = false;
                }

                return _TimeOutOfSight > _TimeOutOfSightMax;
            }
        }

        /// <summary>
        /// State of safety for children while Falcon is busy eating the mouse or whatever
        /// </summary>
        private class DeactivatedState : PassiveState {
            private float _waitedTime;
            public float WaitTime { get; set; }

            public override PassiveState Enter() {
                return this;
            }

            public override bool UpdateState() {
                _waitedTime += Time.deltaTime;
                return _waitedTime > WaitTime;
            }
        }

        /// <summary>
        /// Fly towards target and follow target around / wait above target.
        /// Only moves until target reached, then waits until target has reached some distance, to avoid glitching around. 
        /// </summary>
        private class ObserveState : MoveToTargetState {
            private const float ShortTriggerDistance = 0.2f; // value determined manually by testing what avoids glitching best
            private const float LongTriggerDistance = 4f;

            private readonly GameObject _targetObject;
            private Vector2 _flyingDirection;
            private bool _close;
            private bool _wasCloseOnce;
            private float _waitedTime;
            private readonly float _waitTime;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="target"></param>
            /// <param name="waitTime">Time to wait after reaching the target (as determined by <see cref="ShortTriggerDistance"/>)</param>
            public ObserveState(GameObject target, float waitTime) : base(target.transform.position) {
                _targetObject = target;
                _waitTime = waitTime;
            }

            public override MoveState Enter(Falcon falcon) {
                _flyingDirection = new(Math.Sign(currentTarget.x - falcon.transform.position.x), 0);
                return this;
            }

            public override bool UpdateState(Falcon falcon, out Vector3 deltaPos) {
                float distance = _targetObject.transform.position.x - falcon.transform.position.x;

                if (Math.Abs(distance) <= ShortTriggerDistance) {
                    // if we are inside ShortTriggerDistance, stop
                    _close = true;
                    _wasCloseOnce = true; // _close will probably never be set to false again but who knows. Start timer
                }
                else if (Math.Abs(distance) > LongTriggerDistance) {
                    // if we are not even inside LongTriggerDistance, start moving
                    _close = false;
                }
                if (!_close) {
                    currentTarget = _targetObject.transform.position;
                }

                _flyingDirection = new(Math.Sign(distance), 0);
                deltaPos = _close ? Vector3.zero : new(falcon.speed * Time.deltaTime * _flyingDirection.x, 0);

                // TODO change facing direction
                // TODO angry craaah craaah here

                if (_wasCloseOnce) {
                    _waitedTime += Time.deltaTime;
                    return _waitedTime > _waitTime;
                }

                return false;
            }
        }

        /// <summary>
        /// Fly off super fast until witch is reached.
        /// Needs to be fast enough to always reach witch before children leave (or fake it because bird is magical).
        /// Very similar to <see cref="ReturnBaseState"/> but with higher speed and always flying towards witch house.
        /// </summary>
        private class TellWitchState : ReturnBaseState {
            public TellWitchState(Falcon falcon) : base(falcon) {
                _TimeOutOfSightMax = 7; // the kids have to walk at least 2 screens of 7.11 m at 1.7 m/s, that takes 8.4 s
            }

            public override MoveState Enter(Falcon falcon) {
                _FlyingDirection = new(1, 0); // towards with house
                Vector3 scale = falcon.transform.localScale;
                scale.x = -1; // make Falcon look the right way
                falcon.transform.localScale = scale;
                return this;
            }

            public override bool UpdateState(Falcon falcon, out Vector3 deltaPos) {
                // First get return value from base function
                bool timeUp = base.UpdateState(falcon, out deltaPos);
                // Then override out value to be able to override speed
                deltaPos = new(falcon.speed * 2 * Time.deltaTime * _FlyingDirection.x, 0);

                return timeUp;
                // TODO: Also fulfull condition when the witch hut was reached before the time was up. It's dangerous being close to the witch!
            }

            public override void Exit() {
            }
        }

        #endregion

        #region Properties

        [Header("Patrouille")]
        public float leftEnd;
        public float rightEnd;
        public float speed = 3f;

        [field: SerializeField]
        public float SafeTimeAfterCapture { get; private set; }

        [field: SerializeField]
        public Vector3 RespawnPoint { get; private set; }

        [Header("Config")]
        public float extentBack;
        public float extendFront;
        [Tooltip("Child object that contains the colliders and sprites")]
        public GameObject subFalcon;

        [SerializeField] private Animator animator;
        [ShowInInspector] private State _currentState;
        private Mouse _currentMouse;
        private Character _currentCharacter;

        #endregion

        public event Action NotifiedWitch;

        private void Start() {
            _currentState = new PatrolState {
                leftEnd = leftEnd,
                rightEnd = rightEnd
            };
        }

        private void Update() {
            bool stateDone = false;
            if (_currentState is MoveState moveState) {
                stateDone = moveState.UpdateState(this, out Vector3 deltaPos);
                transform.position += deltaPos;
            }
            else if (_currentState is PassiveState passiveState) {
                stateDone = passiveState.UpdateState();
            }

            if (stateDone) {
                _currentState.Done?.Invoke();
            }
        }

        /// <summary>
        /// Find things. Detect mouse and children (player) colliders and start chains of states. 
        /// </summary>
        /// <seealso cref="OnMouseApproached"/>
        private void OnTriggerEnter2D(Collider2D other) {
            if (other.CompareTag("Mouse")) {
                Mouse mouse = other.GetComponent<Mouse>();
                if (mouse == null) {
                    mouse = other.GetComponentInParent<Mouse>();
                }

                if (mouse != null && mouse.mouseActive && !mouse._moving && _currentMouse == null) {
                    Debug.Log("SPOTTED MOUSE " + mouse.gameObject.name + " in " + mouse.transform.parent.gameObject.name);
                    _currentState.Exit();
                    _currentState = new ApproachingState(mouse.gameObject.transform.position + new Vector3(0.155f, 0.431f, 0)) {
                        // value determined by hand based on what looks good {
                        Done = OnMouseApproached
                    }.Enter(this);
                    _currentMouse = mouse;
                }
            }
            else if (other.CompareTag("Player") && !GameState.Instance.kidsAreSafe) {
                Character character = other.GetComponent<Character>();
                if (character == null) {
                    character = other.transform.parent.parent.GetComponent<Character>();
                }
                if (character == null) {
                    return; // TODO error handling
                }
                if (_currentCharacter == null) {
                    _currentCharacter = character;

                    Debug.Log("Found " + character.gameObject.name);

                    _currentState.Exit();
                    _currentState = new ObserveState(character.gameObject, 1) {
                        Done = () => {
                            _currentState.Exit();
                            _currentState = new TellWitchState(this) {
                                Done = () => {
                                    Debug.Log("NOW THE WITCH KNOWS");
                                    _currentState = null;
                                    NotifiedWitch?.Invoke();
                                }
                            }.Enter(this);
                        }
                    }.Enter(this);
                }
            }
        }

        /// <summary>
        /// Initiate mouse chain
        /// </summary>
        private void OnMouseApproached() {
            _currentState.Exit();
            _currentState = new AttackLandingState(_currentMouse.gameObject.transform.position + new Vector3(0.155f, 0.431f, 0)) {
                animator = animator,
                Done = () => {
                    _currentState.Exit();
                    StartCoroutine(KillMouseAfterFrame());
                    _currentState = new OnFloorState {
                        animator = animator,
                        Done = () => {
                            _currentState.Exit();
                            _currentState = new RisingState(this) {
                                animator = animator,
                                Done = () => {
                                    _currentState = new ReturnBaseState(this) {
                                        animator = animator,
                                        Done = () => {
                                            print("I'm gone");
                                            subFalcon.SetActive(false);
                                            _currentState = new DeactivatedState {
                                                WaitTime = SafeTimeAfterCapture - Screen.ScreenWidth / speed, // reduce by time Falcon continues to fly out of sight in ReturnBaseState
                                                Done = () => {
                                                    print("respawn falcon");
                                                    transform.position = RespawnPoint;
                                                    subFalcon.SetActive(true);
                                                    animator.ResetControllerState();
                                                    _currentState = new PatrolState {
                                                        leftEnd = leftEnd,
                                                        rightEnd = rightEnd
                                                    };
                                                }
                                            }.Enter();
                                        }
                                    }.Enter(this);
                                }
                            }.Enter(this);
                        }
                    }.Enter();
                }
            }.Enter(this);
        }

        private IEnumerator KillMouseAfterFrame() {
            yield return new WaitForSeconds(0);
            Destroy(_currentMouse.gameObject, 0);
            _currentMouse = null;
        }
    }
}
