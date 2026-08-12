#if ODIN_INSPECTOR
using System.Collections.Generic;
using IntoTheWoods;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DebugAssistant : OdinEditorWindow {
    [ShowInInspector] public GameState gameState;

    [HorizontalGroup("screens")]
    [ListDrawerSettings(DraggableItems = false, ShowFoldout = false, NumberOfItemsPerPage = 25)]
    public List<Screen> screens;
    [ListDrawerSettings(DraggableItems = false, ShowFoldout = false, NumberOfItemsPerPage = 25)]
    [HorizontalGroup("screens")]
    public List<Vector3> screenPositions;

    private bool _inGame;

    [MenuItem("Tools/Debugging Assistant & Cheat Tools")]
    private static void OpenWindow() {
        GetWindow<DebugAssistant>().Show();
        GetWindow<DebugAssistant>().Reload();
    }

    [UnityEditor.Callbacks.DidReloadScripts]
    public static void TriggerReloadOnRecompile() {
        GetWindow<DebugAssistant>().Reload();
    }

    private void OnSceneChanged(Scene oldScene, Scene newScene) {
        FindDebugObjects();
    }

    private void FindDebugObjects() {
        gameState = GameState.Instance;
        screens = new();
        screenPositions = new();
        GameObject[] rootGOs = SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (GameObject gameObject in rootGOs) {
            foreach (Screen screen in gameObject.GetComponentsInChildren<Screen>()) {
                screens.Add(screen);
                screenPositions.Add(screen.transform.position);
            }
        }
    }

    private void OnPlayModeChange(PlayModeStateChange state) {
        switch (state) {
            case PlayModeStateChange.ExitingEditMode:
                _inGame = true;
                break;
            case PlayModeStateChange.EnteredPlayMode:
                FindDebugObjects();
                break;
            case PlayModeStateChange.ExitingPlayMode:
                _inGame = false;
                break;
        }
    }

    [HorizontalGroup("Always", Title = "Debugger"), Button(ButtonSizes.Large)]
    public void Reload() {
        EditorSceneManager.activeSceneChangedInEditMode -= OnSceneChanged;
        EditorSceneManager.activeSceneChangedInEditMode += OnSceneChanged;
        SceneManager.activeSceneChanged -= OnSceneChanged;
        SceneManager.activeSceneChanged += OnSceneChanged;
        EditorApplication.playModeStateChanged -= OnPlayModeChange;
        EditorApplication.playModeStateChanged += OnPlayModeChange;
        FindDebugObjects();
    }

    [Button(ButtonSizes.Large), ShowIfGroup("InGame", Condition = "_inGame")]
    public void JumpToActiveScreen() {
        if (!EditorApplication.isPlaying) {
            return;
        }
        SceneView win = GetWindow<SceneView>();
        win.LookAt(GameState.Instance.GetCurrentScreen().transform.position);
    }
}
#endif
