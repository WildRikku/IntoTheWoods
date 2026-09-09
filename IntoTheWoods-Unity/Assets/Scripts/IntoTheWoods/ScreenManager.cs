using Sirenix.OdinInspector;
using UnityEngine;

namespace IntoTheWoods {
    public class ScreenManager : MonoBehaviour {
#if UNITY_EDITOR
        [Title("Screens")]
        [ShowInInspector, ReadOnly] private Screen[] _screens;
#endif

#if UNITY_EDITOR
        [Title("Manage")]
        [Button]
        public void Refresh() {
            _screens = GetComponentsInChildren<Screen>();
            float lastX = _screens[0].transform.position.x;
            for (int i = 1; i < _screens.Length; i++) {
                Screen screen = _screens[i];
                if (screen.transform.position.x <= lastX) {
                    Debug.LogError("Screen " + screen.gameObject.name + " has a wrong position or is positioned wrong in the hierarchy");
                    return;
                }
                lastX = screen.transform.position.x;
            }
        }
#endif
    }
}
