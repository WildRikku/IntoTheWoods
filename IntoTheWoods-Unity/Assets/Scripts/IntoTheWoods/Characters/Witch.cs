using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Assertions;

namespace IntoTheWoods.Characters {
    public class Witch : MonoBehaviour {
        private static readonly int Magic = Animator.StringToHash("magic");

        [ShowInInspector] private readonly Dictionary<int, Walker> _detectedWalkers = new();
        private bool Detected => _detectedWalkers.Count > 0;
        [SerializeField] private Walker myWalker;

        [SerializeField] private Animator animator;
        [SerializeField] private AnimatorSignal _animatorSignal;

        public event Action HexingKids;
        public event Action KidsHexed;
        
        private AudioSource _audioSource;

        private void Awake() {
            Assert.IsNotNull(myWalker);
            Assert.IsNotNull(_animatorSignal);
        }

        private void OnEnable() {
            if (_animatorSignal != null) {
                _animatorSignal.AnimationEnded += OnAnimationEnded;
            }
            _audioSource = gameObject.GetComponent<AudioSource>();
        }

        private void OnDisable() {
            if (_animatorSignal != null) {
                _animatorSignal.AnimationEnded -= OnAnimationEnded;
            }
        }

        private void OnTriggerEnter2D(Collider2D other) {
            if (other.gameObject.CompareTag("Player")) {
                Walker otherWalker = other.gameObject.GetComponentInParent<Walker>();
                if (otherWalker == null) {
                    // whatever the fuck happened
                    Debug.LogError("Witch trigger was entered but doing nothing because no walker was found");
                    return;
                }

                // we need to know if this is the very first detected walker which was detected for the first time
                // it is not if there is already a walker stored, be it this one or another one
                bool firstTime = !Detected;
                _detectedWalkers.TryAdd(otherWalker.gameObject.GetInstanceID(), otherWalker);

                if (firstTime) {
                    Transform otherWalkerTransform = otherWalker.transform;
                    // detect if witch has her back turned towards the player (check flip and relative position)
                    // in that case, the player has entered the shorter end of the collider and the witch turns because she "heard" them
                    if (Math.Sign(otherWalkerTransform.position.x - myWalker.transform.position.x) == Math.Sign(myWalker.transform.localScale.x)) {
                        // I HEARD YOU. Turn around
                        Debug.Log("I HEARD YOU");
                        Vector3 scale = myWalker.transform.localScale;
                        scale.x *= -1;
                        myWalker.transform.localScale = scale;
                    }

                    Debug.Log("I SEE YOU");

                    // walk towards children
                    if (!myWalker.IsWalking) {
                        Debug.Log("I'M COMING FOR YA " + otherWalker.name);
                        // set game over - redundant in the "spotted by falcon" case, but not if the kids walked towards the witch house
                        GameState.Instance.kidsAreDoomed = true;
                        Vector2 moveVector = new(Math.Sign(otherWalkerTransform.position.x - myWalker.transform.position.x), 0);
                        myWalker.ActivateWalking(moveVector, true);
                        myWalker.Moved += MyWalkerOnMoved;
                        if (!_audioSource.isPlaying) {
                            _audioSource.Play();
                        }
                    }
                }
            }
        }

        private void MyWalkerOnMoved(Vector2 newPosition, bool ignoreDistance, bool inTransferZone) {
            // no matter which walker started the moving, stop at the one that is closer
            foreach (Walker walker in _detectedWalkers.Values) {
                if (Math.Abs(newPosition.x - walker.transform.position.x) < 1.5f) {
                    myWalker.StopWalking();
                    animator.SetBool(Magic, true);
                    HexingKids?.Invoke();
                    return;
                }
            }
        }

        private void OnAnimationEnded() {
            animator.SetBool(Magic, false);
            KidsHexed?.Invoke();
        }
    }
}
