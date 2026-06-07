using UnityEngine;
using UnityEngine.Assertions.Must;

public class Screen : MonoBehaviour {
    public enum ScreenEdgeResult {
        LeavingLeft,
        LeavingRight,
        IllegalLeft,
        IllegalRight,
        None
    }

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
}
