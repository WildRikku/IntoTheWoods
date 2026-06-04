using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace IntoTheWoods {
    public delegate bool PlayerWillMoveEventHandler(Player sender, Vector2 direction);

    public class Player : MonoBehaviour, InputSystem_Actions.IPlayerActions {
        // Cached property indices for animator for efficiency
        private static readonly int Walking = Animator.StringToHash("walking");
        private static readonly int Pickup = Animator.StringToHash("pickup");

        // configurable fields
        [SerializeField] private float speed = 1.7f;
        [SerializeField] private int jumpForce = 125;

        // setup fields
        [SerializeField] private Animator animator;

        // internal setup fields
        private @InputSystem_Actions _wrapper;
        private Rigidbody2D _rigidbody;

        // states
        private bool _walking;
        private Vector2 _walkingDirection;

        // events
        public event PlayerWillMoveEventHandler WillMove;

        private void Start() {
            _rigidbody = GetComponent<Rigidbody2D>();
            Assert.IsNotNull(_rigidbody);

            _wrapper = new();
            _wrapper.Player.AddCallbacks(this);
            _wrapper.Enable();

            Assert.IsNotNull(animator);
        }

        private void Update() {
            if (_walking) {
                // By sending the event before moving, we have the game manager check if we are going out of bounds
                // the order is relevant to prevent glitching outside
                if (!WillMove?.Invoke(this, _walkingDirection) ?? false) {
                    _walking = false;
                    return;
                }

                // zeroing out y axis since that only makes sense for ladders etc
                transform.position += new Vector3(speed * Time.deltaTime * _walkingDirection.x, 0);
            }
        }

        public void OnMove(InputAction.CallbackContext context) {
            if (context.started && !animator.GetBool(Pickup)) {
                // grab input
                Vector2 moveVector = DigitizeMovement(context.ReadValue<Vector2>());
                if (moveVector.x == 0) {
                    return;
                }

                // update scale (flip facing direction)
                Vector3 scale = transform.localScale;
                scale.x = moveVector.x;
                transform.localScale = scale;

                // activate walking
                _walking = true;
                _walkingDirection = moveVector;
                animator.SetBool(Walking, true);
            }
            else if (context.canceled) {
                StopWalking();
            }
            // TODO: check if direction has changed
        }

        public void OnAttack(InputAction.CallbackContext context) {
        }

        public void OnInteract(InputAction.CallbackContext context) {
            if (context.performed) {
                StopWalking();
                animator.SetBool(Pickup, true); // will reset itself
            }
        }

        public void OnCrouch(InputAction.CallbackContext context) {
        }

        public void OnJump(InputAction.CallbackContext context) {
            if (context.started) {
                _rigidbody.AddForceY(jumpForce, ForceMode2D.Impulse);
            }
        }

        public void OnPrevious(InputAction.CallbackContext context) {
        }

        public void OnNext(InputAction.CallbackContext context) {
        }

        public void OnSprint(InputAction.CallbackContext context) {
        }

        /// <summary>
        /// Our game does not support analogue movement, so sanitize values from joysticks.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private Vector2 DigitizeMovement(Vector2 input) {
            float x = input.x > 0 ? 1 : input.x < 0 ? -1 : 0;
            float y = input.y > 0 ? 1 : input.y < 0 ? -1 : 0;
            return new(x, y);
        }

        private void StopWalking() {
            _walking = false;
            animator.SetBool(Walking, false);
        }
    }
}
