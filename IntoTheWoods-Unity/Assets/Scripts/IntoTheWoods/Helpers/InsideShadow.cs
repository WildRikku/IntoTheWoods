using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace IntoTheWoods.Helpers {
    public class InsideShadow : MonoBehaviour {
        [ShowInInspector] public Dictionary<string, bool> capturedHaensel = new();
        [ShowInInspector] public Dictionary<string, bool> capturedGretel = new();

        private void OnTriggerEnter2D(Collider2D other) {
            if (!(other.CompareTag("Haensel") || other.CompareTag("Gretel"))) {
                return;
            }

            if (other.CompareTag("Haensel")) {
                capturedHaensel[other.gameObject.name + other.gameObject.GetInstanceID()] = true;
            }
            else {
                capturedGretel[other.gameObject.name + other.gameObject.GetInstanceID()] = true;
            }
        }

        private void OnTriggerExit2D(Collider2D other) {
            if (!(other.CompareTag("Haensel") || other.CompareTag("Gretel"))) {
                return;
            }

            if (other.CompareTag("Haensel")) {
                capturedHaensel.Remove(other.gameObject.name + other.gameObject.GetInstanceID());
            }
            else {
                capturedGretel.Remove(other.gameObject.name + other.gameObject.GetInstanceID());
            }
        }
    }
}
