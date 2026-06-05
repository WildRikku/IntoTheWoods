using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace IntoTheWoods.Characters {
    public delegate bool PlayerWillMoveEventHandler(Walker sender, Vector2 direction);

    public class Player : Walker, InputSystem_Actions.IPrimaryPlayerActions {
        // Cached property indices for animator for efficiency
        private static readonly int Pickup = Animator.StringToHash("pickup");

        // internal setup fields
        private Animator _gretelAnimator;
        private Animator _haenselAnimator;
        private @InputSystem_Actions _wrapper;

        public void Init(Animator gretelAnimator, Animator haenselAnimator) {
            _gretelAnimator = gretelAnimator;
            _haenselAnimator = haenselAnimator;
        }

        private void Start() {
            _wrapper = new();
            _wrapper.PrimaryPlayer.AddCallbacks(this);
            _wrapper.Enable();

            Assert.IsNotNull(_gretelAnimator);
        }

        public void OnMove(InputAction.CallbackContext context) {
            if (context.started && !_gretelAnimator.GetBool(Pickup)) {
                // grab input
                Vector2 moveVector = DigitizeMovement(context.ReadValue<Vector2>());
                if (moveVector.x == 0) {
                    return;
                }

                // update scale (flip facing direction)
                Vector3 scale = transform.localScale;
                scale.x = moveVector.x;
                transform.localScale = scale;

                ActivateWalking(moveVector);
            }
            else if (context.canceled) {
                StopWalking();
            }
        }

        public void OnCrouch(InputAction.CallbackContext context) {
        }

        public void OnJump(InputAction.CallbackContext context) {
            if (context.started) {
                rb2D.AddForceY(jumpForce, ForceMode2D.Impulse);
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
    }
}
