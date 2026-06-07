using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace IntoTheWoods.Characters {
    public class Mouse : MonoBehaviour {
        private static readonly int Running = Animator.StringToHash("running");
        private static readonly int Eating = Animator.StringToHash("eating");
        /// <summary>
        /// false = front lane, true = back lane
        /// </summary>
        public bool backLane;
        [SerializeField] private float speed = 2.7f;
        [SerializeField] private Transform head;
        [SerializeField] private Animator animator;
        [SerializeField] private AnimatorSignal animatorSignal;
        private Vector2 _target;
        private Vector2 _direction;
        private bool _moving;
        private Action _afterEatingAction;

        private void Start() {
            Assert.IsNotNull(head);
            Assert.IsNotNull(animator);
            Assert.IsNotNull(animatorSignal);
        }

        private void OnEnable() {
            animatorSignal.AnimationEnded += OnAnimationEnded;
        }

        private void OnDisable() {
            animatorSignal.AnimationEnded -= OnAnimationEnded;
        }

        private void Update() {
            if (!_moving) {
                return;
            }

            transform.position += new Vector3(speed * Time.deltaTime * _direction.x, 0);
            float distance = Math.Abs(head.position.x - _target.x);
            if (distance < 0.05f) {
                // close enough
                _moving = false;
                animator.SetBool(Running, false);
                animator.SetBool(Eating, true);
            }
        }

        private void OnAnimationEnded() {
            _afterEatingAction?.Invoke();
        }

        public void Call(Vector2 position, Action afterEating) {
            _target = position;
            Vector3 scale = transform.localScale;
            if (position.x < transform.position.x) {
                _direction = new(-1, 0);
                if (Math.Sign(scale.x) == -1) {
                    scale.x *= -1; // flip left, ensure to just flip, not change scale
                }
            }
            else {
                _direction = new(1, 0);
                if (Math.Sign(scale.x) == 1) {
                    scale.x *= -1; // flip right, ensure to just flip, not change scale 
                }
            }

            transform.localScale = scale;

            _moving = true;
            animator.SetBool(Running, true);
            _afterEatingAction = afterEating;
        }
    }
}
