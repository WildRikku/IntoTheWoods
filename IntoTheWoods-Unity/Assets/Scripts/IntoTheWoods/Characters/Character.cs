using UnityEngine;
using UnityEngine.Assertions;

namespace IntoTheWoods.Characters {
    public class Character : MonoBehaviour {
        protected Walker walker;

        private void Start() {
            walker = GetComponent<Walker>();
            Assert.IsNotNull(walker);
        }
    }
}
