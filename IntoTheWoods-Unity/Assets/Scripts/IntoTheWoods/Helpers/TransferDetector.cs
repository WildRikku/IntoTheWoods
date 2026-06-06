using System;
using UnityEngine;

namespace IntoTheWoods {
    public class TransferDetector : MonoBehaviour {
        public event Action<Collider2D> TransferZoneEntered;
        public event Action<Collider2D> TransferZoneLeft;

        private void OnTriggerEnter2D(Collider2D other) {
            if (other.CompareTag("TransferZone")) {
                TransferZoneEntered?.Invoke(other);
            }
        }

        private void OnTriggerExit2D(Collider2D other) {
            if (other.CompareTag("TransferZone")) {
                TransferZoneLeft?.Invoke(other);
            }
        }
    }
}
