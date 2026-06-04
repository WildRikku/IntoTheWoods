using System.Collections.Generic;

namespace IntoTheWoods.Data {
    public class GameData {
        public int stones;
        public int bread;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inventory">Inventory to bind to.</param>
        public GameData(Inventory inventory) {
            inventory.CollectiblesUpdated += InventoryOnCollectiblesUpdated;
        }

        private void InventoryOnCollectiblesUpdated(List<Collectible> obj) {
            stones = obj.Count;
        }
    }
}
