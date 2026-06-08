using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace IntoTheWoods.Characters {
    public class Witch : MonoBehaviour {
        private readonly Dictionary<int, Walker> _detectedWalkers = new();
        private bool Detected => _detectedWalkers.Count > 0;
        [SerializeField] private Walker myWalker;

        [SerializeField] private Animator animator;

        private void Awake() {
            Assert.IsNotNull(myWalker);
        }

        private void OnTriggerEnter2D(Collider2D other) {
            if (other.gameObject.CompareTag("Player")) {
                Walker otherWalker = other.gameObject.GetComponentInParent<Walker>();
                if (otherWalker == null) {
                    // whatever the fuck happened
                    return;
                }

                // for turning around, we need to know if this is the very first detected walker which was detected for the first time
                // it is not if there is already a walker stored, be it this one or another one
                bool firstTime = !Detected;
                _detectedWalkers.TryAdd(otherWalker.gameObject.GetInstanceID(), otherWalker);

                Transform otherWalkerTransform = otherWalker.transform;
                if (firstTime) {
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
                        Debug.Log("I'M COMING FOR YA");
                        Vector2 moveVector = new(Math.Sign(otherWalkerTransform.position.x - myWalker.transform.position.x), 0);
                        myWalker.ActivateWalking(moveVector, true);
                        myWalker.Moved += MyWalkerOnMoved;
                    }
                }
            }
        }

        private void MyWalkerOnMoved(Vector2 newPosition, bool ignoreDistance, bool inTransferZone) {
            if (Math.Abs(newPosition.x - _detectedWalkers.Values.First().transform.position.x) < 1.1f) {
                myWalker.StopWalking();
                animator.SetBool(Magic, true);
            }
        }
    }
}
