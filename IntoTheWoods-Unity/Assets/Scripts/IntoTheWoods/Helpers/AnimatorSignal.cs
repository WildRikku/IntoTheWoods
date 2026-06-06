using System;
using UnityEngine;

namespace IntoTheWoods {
    public class AnimatorSignal : MonoBehaviour {
        public event Action AnimationEnded;

        /// <summary>
        /// Throws <see cref="AnimationEnded"/> at the correct point in the animation.
        /// </summary>
        public void TriggerAnimationEndSignal() {
            AnimationEnded?.Invoke();
        }
    }
}
