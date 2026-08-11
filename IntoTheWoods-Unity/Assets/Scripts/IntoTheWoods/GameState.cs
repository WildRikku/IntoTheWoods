using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace IntoTheWoods {
    public class GameState {
        private const int StartScreen = 3;
        [ShowInInspector] private List<Screen> _screens = new();
        [ShowInInspector] private int _currentScreen = StartScreen; // configure start screen here (0-based)

        public bool kidsAreSafe;

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

        public bool NextScreen() {
            if (!PlayerIsAtRightMostScreen()) {
                _currentScreen++;
                return true;
            }
            return false;
        }

        public bool PreviousScreen() {
            if (!PlayerIsAtLeftMostScreen()) {
                _currentScreen--;
                return true;
            }
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
