using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace IntoTheWoods.Helpers {
    public class InsideShadow : MonoBehaviour {
        [ShowInInspector] public Dictionary<string, bool> captured = new();

        private void OnTriggerEnter2D(Collider2D other) {
            if (!other.CompareTag("Player")) {
                return;
            }

            captured[other.gameObject.name + other.gameObject.GetInstanceID()] = true;
        }

        private void OnTriggerExit2D(Collider2D other) {
            if (!other.CompareTag("Player")) {
                return;
            }

            captured.Remove(other.gameObject.name + other.gameObject.GetInstanceID());
        }
    }
}
