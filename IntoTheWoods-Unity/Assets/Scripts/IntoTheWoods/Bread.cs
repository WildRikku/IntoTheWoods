using IntoTheWoods.Characters;
using UnityEngine;

namespace IntoTheWoods {
    public class Bread : MonoBehaviour {
        private bool _hasFired;
        private int _miceLayer;
        private const float Distance = 100;

        private void Awake() {
            _miceLayer = LayerMask.GetMask("Mice");
        }

        private void OnCollisionEnter2D(Collision2D other) {
            if (_hasFired) {
                return;
            }

            // call closest mouse
            RaycastHit2D hitRight = Physics2D.Raycast(transform.position, Vector2.right, Distance, _miceLayer);
            RaycastHit2D hitLeft = Physics2D.Raycast(transform.position, Vector2.left, Distance, _miceLayer);
            RaycastHit2D hit;
            _hasFired = true;

            if (hitRight.collider != null) {
                if (hitLeft.collider != null) {
                    hit = hitRight.distance < hitLeft.distance ? hitRight : hitLeft;
                }
                else {
                    hit = hitRight;
                }
            }
            else {
                if (hitLeft.collider != null) {
                    hit = hitLeft;
                }
                else {
                    return;
                }
            }

            Mouse mouse = hit.transform.GetComponent<Mouse>();
            if (mouse == null) {
                mouse = hit.transform.GetComponentInParent<Mouse>();
            }

            if (mouse != null) {
                mouse.Call(transform.position, Despawn);
            }
        }

        private void Despawn() {
            Destroy(gameObject);
        }
    }
}
