using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace IntoTheWoods {
    public class GameManager : MonoBehaviour {
        private const float FullHDratio = 0.5625f;
        /// <summary>
        /// Value chosen based on what looks good and also so that it's a little asymmetrical to avoid jumping back and forth between screens
        /// </summary>
        private const float CrossScreenThreshold = 3.6f;
        /// <summary>
        /// // 4 high and 16:9
        /// </summary>
        private const float ScreenWidth = 7.11f;

        private Camera _mainCamera;
        private List<Screen> _screens;
        private int _currentScreen;

        [SerializeField] private Player primaryPlayer;

        private void Awake() {
            Assert.IsNotNull(Camera.main);
            _mainCamera = Camera.main;

            // Scale camera to show black borders and keep 16:9 aspect ratio
            float aspect = (float)UnityEngine.Screen.height / UnityEngine.Screen.width; // do not use currentResolution, it returns the total resolution for all screens on multi-screen setups
            if (aspect > FullHDratio) {
                Rect rect = Camera.main.rect;
                rect.height = FullHDratio * UnityEngine.Screen.width / UnityEngine.Screen.height;
                rect.y = (1 - rect.height) / 2;
                Camera.main.rect = rect;
            }

            // Get all screens
            _screens = new(GetComponentsInChildren<Screen>());
            Assert.IsTrue(_screens.Count > 0);
        }

        private void OnEnable() {
            primaryPlayer.WillMove += PlayerHasMoved;
        }

        private void OnDisable() {
            primaryPlayer.WillMove -= PlayerHasMoved;
        }

        private bool PlayerHasMoved(Player sender, Vector2 walkingDirection) {
            // possible optimization: cache screen position 
            if (walkingDirection.x > 0 && sender.transform.position.x > _screens[_currentScreen].transform.position.x + CrossScreenThreshold) {
                if (_currentScreen < _screens.Count - 1) {
                    // scroll right
                    Vector3 pos = _mainCamera.transform.position;
                    pos.x += ScreenWidth;
                    _mainCamera.transform.position = pos;
                    _currentScreen++;
                }
                else {
                    // player would leave the game
                    return false;
                }
            }
            else if (walkingDirection.x < 0 && sender.transform.position.x < _screens[_currentScreen].transform.position.x - CrossScreenThreshold) {
                if (_currentScreen > 0) {
                    // scroll left
                    Vector3 pos = _mainCamera.transform.position;
                    pos.x -= ScreenWidth;
                    _mainCamera.transform.position = pos;
                    _currentScreen--;
                }
                else {
                    // player would leave the game
                    return false;
                }
            }

            return true;
        }
    }
}
