using System.Collections.Generic;
using IntoTheWoods.Characters;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

namespace IntoTheWoods {
    [RequireComponent(typeof(Inventory))]
    [DisallowMultipleComponent]
    public class GameManager : MonoBehaviour {
        private Camera _mainCamera;

        private Walker _playerWalker;

        [SerializeField] private Character primaryCharacter;
        [SerializeField] private Character secondaryCharacter;
        [SerializeField] private UIDocument UI;
        [SerializeField] private Light2D characterLight;

        private void Awake() {
            Assert.IsNotNull(Camera.main);
            _mainCamera = Camera.main;

            // Scale camera to show black borders and keep 16:9 aspect ratio
            float aspect = (float)UnityEngine.Screen.height / UnityEngine.Screen.width; // do not use currentResolution, it returns the total resolution for all screens on multi-screen setups
            if (aspect > Screen.FullHDratio) {
                Rect rect = Camera.main.rect;
                rect.height = Screen.FullHDratio * UnityEngine.Screen.width / UnityEngine.Screen.height;
                rect.y = (1 - rect.height) / 2;
                Camera.main.rect = rect;
            }

            // Get all screens
            List<Screen> screens = new(GetComponentsInChildren<Screen>());
            foreach (Screen screen in screens) {
                screen.InsideShadowChanged += ScreenOnInsideShadowChanged;
            }
            Assert.IsTrue(screens.Count > 0);
            GameState.Instance.InitializeScreens(screens);

            Assert.IsNotNull(primaryCharacter);
            primaryCharacter.AddComponent<PlayerController>();
            _playerWalker = primaryCharacter.GetComponent<Walker>();
            primaryCharacter.GetComponentInChildren<SortingGroup>().sortingOrder = 1; // primary player in front of secondary character
            primaryCharacter.AddComponent<AudioListener>();

            Assert.IsNotNull(secondaryCharacter);
            FollowerController followerController = secondaryCharacter.AddComponent<FollowerController>();
            followerController.leader = _playerWalker;
            // we might need to save the follower walker and subscribe to its events, too, but maybe not since the follower should never walk off the screen if the player doesn't...

            Inventory inventory = GetComponent<Inventory>();
            Assert.IsNotNull(inventory);

            Assert.IsNotNull(UI);
            UI.rootVisualElement.dataSource = inventory;
        }

        private void OnEnable() {
            _playerWalker.WillMove += OnPlayerWillMove;
        }

        private void OnDisable() {
            _playerWalker.WillMove -= OnPlayerWillMove;
        }

        private void ScreenOnInsideShadowChanged(bool safe) {
            characterLight.intensity = safe ? 0.15f : 0.3f;
        }

        /// <summary>
        /// Reacts to movement by adjusting camera. Also verifies if movement is allowed
        /// </summary>
        /// <param name="nextPosition"></param>
        /// <param name="walkingDirection"></param>
        /// <returns>true if movement was done, false if movement was not allowed because it was at a map edge</returns>
        private bool OnPlayerWillMove(Vector2 nextPosition, Vector2 walkingDirection) {
            Screen.ScreenEdgeResult edgeStatus = GameState.Instance.GetCurrentScreen().CheckPosition(nextPosition);
            switch (edgeStatus) {
                case Screen.ScreenEdgeResult.IllegalLeft:
                case Screen.ScreenEdgeResult.IllegalRight:
                    // Reached forbidden area
                    return false;
                case Screen.ScreenEdgeResult.LeavingRight when walkingDirection.x > 0:
                    // going right close to the edge
                    if (!GameState.Instance.PlayerIsAtRightMostScreen()) {
                        // scroll right
                        Vector3 pos = _mainCamera.transform.position;
                        pos.x += Screen.ScreenWidth;
                        _mainCamera.transform.position = pos;
                        GameState.Instance.NextScreen();
                    }
                    else {
                        // player would leave the game
                        return false;
                    }

                    break;
                case Screen.ScreenEdgeResult.LeavingLeft when walkingDirection.x < 0:
                    // going left close to the edge
                    if (!GameState.Instance.PlayerIsAtLeftMostScreen()) {
                        // scroll left
                        Vector3 pos = _mainCamera.transform.position;
                        pos.x -= Screen.ScreenWidth;
                        _mainCamera.transform.position = pos;
                        GameState.Instance.PreviousScreen();
                    }
                    else {
                        // player would leave the game
                        return false;
                    }

                    break;
            }

            return true;
        }
    }
}
