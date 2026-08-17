using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

namespace IntoTheWoods.Characters {
    public class Haensel : Character, InputSystem_Actions.IHaenselActions {
        // Cached property indices for animator for efficiency
        private static readonly int Throwing = Animator.StringToHash("throwing");

        // setup fields
        [SerializeField] private Animator animator;
        [SerializeField] private Inventory inventory;

        // internal setup fields
        private @InputSystem_Actions _wrapper;

        private void Awake() {
            _wrapper = new();
            _wrapper.Haensel.AddCallbacks(this);
            _wrapper.Enable();

            Assert.IsNotNull(animator);
        }

        public void OnThrowBread(InputAction.CallbackContext context) {
            // note that the order matters because TryGetBread() will reduce bread count
            if (context.performed && inventory.TryGetBread()) {
                animator.SetBool(Throwing, true); // will reset itself
            }
        }

        public override bool IsBusy() {
            return animator.GetBool(Throwing);
        }
    }
}
