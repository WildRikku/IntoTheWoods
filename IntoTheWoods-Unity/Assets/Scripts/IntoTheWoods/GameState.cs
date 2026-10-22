using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace IntoTheWoods {
    public class GameState {
        private const int StartScreen = 4;
        [ShowInInspector] private List<Screen> _screens = new();
        /// <summary>
        /// The screen currently shown by the camera. This is always the screen with the player-controlled character.
        /// </summary>
        [ShowInInspector] private int _currentScreen = StartScreen; // configure start screen here (0-based)

        /// <summary>
        /// true = Gretel, false = Hänsel
        /// </summary>
        public bool chosenCharacter;

        public bool kidsAreSafe;
        public bool kidsAreDoomed;
        public bool gameOver;
        public bool gameWon;

        public void InitializeScreens(List<Screen> screens) {
            _screens = screens;
            foreach (Screen screen in _screens) {
                screen.InsideShadowChanged += ScreenOnInsideShadowChanged;
            }
            _currentScreen = StartScreen;
        }

        public Screen GetCurrentScreen() {
            return _screens[_currentScreen];
        }

        public bool PlayerIsAtRightMostScreen() {
            return _currentScreen == _screens.Count - 1;
        }

        public bool PlayerIsAtLeftMostScreen() {
            return _currentScreen == 0;
        }

        public bool NextScreen(out int newScreenIndex) {
            if (!PlayerIsAtRightMostScreen()) {
                _currentScreen++;
                newScreenIndex = _currentScreen;
                return true;
            }
            newScreenIndex = _currentScreen;
            return false;
        }

        public bool PreviousScreen(out int newScreenIndex) {
            if (!PlayerIsAtLeftMostScreen()) {
                _currentScreen--;
                newScreenIndex = _currentScreen;
                return true;
            }
            newScreenIndex = _currentScreen;
            return false;
        }

        private void ScreenOnInsideShadowChanged(bool safe) {
            kidsAreSafe = safe;
        }

        #region Singleton management

        private static readonly Lazy<GameState> Lazy = new(() => new());

        public static GameState Instance => Lazy.Value;

        private GameState() {
#if UNITY_EDITOR
            Debug.Log("<color=orange>Singleton instantiated :)</color>");
#endif
        }

        #endregion
    }
}
