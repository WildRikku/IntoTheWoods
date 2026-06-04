using System;
using System.Collections.Generic;
using UnityEngine;

namespace IntoTheWoods {
    public class Inventory : MonoBehaviour {
        [SerializeField] private List<Collectible> collectibles;

        public event Action<List<Collectible>> CollectiblesUpdated;

        private void Awake() {
            collectibles = new();
        }

        public void AddCollectible(Collectible collectible) {
            collectibles.Add(collectible);
            CollectiblesUpdated?.Invoke(collectibles);
        }
    }
}
