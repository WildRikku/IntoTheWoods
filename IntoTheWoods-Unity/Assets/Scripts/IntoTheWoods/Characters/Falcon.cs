using System;
using UnityEngine;

namespace IntoTheWoods.Characters {
    public class Falcon : MonoBehaviour {
        private static readonly int Attacking = Animator.StringToHash("attacking");
        private static readonly int Captured = Animator.StringToHash("captured");
        private const float DefaultHeight = 1.495f;

        public bool isActive = true;
        public Mouse currentMouse;
        public bool debug = true;
        public float speed = 3f;
        public Vector2 _walkingDirection = new(-1, 0);
        [SerializeField] private Animator animator;
        public bool flyingToTarget = true;
        public Vector3 currentTarget;
        public bool targetIsMouse = true;

        private void Awake() {
            currentTarget = currentMouse.transform.position;
        }

        private void Update() {
            Vector3 posDelta;
            if (debug && flyingToTarget) {
                Vector3 distance = currentTarget - transform.position;
                if (Math.Abs(distance.magnitude) < 0.1f) {
                    // reached target
                    flyingToTarget = false;
                    posDelta = new(speed * Time.deltaTime * _walkingDirection.x, 0);
                }
                else {
                    if (Math.Abs(distance.x) > 5) {
                        // normal flying until close
                        posDelta = new(speed * Time.deltaTime * _walkingDirection.x, 0);
                    }
                    else {
                        // attack flying
                        animator.SetBool(Attacking, true);
                        posDelta = distance.normalized * (speed * Time.deltaTime);
                    }
                }
            }
            else {
                // normal flying
                posDelta = new(speed * Time.deltaTime * _walkingDirection.x, 0);
            }

            transform.position += posDelta;
        }

        private void OnTriggerEnter2D(Collider2D other) {
            if (other.CompareTag("Mouse")) {
                Debug.Log("MAMPF");
                animator.SetBool(Attacking, false);
                animator.SetBool(Captured, true);
                currentTarget = new(transform.position.x - 5f, DefaultHeight, 0);
            }
        }
    }
}
