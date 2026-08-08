using System;
using IntoTheWoods.Characters;
using UnityEngine;

namespace IntoTheWoods {
    public abstract class State {
        public Action Done;
        public Animator animator;

        public virtual void Exit() {
        }
    }

    public abstract class PassiveState : State {
        public abstract PassiveState Enter();
        public abstract bool UpdateState();
    }

    public abstract class MoveState : State {
        public virtual MoveState Enter(Falcon falcon) {
            return this; // TODO why?
        }

        public abstract bool UpdateState(Falcon falcon, out Vector3 deltaPos);
    }

    public abstract class MoveToTargetState : MoveState {
        protected Vector3 currentTarget;

        protected MoveToTargetState(Vector3 currentTarget) {
            this.currentTarget = currentTarget;
        }
    }
}
