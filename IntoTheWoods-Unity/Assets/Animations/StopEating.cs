using UnityEngine;

namespace Animations {
    public class StopEating : StateMachineBehaviour {
        private static readonly int Eating = Animator.StringToHash("eating");

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            animator.SetBool(Eating, false);
        }
    }
}
