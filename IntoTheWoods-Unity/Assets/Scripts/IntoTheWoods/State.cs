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
        public Vector3 currentTarget;

        public virtual MoveState Enter(Falcon falcon) {
            return this;
        }

        public abstract bool UpdateState(Falcon falcon, out Vector3 deltaPos);
    }
}
