using System;
using UnityEngine;

namespace IntoTheWoods {
    public class StepAudio : MonoBehaviour {
        public event Action AnimationStep;

        /// <summary>
        /// Throws <see cref="AnimationStep"/> at the correct point in the animation....
        /// </summary>
        public void TriggerAnimationStepSignal() {
            AnimationStep?.Invoke();
        }
    }
}
