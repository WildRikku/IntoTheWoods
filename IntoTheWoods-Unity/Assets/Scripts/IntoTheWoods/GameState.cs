using System;
using System.Collections.Generic;
using IntoTheWoods.Characters;
using IntoTheWoods.Helpers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace IntoTheWoods {
    public class GameState {
        public const int StartScreen = 4;
        [ShowInInspector] private List<Screen> _screens = new();
        /// <summary>
        /// The screen currently shown by the camera. This is always the screen with the player-controlled character.
        /// </summary>
        [ShowInInspector] private int _currentScreen = StartScreen; // configure start screen here (0-based)

        /// <summary>
        /// true = Gretel, false = Hänsel
        /// </summary>
        public bool chosenCharacter;
        public Walker haenselWalker;
        public Walker gretelWalker;

        private bool _kidsAreSafe;
        [ShowInInspector]
        private readonly Dictionary<int, HiddenKids> _shadowKidsStates = new();

        [ShowInInspector]
        public bool KidsAreSafe {
            get {
                // Calculate just in time so that we don't need to care about screen changes or which shadow reports when
                // inefficient but tracking all that would be pretty complicated
                if (_shadowKidsStates.Count == 0) {
                    return false;
                }

                // if a kid is hidden in any shadow, it is hidden, no matter which shadow reported right now
                // TODO at least the iteration over the dict could be cached
                HiddenKids totalHiddenKids = HiddenKids.None;
                foreach (HiddenKids h in _shadowKidsStates.Values) {
                    totalHiddenKids |= h;
                }

                // check if either both kids are safe or the kid that is not safe is not on screen
                bool kidsAreSafe = totalHiddenKids == HiddenKids.Both
                                   || (totalHiddenKids == HiddenKids.Haensel && haenselWalker.currentScreen == _currentScreen && haenselWalker.currentScreen != gretelWalker.currentScreen)
                                   || (totalHiddenKids == HiddenKids.Gretel && gretelWalker.currentScreen == _currentScreen && haenselWalker.currentScreen != gretelWalker.currentScreen);

                if (kidsAreSafe != _kidsAreSafe) {
                    KidsAreSafeChanged?.Invoke(kidsAreSafe);
                }

                _kidsAreSafe = kidsAreSafe;
                return _kidsAreSafe;
            }
            set {
                if (value != _kidsAreSafe) {
                    _kidsAreSafe = value;
                    KidsAreSafeChanged?.Invoke(_kidsAreSafe);
                }
            }
        }

        public event Action<bool> KidsAreSafeChanged;

        public bool kidsAreDoomed;
        public bool gameOver;
        public bool gameWon;

        public void Reset() {
            kidsAreDoomed = false;
            KidsAreSafe = false;
            gameOver = false;
            gameWon = false;
            _kidsAreSafe = false;
            _shadowKidsStates.Clear();
        }

        public void InitializeScreens(List<Screen> screens) {
            _screens = screens;
            foreach (Screen screen in _screens) {
                screen.InsideShadowChanged += ScreenOnInsideShadowChanged;
            }
            _currentScreen = StartScreen;
        }

        public Screen GetScreen() {
            return _screens[_currentScreen];
        }

        public Screen GetScreen(int index) {
            return _screens[index];
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

        private void ScreenOnInsideShadowChanged(HiddenKids safe, int shadowInstanceId) {
            // collect kids hiddenness from all shadows
            _shadowKidsStates[shadowInstanceId] = safe;
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
