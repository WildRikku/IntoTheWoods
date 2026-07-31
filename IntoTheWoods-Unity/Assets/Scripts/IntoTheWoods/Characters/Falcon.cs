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

        public class IdleState : MoveState {
            private Vector2 _flyingDirection = new(-1, 0);
            public float leftEnd;
            public float rightEnd;

            public override MoveState Enter(Falcon falcon) {
                return this;
            }

            public override bool UpdateState(Falcon falcon, out Vector3 deltaPos) {
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

            public override void Exit() {
            }
        }

        public class ApproachingState : MoveState {
            private Vector2 _flyingDirection;

            public override MoveState Enter(Falcon falcon) {
                _flyingDirection = new(-Math.Sign(falcon.transform.localScale.x), 0); // keep direction
                return this;
            }

            public override bool UpdateState(Falcon falcon, out Vector3 deltaPos) {
                deltaPos = new(falcon.speed * Time.deltaTime * _flyingDirection.x, 0);
                return Math.Abs((currentTarget - falcon.transform.position).x) <= ApproachDistance;
            }

            public override void Exit() {
            }
        }

        public class AttackLandingState : MoveState {
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

        public class OnFloorState : PassiveState {
            private float _waitedTime;

            public override PassiveState Enter() {
                animator.SetBool(Landed, true);
                _waitedTime = 0;
                // StartCoroutine(KillMouseAfterFrame()); // TODO only possible in monobehaviour
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

        public class RisingState : MoveState {
            public override MoveState Enter(Falcon falcon) {
                animator.SetBool(Rising, true);
                currentTarget = new(falcon.transform.position.x + 5 * -Math.Sign(falcon.transform.localScale.x), DefaultHeight, 0); // TODO do not hardcode x distance (use approach distance?)
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

        public class ReturnBaseState : MoveState {
            private Vector2 _flyingDirection;

            public override MoveState Enter(Falcon falcon) {
                _flyingDirection = new(-Math.Sign(falcon.transform.localScale.x), 0); // keep direction
                return this;
            }

            public override bool UpdateState(Falcon falcon, out Vector3 deltaPos) {
                deltaPos = new(falcon.speed * Time.deltaTime * _flyingDirection.x, 0);
                return false; // TODO
            }

            public override void Exit() {
            }
        }

        public class ObserveState : MoveState {
            public override MoveState Enter(Falcon falcon) {
                throw new NotImplementedException();
            }

            public override bool UpdateState(Falcon falcon, out Vector3 deltaPos) {
                throw new NotImplementedException();
            }

            public override void Exit() {
                throw new NotImplementedException();
            }
        }

        public class TellWitchState : MoveState {
            public override MoveState Enter(Falcon falcon) {
                throw new NotImplementedException();
            }

            public override bool UpdateState(Falcon falcon, out Vector3 deltaPos) {
                throw new NotImplementedException();
            }

            public override void Exit() {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region Properties

        [Header("Patrouille")]
        public float leftEnd;
        public float rightEnd;
        public float speed = 3f;

        [SerializeField] private Animator animator;
        [ShowInInspector] private State _currentState;
        private Mouse _currentMouse;

        #endregion

        private void Start() {
            _currentState = new IdleState {
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
                _currentState.Done.Invoke();
            }
        }

        /// <summary>
        /// Find things
        /// </summary>
        /// <param name="other"></param>
        private void OnTriggerEnter2D(Collider2D other) {
            if (other.CompareTag("Mouse")) {
                Mouse mouse = other.GetComponent<Mouse>();
                if (mouse == null) {
                    mouse = other.GetComponentInParent<Mouse>();
                }

                if (mouse != null && mouse.mouseActive && !mouse._moving && _currentMouse == null) {
                    Debug.Log("SPOTTED MOUSE " + mouse.gameObject.name + " in " + mouse.transform.parent.gameObject.name);
                    _currentState.Exit();
                    _currentState = new ApproachingState {
                        currentTarget = mouse.gameObject.transform.position + new Vector3(0.155f, 0.431f, 0), // value determined by hand based on what looks good
                        Done = OnMouseApproached
                    }.Enter(this);
                    _currentMouse = mouse;
                }
            }
            else if (other.CompareTag("Player")) {
                Character character = other.GetComponent<Character>();
                if (character == null) {
                    character = other.transform.parent.parent.GetComponent<Character>();
                }

                Debug.Log("Found " + character.gameObject.name);
            }
        }

        /// <summary>
        /// Initiate mouse chain
        /// </summary>
        private void OnMouseApproached() {
            _currentState.Exit();
            _currentState = new AttackLandingState {
                animator = animator,
                currentTarget = _currentMouse.gameObject.transform.position + new Vector3(0.155f, 0.431f, 0),
                Done = () => {
                    _currentState.Exit();
                    StartCoroutine(KillMouseAfterFrame());
                    _currentState = new OnFloorState {
                        animator = animator,
                        Done = () => {
                            _currentState.Exit();
                            _currentState = new RisingState {
                                animator = animator,
                                Done = () => {
                                    _currentState = new ReturnBaseState {
                                        animator = animator
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
