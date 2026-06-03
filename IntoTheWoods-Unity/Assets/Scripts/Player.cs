using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DefaultNamespace {
    public class Player : MonoBehaviour, InputSystem_Actions.IPlayerActions {
        private @InputSystem_Actions _wrapper;

        private void Start() {
            _wrapper = new();
            _wrapper.Player.AddCallbacks(this);
            _wrapper.Enable();
        }

        public void OnMove(InputAction.CallbackContext context) {
            Debug.Log("HARRR");
        }

        public void OnLook(InputAction.CallbackContext context) {
        }

        public void OnAttack(InputAction.CallbackContext context) {
        }

        public void OnInteract(InputAction.CallbackContext context) {
        }

        public void OnCrouch(InputAction.CallbackContext context) {
        }

        public void OnJump(InputAction.CallbackContext context) {
        }

        public void OnPrevious(InputAction.CallbackContext context) {
        }

        public void OnNext(InputAction.CallbackContext context) {
        }

        public void OnSprint(InputAction.CallbackContext context) {
        }
    }
}
