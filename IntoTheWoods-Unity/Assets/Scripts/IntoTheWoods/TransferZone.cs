using UnityEngine;

namespace IntoTheWoods {
    public class TransferZone : MonoBehaviour {
        /// <summary>
        /// Back lane transfer zones must be higher than this.
        /// Front lane transfer zones must be lower than this.
        /// </summary>
        public const float BackFrontThreshold = -0.8f;

        public Transform partner;
    }
}
