using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// Scale camera according to screen resolution
/// </summary>
public class BlackBorder : MonoBehaviour {
    private const float CheckInterval = 2;
    private const float FullHDRatio = 16f / 9;

    private float _timeSinceCheck;
    private Camera _mainCamera;
    /// <summary>
    /// Cached default screen rect
    /// </summary>
    private Rect _defaultRect;
    /// <summary>
    /// The aspect ratio the camera is currently scaled for.
    /// </summary>
    private float _currentAspect;

    private void Start() {
        // Get camera
        Assert.IsNotNull(Camera.main);
        _mainCamera = Camera.main;

        // Cache default camera rect 
        _defaultRect = _mainCamera.rect;
        _defaultRect.y = 0;
        _defaultRect.height = 1;

        UpdateCamera();
    }

    private void UpdateCamera() {
        // Scale camera to show black borders and keep 16:9 aspect ratio
        float aspectRatio = (float)UnityEngine.Screen.width / UnityEngine.Screen.height; // do not use currentResolution, it returns the total resolution for all screens on multi-screen setups
        if (Mathf.Approximately(aspectRatio, _currentAspect)) {
            return;
        }

        _currentAspect = aspectRatio;

        switch (FullHDRatio - aspectRatio) {
            case > 0.001f: {
                Rect rect = _defaultRect;
                rect.height = Screen.FullHDratio * UnityEngine.Screen.width / UnityEngine.Screen.height;
                rect.y = (1 - rect.height) / 2;
                _mainCamera.rect = rect;
                break;
            }
            case < -0.001f: {
                Rect rect = _defaultRect;
                rect.width = 1 / (Screen.FullHDratio * UnityEngine.Screen.width / UnityEngine.Screen.height);
                rect.x = (1 - rect.width) / 2;
                _mainCamera.rect = rect;
                break;
            }
            default:
                // _mainCamera.orthographicSize = 2;
                _mainCamera.rect = _defaultRect;
                break;
        }
    }

    private void Update() {
        _timeSinceCheck += Time.deltaTime;
        if (_timeSinceCheck > CheckInterval) {
            _timeSinceCheck = 0;
            UpdateCamera();
        }
    }
}
