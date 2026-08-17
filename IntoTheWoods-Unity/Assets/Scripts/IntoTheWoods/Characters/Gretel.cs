using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

namespace IntoTheWoods.Characters {
    public class Gretel : Character, InputSystem_Actions.IGretelActions {
        // Cached property indices for animator for efficiency
        private static readonly int Pickup = Animator.StringToHash("pickup");

        // setup fields
        [SerializeField] private Animator animator;
        [SerializeField] private Inventory inventory;

        // internal setup fields
        private @InputSystem_Actions _wrapper;
        private CollectibleDetector _collectibleDetector;
        private AnimatorSignal _animatorSignal;

        // interactions
        private Dictionary<int, GameObject> _nearbyCollectibles;

        private void Awake() {
            _wrapper = new();
            _wrapper.Gretel.AddCallbacks(this);
            _wrapper.Enable();

            Assert.IsNotNull(animator);
            Assert.IsNotNull(inventory);

            _nearbyCollectibles = new();

            _collectibleDetector = GetComponentInChildren<CollectibleDetector>();
            _animatorSignal = GetComponentInChildren<AnimatorSignal>();
        }

        private void OnEnable() {
            if (_collectibleDetector != null) {
                _collectibleDetector.CollectibleDetected += OnCollectibleDetected;
                _collectibleDetector.CollectibleLost += OnCollectibleLost;
            }

            if (_animatorSignal != null) {
                _animatorSignal.AnimationEnded += OnAnimationEnded;
            }
        }

        private void OnDisable() {
            if (_collectibleDetector != null) {
                _collectibleDetector.CollectibleDetected -= OnCollectibleDetected;
                _collectibleDetector.CollectibleLost -= OnCollectibleLost;
            }

            if (_animatorSignal != null) {
                _animatorSignal.AnimationEnded -= OnAnimationEnded;
            }
        }

        public void OnPickup(InputAction.CallbackContext context) {
            if (!walker.IsTransfering && _nearbyCollectibles.Count > 0 && context.performed) {
                walker.StopWalking();
                // animation resets itself
                // animation also triggers the actual pickup via event
                animator.SetBool(Pickup, true);
            }
        }

        private void OnCollectibleDetected(Collider2D obj) {
            _nearbyCollectibles.TryAdd(obj.gameObject.GetInstanceID(), obj.gameObject);
            // Debug.Log($"Hello {obj.gameObject.name}");
        }

        private void OnCollectibleLost(Collider2D obj) {
            _nearbyCollectibles.Remove(obj.gameObject.GetInstanceID());
            // Debug.Log($"Bye {obj.gameObject.name}");
        }

        private void OnAnimationEnded() {
            foreach ((int _, GameObject value) in _nearbyCollectibles) {
                Collectible c = value.GetComponent<Collectible>();
                if (c == null) {
                    continue;
                }

                c.PickUp();
                inventory.AddCollectible(c);
            }

            _nearbyCollectibles.Clear();
        }

        public override bool IsBusy() {
            return animator.GetBool(Pickup);
        }
    }
}
