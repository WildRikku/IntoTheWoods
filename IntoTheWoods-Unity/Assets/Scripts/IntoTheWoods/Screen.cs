using System;
using IntoTheWoods.Helpers;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

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

    /// <summary>
    /// Value chosen based on what looks good and also so that it's a little asymmetrical to avoid jumping back and forth between screens
    /// </summary>
    private const float CrossScreenThreshold = 3.6f;

    [Header("Define where the next screen can be reached (independent of whether there actually is a screen")]
    public bool hasBackLane;
    public bool canLeaveScreenFrontLeft;
    public bool canLeaveScreenFrontRight;
    [ShowIf("hasBackLane")]
    public bool canLeaveScreenBackLeft;
    [ShowIf("hasBackLane")]
    public bool canLeaveScreenBackRight;

    [Header("For those corners where no screen crossing is allowed, define the edges (others are ignored).")]
    public float frontLeftThreshold;
    public float frontRightThreshold;
    public float backLeftThreshold;
    public float backRightThreshold;
    [Tooltip("y above this value will be considered back lane")]
    public float heightThreshold = -0.56f;

#if ODIN_INSPECTOR && UNITY_EDITOR
    [Button]
    public void MakeThresholdsRelative() {
        Undo.RecordObject(this, "Make thresholds relative");
        float x = transform.position.x;
        if (Math.Abs(frontLeftThreshold) > ScreenWidth) {
            // not relative
            frontLeftThreshold -= x;
        }
        if (Math.Abs(frontRightThreshold) > ScreenWidth) {
            // not relative
            frontRightThreshold -= x;
        }
        if (Math.Abs(backLeftThreshold) > ScreenWidth) {
            // not relative
            backLeftThreshold -= x;
        }
        if (Math.Abs(backRightThreshold) > ScreenWidth) {
            // not relative
            backRightThreshold -= x;
        }
    }
#endif

    public event Action<bool> InsideShadowChanged;

    private void Awake() {
        foreach (InsideShadowCheck check in GetComponentsInChildren<InsideShadowCheck>()) {
            check.InsideShadowChanged += OnInsideShadowChanged;
        }

        Assert.IsTrue(canLeaveScreenFrontLeft || frontLeftThreshold != 0, gameObject.name);
        Assert.IsTrue(canLeaveScreenFrontRight || frontRightThreshold != 0, gameObject.name);
        Assert.IsTrue(!hasBackLane || canLeaveScreenBackLeft || backLeftThreshold != 0, gameObject.name);
        Assert.IsTrue(!hasBackLane || canLeaveScreenBackRight || backRightThreshold != 0, gameObject.name);
    }

    /// <summary>
    /// Check if given position is in screen transfer area or in illegal area.
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

            if (frontLeftThreshold != 0 && characterPosition.x < transform.position.x + frontLeftThreshold) {
                return ScreenEdgeResult.IllegalLeft;
            }

            if (frontRightThreshold != 0 && characterPosition.x > transform.position.x + frontRightThreshold) {
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

            if (backLeftThreshold != 0 && characterPosition.x < transform.position.x + backLeftThreshold) {
                return ScreenEdgeResult.IllegalLeft;
            }

            if (backRightThreshold != 0 && characterPosition.x > transform.position.x + backRightThreshold) {
                return ScreenEdgeResult.IllegalRight;
            }
        }

        return ScreenEdgeResult.None;
    }

    /// <summary>
    /// Checks if the given <paramref name="position"/> is within this screen on both axes.
    /// </summary>
    public bool PositionInScreen(Vector3 position) {
        return transform.position.x - ScreenWidth / 2f < position.x
               && position.x < transform.position.x + ScreenWidth / 2f
               && transform.position.y - ScreenHeight / 2f < position.y
               && position.y < transform.position.y + ScreenHeight / 2f;
    }

    /// <summary>
    /// Checks if the given <paramref name="position"/> is within this screen on the x axis and returns where.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="percentage">0 when the given x position is exactly on the left edge of the screen,
    /// 1 if exactly on the right edge and linear moving in between,
    /// or -1 if not within the screen at all.</param>
    /// <returns>True if the position is within this screen on the x axis, false if not.</returns>
    public bool PositionInScreen(Vector3 position, out float percentage) {
        float x = transform.position.x;
        float xPos = x < 0
            ? -1 * (x - ScreenWidth / 2f - position.x)
            : position.x - (x - ScreenWidth / 2f);
        bool isWithin = xPos is >= 0 and <= ScreenWidth;
        percentage = isWithin ? xPos / ScreenWidth : -1;
        return isWithin;
    }

    protected virtual void OnInsideShadowChanged(bool obj) {
        InsideShadowChanged?.Invoke(obj);
    }

    public override string ToString() {
        return gameObject.name;
    }
}
