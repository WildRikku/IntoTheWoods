using IntoTheWoods.Characters;
using UnityEngine;

namespace IntoTheWoods.Helpers {
    public class DoNotCross : MonoBehaviour {
        private void OnTriggerEnter2D(Collider2D other) {
            Walker walker = other.GetComponentInParent<Walker>();
            if (walker != null && !walker.IsTransfering) {
                walker.StopWalking();
            }
        }
    }
}
