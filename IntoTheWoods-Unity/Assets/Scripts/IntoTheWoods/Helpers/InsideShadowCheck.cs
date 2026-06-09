using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace IntoTheWoods.Helpers {
    /// <summary>
    /// if collider is inside <see cref="InsideShadow"/> but not in this, it's truly inside
    /// </summary>
    public class InsideShadowCheck : MonoBehaviour {
        [ShowInInspector] private Dictionary<string, bool> captured = new();
        [SerializeField] private InsideShadow partner;

        private bool _success;

        public bool Success {
            get => _success;
            set {
                if (value != Success) {
                    InsideShadowChanged?.Invoke(value);
                }

                _success = value;
            }
        }

        public event Action<bool> InsideShadowChanged;

        private void OnTriggerEnter2D(Collider2D other) {
            if (!other.CompareTag("Player")) {
                return;
            }

            captured[other.gameObject.name + other.gameObject.GetInstanceID()] = true;
            Success = false;
        }

        private void OnTriggerExit2D(Collider2D other) {
            if (!other.CompareTag("Player")) {
                return;
            }

            string key = other.gameObject.name + other.gameObject.GetInstanceID();
            captured[key] = false;

            if (captured.Count != 16) {
                // we haven't seen all body parts yet
                return;
            }

            // same collider must be inside partner now, otherwise it has gone completely outside
            // since it might have been the last one, check all
            bool maybe = true;
            foreach (KeyValuePair<string, bool> pair in captured) {
                if (pair.Value || !partner.captured.ContainsKey(pair.Key)) {
                    maybe = false;
                    break;
                }
            }

            Success = maybe;
        }
    }
}
