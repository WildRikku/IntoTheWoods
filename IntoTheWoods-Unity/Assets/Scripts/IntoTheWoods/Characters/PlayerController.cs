using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Vector2 = UnityEngine.Vector2;

// ReSharper disable Unity.PreferNonAllocApi

namespace IntoTheWoods.Characters {
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
            void WalkerOnTransfered() {
                _walker.Transfered -= WalkerOnTransfered;
                ProcessMoving(context);
            }

            if (_character.IsBusy() || GameState.Instance.kidsAreDoomed) {
                // throwing, picking up, or game is over
                return;
            }

            if (context.started) {
                if (_walker.IsBusy()) {
                    // transfering, wait
                    _walker.Transfered += WalkerOnTransfered;
                    return;
                }

                ProcessMoving(context);
            }
            else if (context.canceled) {
                if (_walker.IsBusy()) {
                    // KeyUp while still transfering
                    _walker.Transfered -= WalkerOnTransfered;
                }
                else {
                    _walker.StopWalking();
                }
            }
        }

        public void OnJump(InputAction.CallbackContext context) {
            if (context.started) {
                // rb2D.AddForceY(jumpForce, ForceMode2D.Impulse);
            }
        }

        public void OnChangeLane(InputAction.CallbackContext context) {
            if (!_walker.IsBusy() && !_character.IsBusy() && _walker.CanTransfer && context.started && !GameState.Instance.kidsAreDoomed) {
                // grab input
                Vector2 inputVector = DigitizeMovement(context.ReadValue<Vector2>());
                if (inputVector.y == 0) {
                    return;
                }

                _walker.ActivateTransfer(inputVector, true);
            }
        }

        public void OnUseNet(InputAction.CallbackContext context) {
            if (context.started && _character.inventory.hasNet && !GameState.Instance.kidsAreDoomed) {
                RaycastHit2D hit = Physics2D.CircleCast(transform.position, 2.5f, Vector2.down, 0.5f, LayerMask.GetMask("Falcon"));
                if (!hit) {
                    Debug.Log("no hit");
                    return;
                }
                Falcon falcon = hit.collider.GetComponent<Falcon>();
                if (falcon == null) {
                    falcon = hit.collider.GetComponentInParent<Falcon>();
                }
                if (falcon != null) {
                    if (falcon.TryCapture(_character.inventory.StonesOnNet)) {
                        Debug.Log("gotcha falcon");
                    }
                    else {
                        Debug.Log("capture attempt failed");
                    }
                }
                else {
                    Debug.Log("no falcon " + hit.collider.gameObject.name);
                }
            }
        }

        public void OnClick(InputAction.CallbackContext context) {
            if (!GameState.Instance.gameOver || !context.performed) {
                return;
            }

            GoToMainMenu();
        }

        public void OnMenu(InputAction.CallbackContext context) {
            if (context.performed) {
                GoToMainMenu();
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

        private void ProcessMoving(InputAction.CallbackContext context) {
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

        private void GoToMainMenu() {
            // return to main menu
            SceneManager.LoadScene("MainMenu");
        }
    }
}
