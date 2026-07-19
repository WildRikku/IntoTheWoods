using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace IntoTheWoods.Characters {
    public class Falcon : MonoBehaviour {
        private static readonly int Attacking = Animator.StringToHash("attacking");
        private static readonly int Captured = Animator.StringToHash("captured");
        private static readonly int Landed = Animator.StringToHash("landed");
        private static readonly int Rising = Animator.StringToHash("rising");
        private const float DefaultHeight = 1.495f;
        private const float WaitOnGroundTime = 1.5f;
        [Header("Patrouille")]
        public float leftEnd;
        public float rightEnd;

        public bool isActive = true;
        public Mouse currentMouse;
        public float speed = 3f;
        [ShowInInspector] private Vector2 _flyingDirection = new(-1, 0);
        [SerializeField] private Animator animator;
        [ShowInInspector] private bool _flyingToTarget;
        [ShowInInspector] private bool _waiting;
        private float _waitedTime;
        [ShowInInspector] private bool _rising;
        [ShowInInspector] private Vector3 _currentTarget;
        /// <summary>
        /// Indicates if the target is a mouse (if not, it's probably a character).
        /// Does not contain a meaningful value when <see cref="_flyingToTarget"/> is false (does not get reset after reaching target).
        /// </summary>
        [ShowInInspector] private bool _targetIsMouse = true;

        private void Update() {
            Vector3 posDelta = new(speed * Time.deltaTime * _flyingDirection.x, 0); // default flying
            if (_flyingToTarget) {
                Vector3 distance = _currentTarget - transform.position;
                if (Math.Abs(distance.magnitude) < 0.1f) {
                    // reached target
                    _flyingToTarget = false;
                    animator.SetBool(Attacking, false);
                    animator.SetBool(Landed, true);
                    _waiting = true;
                    _waitedTime = 0;
                    // EditorApplication.isPaused = true;
                }
                else {
                    if (Math.Abs(distance.x) > 5) {
                        // normal flying until close
                        posDelta = new(speed * Time.deltaTime * _flyingDirection.x, 0);
                    }
                    else {
                        // attack flying
                        animator.SetBool(Attacking, true);
                        posDelta = distance.normalized * (speed * Time.deltaTime);
                    }
                }
            }
            else if (_waiting) {
                // wait on ground to pick on mouse and give players a little time to capture the bird
                _waitedTime += Time.deltaTime;
                if (_waitedTime > WaitOnGroundTime) {
                    _waiting = false;
                    _rising = true;
                    animator.SetBool(Rising, true);
                    _currentTarget = new(transform.position.x + 5, DefaultHeight, 0); // TODO respect facing direction
                    Destroy(currentMouse);
                }
                else {
                    posDelta = Vector3.zero;
                }
            }
            else if (_rising) {
                // get back to flying height
                Vector3 distance = _currentTarget - transform.position;
                if (Math.Abs(distance.magnitude) < 0.1f) {
                    animator.SetBool(Rising, false);
                    _rising = false;
                }
                else {
                    posDelta = distance.normalized * (speed * Time.deltaTime);
                }
            }
            else {
                // Patrouille
                if (transform.position.x < leftEnd) {
                    _flyingDirection = new(1, 0);
                    Vector3 scale = transform.localScale;
                    scale.x *= -1;
                    transform.localScale = scale;
                }
                else if (transform.position.x > rightEnd) {
                    _flyingDirection = new(-1, 0);
                    Vector3 scale = transform.localScale;
                    scale.x *= -1;
                    transform.localScale = scale;
                }
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
                    _currentTarget = mouse.gameObject.transform.position;
                    currentMouse = mouse;
                    _flyingToTarget = true;
                    // Time.timeScale = 0.5f; // TODO DEBUG
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
