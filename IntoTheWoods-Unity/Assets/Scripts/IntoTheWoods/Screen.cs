using System;
using IntoTheWoods.Helpers;
using UnityEngine;

public class Screen : MonoBehaviour {
    public enum ScreenEdgeResult {
        LeavingLeft,
        LeavingRight,
        IllegalLeft,
        IllegalRight,
        None
    }

    /// <summary>
    /// // 4 high and 16:9
    /// </summary>
    public const float ScreenWidth = 7.11f;
    public const float FullHDratio = 0.5625f;
    public const float ScreenHeight = ScreenWidth * FullHDratio;

    public event Action<bool> InsideShadowChanged;

    /// <summary>
    /// Value chosen based on what looks good and also so that it's a little asymmetrical to avoid jumping back and forth between screens
    /// </summary>
    private const float CrossScreenThreshold = 3.6f;

    [Header("Define where the next screen can be reached (independent of whether there actually is a screen")]
    public bool canLeaveScreenFrontLeft;
    public bool canLeaveScreenFrontRight;
    public bool canLeaveScreenBackLeft;
    public bool canLeaveScreenBackRight;

    [Header("For those corners where no screen crossing is allowed, define the edges (others are ignored).")]
    public float frontLeftThreshold;
    public float frontRightThreshold;
    public float backLeftThreshold;
    public float backRightThreshold;
    [Tooltip("y above this value will be considered back lane")]
    public float heightThreshold = -0.56f;

    private void Awake() {
        foreach (InsideShadowCheck check in GetComponentsInChildren<InsideShadowCheck>()) {
            check.InsideShadowChanged += OnInsideShadowChanged;
        }
    }

    /// <summary>
    /// Check if 
    /// </summary>
    /// <param name="characterPosition"></param>
    /// <returns></returns>
    public ScreenEdgeResult CheckPosition(Vector2 characterPosition) {
        if (characterPosition.y <= heightThreshold) {
            if (canLeaveScreenFrontLeft && characterPosition.x < transform.position.x - CrossScreenThreshold) {
                return ScreenEdgeResult.LeavingLeft;
            }

            if (canLeaveScreenFrontRight && characterPosition.x > transform.position.x + CrossScreenThreshold) {
                return ScreenEdgeResult.LeavingRight;
            }

            if (frontLeftThreshold != 0 && characterPosition.x < frontLeftThreshold) {
                return ScreenEdgeResult.IllegalLeft;
            }

            if (frontRightThreshold != 0 && characterPosition.x > frontRightThreshold) {
                return ScreenEdgeResult.IllegalRight;
            }
        }
        else {
            if (canLeaveScreenBackLeft && characterPosition.x < transform.position.x - CrossScreenThreshold) {
                return ScreenEdgeResult.LeavingLeft;
            }

            if (canLeaveScreenBackRight && characterPosition.x > transform.position.x + CrossScreenThreshold) {
                return ScreenEdgeResult.LeavingRight;
            }

            if (backLeftThreshold != 0 && characterPosition.x < backLeftThreshold) {
                return ScreenEdgeResult.IllegalLeft;
            }

            if (backRightThreshold != 0 && characterPosition.x > backRightThreshold) {
                return ScreenEdgeResult.IllegalRight;
            }
        }

        return ScreenEdgeResult.None;
    }

    public bool PositionInScreen(Vector3 position) {
        return transform.position.x - ScreenWidth / 2f < position.x
               && position.x < transform.position.x + ScreenWidth / 2f
               && transform.position.y - ScreenHeight / 2f < position.y
               && position.y < transform.position.y + ScreenHeight / 2f;
    }

    protected virtual void OnInsideShadowChanged(bool obj) {
        InsideShadowChanged?.Invoke(obj);
    }

    public override string ToString() {
        return gameObject.name;
    }
}
