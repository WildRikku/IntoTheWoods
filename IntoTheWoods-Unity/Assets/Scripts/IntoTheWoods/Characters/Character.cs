using UnityEngine;
using UnityEngine.Assertions;

namespace IntoTheWoods.Characters {
    public abstract class Character : MonoBehaviour {
        [SerializeField] protected Walker walker;
        public Inventory inventory;
        public new GameObject particleSystem;
        public GameObject characterContainer;

        private void Start() {
            Assert.IsNotNull(walker);
        }

        public abstract bool IsBusy();
    }
}
