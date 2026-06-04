using UnityEngine;
using UnityEngine.Assertions;

namespace IntoTheWoods {
    public class GameManager : MonoBehaviour {
        private const float FullHDratio = 0.5625f;

        private void Awake() {
            // Scale camera to show black borders and keep 16:9 aspect ratio
            float aspect = (float)Screen.height / Screen.width; // do not use currentResolution, it returns the total resolution for all screens on multi-screen setups

            if (aspect > FullHDratio) {
                Assert.IsNotNull(Camera.main);
                Rect rect = Camera.main.rect;
                rect.height = FullHDratio * Screen.width / Screen.height;
                rect.y = (1 - rect.height) / 2;
                Camera.main.rect = rect;
            }
        }
    }
}
