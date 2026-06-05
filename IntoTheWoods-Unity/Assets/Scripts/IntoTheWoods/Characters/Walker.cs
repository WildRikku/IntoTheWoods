using UnityEngine;
using UnityEngine.Assertions;

namespace IntoTheWoods.Characters {
    public class Walker : MonoBehaviour {
        // Cached property indices for animator for efficiency
        private static readonly int Walking = Animator.StringToHash("walking");

        // configurable fields
        [SerializeField] private float speed = 1.7f;
        [SerializeField] protected int jumpForce = 125;

        // internal setup fields
        protected Rigidbody2D rb2D;
        private Animator _animator;

        // states
        private bool _walking;
        private Vector2 _walkingDirection;

        // events
        public event PlayerWillMoveEventHandler WillMove;

        private void Awake() {
            rb2D = GetComponent<Rigidbody2D>();
            Assert.IsNotNull(rb2D);
            _animator = GetComponentInChildren<Animator>();
            Assert.IsNotNull(_animator);
        }

        private void Update() {
            if (_walking) {
                // By sending the event before moving, we have the game manager check if we are going out of bounds
                // the order is relevant to prevent glitching outside
                if (!WillMove?.Invoke(this, _walkingDirection) ?? false) {
                    StopWalking();
                    return;
                }

                // zeroing out y axis since that only makes sense for ladders etc
                transform.position += new Vector3(speed * Time.deltaTime * _walkingDirection.x, 0);
            }
        }

        protected void ActivateWalking(Vector2 moveVector) {
            _walking = true;
            _walkingDirection = moveVector;
            _animator.SetBool(Walking, true);
        }

        public void StopWalking() {
            _walking = false;
            _animator.SetBool(Walking, false);
        }
    }
}
