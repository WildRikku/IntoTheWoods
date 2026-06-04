using UnityEngine;

public class StopThrow : StateMachineBehaviour {
    private static readonly int Throw = Animator.StringToHash("throwing");

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        animator.SetBool(Throw, false);
    }
}
