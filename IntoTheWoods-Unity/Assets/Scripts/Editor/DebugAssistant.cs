#if ODIN_INSPECTOR
using IntoTheWoods;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DebugAssistant : OdinEditorWindow {
    [ShowInInspector] public GameState gameState;

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
    }

    private void OnPlayModeChange(PlayModeStateChange state) {
        switch (state) {
            case PlayModeStateChange.ExitingEditMode:
                break;
            case PlayModeStateChange.EnteredPlayMode: {
                FindDebugObjects();
                break;
            }
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

    [Button(ButtonSizes.Large), HorizontalGroup("Always")]
    public void JumpToActiveScreen() {
        if (!EditorApplication.isPlaying) {
            return;
        }
        SceneView win = GetWindow<SceneView>();
        win.LookAt(GameState.Instance.GetCurrentScreen().transform.position);
    }
}
#endif
