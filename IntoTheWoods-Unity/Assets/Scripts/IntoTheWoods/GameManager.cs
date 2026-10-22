using System;
using System.Collections;
using System.Collections.Generic;
using IntoTheWoods.Characters;
using IntoTheWoods.Helpers;
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
        private Walker _followerWalker;

        [SerializeField] private Character primaryCharacter;
        [SerializeField] private Character secondaryCharacter;
        [SerializeField] private Falcon falcon;
        [SerializeField] private Witch witch;
        [SerializeField] private GameObject witchParticleSystem;
        [SerializeField] private UIDocument UI;
        [SerializeField] private Light2D characterLight;
        [SerializeField] private GameObject mousePrefab;
        [SerializeField] private Light2D globalLight;
        [SerializeField] private GameObject gameOverSign;
        [SerializeField] private GameObject gameWonSign;
        [SerializeField] private ExitDetector exitDetector;

        private void Awake() {
            Assert.IsNotNull(Camera.main);
            _mainCamera = Camera.main;

            // Get all screens
            List<Screen> screens = new(GetComponentsInChildren<Screen>());
            foreach (Screen screen in screens) {
                screen.InsideShadowChanged += ScreenOnInsideShadowChanged;
            }
            Assert.IsTrue(screens.Count > 0);
            GameState.Instance.InitializeScreens(screens);

            // reset globals
            GameState.Instance.kidsAreDoomed = false;
            GameState.Instance.kidsAreSafe = false;
            GameState.Instance.gameOver = false;
            GameState.Instance.gameWon = false;

            Assert.IsNotNull(primaryCharacter);
            Assert.IsNotNull(secondaryCharacter);

            // swap characters if player chose Gretel
            if (GameState.Instance.chosenCharacter) {
                (primaryCharacter, secondaryCharacter) = (secondaryCharacter, primaryCharacter);
            }

            primaryCharacter.AddComponent<PlayerController>();
            _playerWalker = primaryCharacter.GetComponent<Walker>();
            primaryCharacter.GetComponentInChildren<SortingGroup>().sortingOrder = 1; // primary player in front of secondary character
            // primaryCharacter.AddComponent<AudioListener>();

            FollowerController followerController = secondaryCharacter.AddComponent<FollowerController>();
            followerController.leader = _playerWalker;
            _followerWalker = secondaryCharacter.GetComponent<Walker>();


            Inventory inventory = GetComponent<Inventory>();
            Assert.IsNotNull(inventory);

            Assert.IsNotNull(UI);
            UI.rootVisualElement.dataSource = inventory;
        }

        private void OnEnable() {
            _playerWalker.WillMove += OnPlayerWillMove;
            _followerWalker.WillMove += OnPlayerWillMove;
            falcon.NotifiedWitch += FalconOnNotifiedWitch;
            witch.HexingKids += WitchOnHexingKids;
            witch.KidsHexed += WitchOnKidsHexed;
            witchParticleSystem.GetComponent<ParticleSystemEndedNotifier>().ParticleSystemDone += OnWitchParticleSystemDone;
            exitDetector.ExitReached += ExitDetectorOnExitReached;
        }

        private void Update() {
            if (!GameState.Instance.gameOver) {
                return;
            }

            if (globalLight.intensity > Time.deltaTime) {
                // dim light, since it should happen within 1 second and starts at 1, no factor is needed
                globalLight.intensity -= Time.deltaTime;
            }
            else {
                globalLight.intensity = 0;
                gameOverSign.transform.position = GameState.Instance.GetCurrentScreen().transform.position;
                gameOverSign.SetActive(true);
            }
        }

        private void OnDisable() {
            _playerWalker.WillMove -= OnPlayerWillMove;
            _followerWalker.WillMove -= OnPlayerWillMove;
            falcon.NotifiedWitch -= FalconOnNotifiedWitch;
            witch.HexingKids -= WitchOnHexingKids;
            witch.KidsHexed -= WitchOnKidsHexed;
            witchParticleSystem.GetComponent<ParticleSystemEndedNotifier>().ParticleSystemDone -= OnWitchParticleSystemDone;
            exitDetector.ExitReached -= ExitDetectorOnExitReached;
        }

        private void ScreenOnInsideShadowChanged(bool safe) {
            characterLight.intensity = safe ? 0.15f : 0.3f;
        }

        /// <summary>
        /// Reacts to movement by adjusting camera. Also verifies if movement is allowed
        /// </summary>
        /// <param name="nextPosition"></param>
        /// <param name="walkingDirection"></param>
        /// <param name="walker"></param>
        /// <returns>true if movement was done, false if movement was not allowed because it was at a map edge</returns>
        private bool OnPlayerWillMove(Vector2 nextPosition, Vector2 walkingDirection, Walker walker) {
            Screen.ScreenEdgeResult edgeStatus = GameState.Instance.GetCurrentScreen().CheckPosition(nextPosition);
            switch (edgeStatus) {
                case Screen.ScreenEdgeResult.IllegalLeft:
                case Screen.ScreenEdgeResult.IllegalRight:
                    // Reached forbidden area
                    return false;
                case Screen.ScreenEdgeResult.LeavingRight when walkingDirection.x > 0: {
                    // going right close to the edge
                    // Actions are different according to whether this is the actively controlled character
                    if (walker == _playerWalker) {
                        if (!GameState.Instance.NextScreen(out int newScreenIndex)) {
                            // player would leave the game
                            return false;
                        }

                        // scroll right
                        Vector3 pos = _mainCamera.transform.position;
                        pos.x += Screen.ScreenWidth;
                        _mainCamera.transform.position = pos;
                    }
                    break;
                }
                case Screen.ScreenEdgeResult.LeavingLeft when walkingDirection.x < 0: {
                    // going left close to the edge
                    // Actions are different according to whether this is the actively controlled character
                    if (walker == _playerWalker) {
                        if (!GameState.Instance.PreviousScreen(out int newScreenIndex)) {
                            // player would leave the game
                            return false;
                        }
                        // scroll left
                        Vector3 pos = _mainCamera.transform.position;
                        pos.x -= Screen.ScreenWidth;
                        _mainCamera.transform.position = pos;
                    }

                    break;
                }
            }

            return true;
        }

        /// <summary>
        /// "Call" witch when the falcon has "notified" her.
        /// Teleport witch to kids, which will trigger the witch hexing the kids through physics detection 
        /// </summary>
        private void FalconOnNotifiedWitch() {
            GameState.Instance.kidsAreDoomed = true;
            // Position witch on the side of the player with more space
            // TODO solve the case in which the player character is on one screen and the follower character is on another (which is not trivial since the follower character might be far away)
            GameState.Instance.GetCurrentScreen().PositionInScreen(_playerWalker.transform.position, out float percentage);
            float witchXPos = percentage > 0.5f ? _playerWalker.transform.position.x - 2 : _playerWalker.transform.position.x + 2;
            witch.gameObject.SetActive(false);
            witch.transform.parent.position = new(witchXPos, -0.57f, 0);
            Vector3 scale = witch.transform.parent.localScale;
            scale.x = -1;
            witch.transform.parent.localScale = scale;
            witchParticleSystem.SetActive(true);
        }

        private void OnWitchParticleSystemDone() {
            witch.gameObject.SetActive(true);
        }

        private void WitchOnHexingKids() {
            primaryCharacter.particleSystem.SetActive(true);
            secondaryCharacter.particleSystem.SetActive(true);
        }

        private void WitchOnKidsHexed() {
            Debug.Log("pooof make mice");
            Instantiate(mousePrefab, primaryCharacter.transform.position, Quaternion.identity);
            Instantiate(mousePrefab, secondaryCharacter.transform.position, Quaternion.identity);
            Destroy(primaryCharacter.characterContainer);
            Destroy(secondaryCharacter.characterContainer);
            primaryCharacter.particleSystem.GetComponent<ParticleSystem>().Stop();
            secondaryCharacter.particleSystem.GetComponent<ParticleSystem>().Stop();
            GameState.Instance.gameOver = true;
        }

        #region Debug helper functions

#if UNITY_EDITOR
        public void SpawnWitch() {
            GameState.Instance.GetCurrentScreen().PositionInScreen(_playerWalker.transform.position, out float percentage);
            float witchXPos = percentage > 0.5f ? _playerWalker.transform.position.x - 2 : _playerWalker.transform.position.x + 2;
            witch.gameObject.SetActive(false);
            witch.transform.parent.position = new(witchXPos, -0.57f, 0);
            Vector3 scale = witch.transform.parent.localScale;
            scale.x = -1;
            witch.transform.parent.localScale = scale;
            witchParticleSystem.SetActive(true);
        }
#endif

        #endregion

        private void ExitDetectorOnExitReached() {
            GameState.Instance.gameWon = true;
            gameWonSign.transform.position = GameState.Instance.GetCurrentScreen().transform.position;
            gameWonSign.SetActive(true);
        }
    }
}
