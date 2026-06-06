using System;
using UnityEngine;

namespace IntoTheWoods {
    public class Pickup : MonoBehaviour {
        public event Action PickingUp;

        /// <summary>
        /// Throws <see cref="PickingUp"/> at the correct point in the animation.
        /// </summary>
        public void TriggerPickup() {
            PickingUp?.Invoke();
        }
    }
}
