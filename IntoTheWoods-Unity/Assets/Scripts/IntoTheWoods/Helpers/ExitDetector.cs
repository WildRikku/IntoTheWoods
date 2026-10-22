using System;
using UnityEngine;

namespace IntoTheWoods.Helpers {
    public class ExitDetector : MonoBehaviour {
        public event Action ExitReached;

        private void OnTriggerEnter2D(Collider2D other) {
            ExitReached?.Invoke();
        }
    }
}
