using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;

namespace IntoTheWoods {
    public class Inventory : MonoBehaviour {
        private List<Collectible> _stones;

        [CreateProperty]
        // ReSharper disable once MemberCanBePrivate.Global - used in UI
        public int BreadCount { get; private set; } = 4;

        private void Awake() {
            _stones = new();
        }

        public void AddCollectible(Collectible collectible) {
            _stones.Add(collectible);
        }

        public bool TryGetBread() {
            if (BreadCount > 0) {
                BreadCount--;
                return true;
            }

            return false;
        }

        [CreateProperty]
        // ReSharper disable once UnusedMember.Global - used in UI
        public int StoneCount => _stones.Count;
    }
}
