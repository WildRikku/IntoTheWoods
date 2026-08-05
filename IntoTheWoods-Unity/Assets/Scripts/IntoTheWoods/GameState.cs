using System;
using System.Collections.Generic;
using UnityEngine;

namespace IntoTheWoods {
    public class GameState {
        private List<Screen> _screens = new();
        private int _currentScreen = 3; // configure start screen here (0-based)

        public void InitializeScreens(List<Screen> screens) {
            _screens = screens;
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

        #region Singleton management

        private static readonly Lazy<GameState> Lazy = new(() => new());

        public static GameState Instance => Lazy.Value;

        private GameState() {
#if UNITY_EDITOR
            Debug.Log("<color=orange>QUEST Singleton instantiated :)</color>");
#endif
        }

        #endregion
    }
}
