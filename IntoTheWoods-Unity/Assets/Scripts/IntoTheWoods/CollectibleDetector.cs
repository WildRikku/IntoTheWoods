using System;
using UnityEngine;

namespace IntoTheWoods {
    public class CollectibleDetector : MonoBehaviour {
        public event Action<Collider2D> CollectibleDetected;
        public event Action<Collider2D> CollectibleLost;

        private void OnTriggerEnter2D(Collider2D other) {
            if (other.CompareTag("Collectible")) {
                CollectibleDetected?.Invoke(other);
            }
        }

        private void OnTriggerExit2D(Collider2D other) {
            if (other.CompareTag("Collectible")) {
                CollectibleLost?.Invoke(other);
            }
        }
    }
}
