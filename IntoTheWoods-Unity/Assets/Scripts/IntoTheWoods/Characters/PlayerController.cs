using UnityEngine;
using UnityEngine.InputSystem;
using Vector2 = UnityEngine.Vector2;
using UnityEngine.Rendering;

namespace IntoTheWoods.Characters {
    public delegate bool PlayerWillMoveEventHandler(Vector2 nextPosition, Vector2 direction);

    /// <summary>
    /// Human control on top of a <see cref="Walker"/>.
    /// </summary>
    public class PlayerController : MonoBehaviour, InputSystem_Actions.IPrimaryPlayerActions {
        // internal setup fields
        private Walker _walker;
        private Character _character;
        private @InputSystem_Actions _wrapper;

        private void Start() {
            // init controls
            _wrapper = new();
            _wrapper.PrimaryPlayer.AddCallbacks(this);
            _wrapper.Enable();

            // get components dynamically since PlayerController is instantiated at runtime
            _walker = GetComponent<Walker>();
            _character = GetComponent<Character>();
        }

        public void OnMove(InputAction.CallbackContext context) {
            if (_walker.IsBusy() || _character.IsBusy() || GameState.Instance.kidsAreDoomed) {
                // walking is only allowed when not busy, so if busy, process neither starting nor stopping walking
                return;
            }

            if (context.started) {
                // grab input
                Vector2 moveVector = DigitizeMovement(context.ReadValue<Vector2>());
                if (moveVector.x == 0) {
                    return;
                }

                // remove y component in case a joystick was used.
                // This is relevant for distinguishing between player-controled movement and animated movement to background/foreground
                moveVector.y = 0;

                _walker.ActivateWalking(moveVector);
            }
            else if (context.canceled) {
                _walker.StopWalking();
            }
        }

        public void OnJump(InputAction.CallbackContext context) {
            if (context.started) {
                // rb2D.AddForceY(jumpForce, ForceMode2D.Impulse);
            }
        }

        public void OnChangeLane(InputAction.CallbackContext context) {
            if (!_walker.IsBusy() && !_character.IsBusy() && _walker.CanTransfer && context.started) {
                // grab input
                Vector2 inputVector = DigitizeMovement(context.ReadValue<Vector2>());
                if (inputVector.y == 0) {
                    return;
                }

                _walker.ActivateTransfer(inputVector, true);
            }
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
