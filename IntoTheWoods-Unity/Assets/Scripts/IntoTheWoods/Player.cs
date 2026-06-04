using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

namespace IntoTheWoods {
    public class Player : MonoBehaviour, InputSystem_Actions.IPlayerActions {
        private @InputSystem_Actions _wrapper;
        private Rigidbody2D _rigidbody;
        [SerializeField] private float speed;
        [SerializeField] private int jumpForce = 125;

        private bool _walking;
        private Vector2 _walkingDirection;

        private void Start() {
            _rigidbody = GetComponent<Rigidbody2D>();
            Assert.IsNotNull(_rigidbody);
            _wrapper = new();
            _wrapper.Player.AddCallbacks(this);
            _wrapper.Enable();
        }

        private void Update() {
            if (_walking) {
                // zeroing out y axis since that only makes sense for ladders etc
                transform.position += new Vector3(speed * Time.deltaTime * _walkingDirection.x, 0);
            }
        }

        public void OnMove(InputAction.CallbackContext context) {
            Vector2 moveVector = context.ReadValue<Vector2>();
            if (context.started) {
                _walking = true;
                _walkingDirection = moveVector;
            }
            else if (context.canceled) {
                _walking = false;
            }
            // TODO: check if direction has changed
        }

        public void OnAttack(InputAction.CallbackContext context) {
        }

        public void OnInteract(InputAction.CallbackContext context) {
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
    }
}
