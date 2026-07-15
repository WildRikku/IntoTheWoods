using System;
using UnityEngine;

namespace IntoTheWoods.Characters {
    public class Falcon : MonoBehaviour {
        private static readonly int Attacking = Animator.StringToHash("attacking");
        private static readonly int Captured = Animator.StringToHash("captured");
        private const float DefaultHeight = 1.495f;
        [Header("Patrouille")]
        public float leftEnd;
        public float rightEnd;

        public bool isActive = true;
        public Mouse currentMouse;
        public float speed = 3f;
        public Vector2 _walkingDirection = new(-1, 0);
        [SerializeField] private Animator animator;
        public bool flyingToTarget;
        public Vector3 currentTarget;
        public bool targetIsMouse = true;

        private void Awake() {
        }

        private void Update() {
            Vector3 posDelta;
            if (flyingToTarget) {
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
                // Patrouille
                if (transform.position.x < leftEnd) {
                    _walkingDirection = new(1, 0);
                    Vector3 scale = transform.localScale;
                    scale.x *= -1;
                    transform.localScale = scale;
                }
                else if (transform.position.x > rightEnd) {
                    _walkingDirection = new(-1, 0);
                    Vector3 scale = transform.localScale;
                    scale.x *= -1;
                    transform.localScale = scale;
                }

                posDelta = new(speed * Time.deltaTime * _walkingDirection.x, 0);
            }

            transform.position += posDelta;
        }

        /// <summary>
        /// Find things
        /// </summary>
        /// <param name="other"></param>
        private void OnTriggerEnter2D(Collider2D other) {
            if (other.CompareTag("Mouse")) {
                Mouse mouse = other.GetComponent<Mouse>();
                if (mouse == null) {
                    mouse = other.GetComponentInParent<Mouse>();
                }

                if (mouse != null && mouse.mouseActive && !mouse._moving) {
                    Debug.Log("SPOTTED MOUSE " + mouse.gameObject.name + " in " + mouse.transform.parent.gameObject.name);
                    // animator.SetBool(Attacking, false);
                    // animator.SetBool(Captured, true);
                    // currentTarget = new(transform.position.x - 5f, DefaultHeight, 0);
                    currentTarget = mouse.gameObject.transform.position;
                    flyingToTarget = true;
                }
            }
            else if (other.CompareTag("Player")) {
                Character character = other.GetComponent<Character>();
                if (character == null) {
                    character = other.transform.parent.parent.GetComponent<Character>();
                }

                Debug.Log("Found " + character.gameObject.name);
            }
        }
    }
}
