using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

namespace IntoTheWoods.Characters {
    public class Haensel : Character, InputSystem_Actions.IHaenselActions {
        // Cached property indices for animator for efficiency
        private static readonly int Throwing = Animator.StringToHash("throwing");

        // setup fields
        [SerializeField] private Animator animator;

        // internal setup fields
        private @InputSystem_Actions _wrapper;

        private void Awake() {
            _wrapper = new();
            _wrapper.Haensel.AddCallbacks(this);
            _wrapper.Enable();

            Assert.IsNotNull(animator);
        }

        public void OnAttack(InputAction.CallbackContext context) {
            if (context.performed) {
                animator.SetBool(Throwing, true); // will reset itself
            }
        }
    }
}
