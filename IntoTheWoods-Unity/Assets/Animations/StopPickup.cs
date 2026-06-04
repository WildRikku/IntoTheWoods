using UnityEngine;

public class StopPickup : StateMachineBehaviour {
    private static readonly int Pickup = Animator.StringToHash("pickup");

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        animator.SetBool(Pickup, false);
    }
}
