using System.Collections.Generic;
using DefaultNamespace;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable UnusedMember.Global - used in UI
// ReSharper disable MemberCanBePrivate.Global - used in UI

namespace IntoTheWoods {
    public class Inventory : MonoBehaviour {
        private List<Collectible> _stones;
        [SerializeField] private List<Sprite> breadSprites;
        [SerializeField] private List<Sprite> stoneSprites;

        [CreateProperty]
        // ReSharper disable once MemberCanBePrivate.Global - used in UI
        public int BreadCount { get; private set; } = 4;

        public bool hasNet;

        private void Awake() {
            _stones = new();
        }

        public void AddCollectible(Collectible collectible) {
            if (collectible is StoneCollectible) {
                _stones.Add(collectible);
                if (_stones.Count > 1) {
                    // make next one come to top for UI - from 2nd one, first one is show at all
                    stoneSprites.RemoveAt(0);
                }
            }
            else if (collectible is NetCollectible) {
                hasNet = true;
            }
        }

        public bool TryGetBread() {
            if (BreadCount > 0) {
                BreadCount--;
                breadSprites.RemoveAt(0); // make next one come to top for UI
                return true;
            }

            return false;
        }

        [CreateProperty]
        public int StoneCount => _stones.Count;

        [CreateProperty]
        public StyleEnum<Visibility> HasStones => StoneCount > 0 ? Visibility.Visible : Visibility.Hidden;
        
        [CreateProperty]
        public StyleEnum<Visibility> HasNet => hasNet ? Visibility.Visible : Visibility.Hidden;
    }
}
