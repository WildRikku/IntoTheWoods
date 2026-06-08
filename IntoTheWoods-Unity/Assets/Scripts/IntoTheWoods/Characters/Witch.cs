using System;
using UnityEngine;

namespace IntoTheWoods.Characters {
    public class Witch : MonoBehaviour {
        private void OnCollisionEnter2D(Collision2D other) {
            if (other.gameObject.CompareTag("Player")) {
                Walker walker = other.gameObject.GetComponentInParent<Walker>();
                if (walker == null) {
                    // whatever the fuck happened
                    return;
                }

                // detect if witch has her back turned towards the player (check flip and relative position)
                // in that case, the player has entered the shorter end of the collider and the witch turns because she "heard" them
                Transform walkerTransform = walker.transform;
                if (Math.Sign(walkerTransform.position.x - transform.position.x) == Math.Sign(transform.localScale.x)) {
                    // I HEARD YOU. Turn around
                    Vector3 scale = transform.localScale;
                    scale.x *= -1;
                    transform.localScale = scale;
                }

                // walk towards children
            }
        }
    }
}
