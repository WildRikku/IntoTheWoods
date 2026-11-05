using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace IntoTheWoods.Helpers {
    [Flags]
    public enum HiddenKids : ushort {
        None = 0, // 00
        Haensel = 1, // 01
        Gretel = 2, // 10
        Both = 3 // 11
    }

    /// <summary>
    /// if collider is inside <see cref="InsideShadow"/> but not in this, it's truly inside
    /// </summary>
    public class InsideShadowCheck : MonoBehaviour {
        [ShowInInspector] private Dictionary<string, bool> _capturedHaensel = new();
        [ShowInInspector] private Dictionary<string, bool> _capturedGretel = new();
        [SerializeField] private InsideShadow partner;

        private HiddenKids _success;

        public HiddenKids Success {
            get => _success;
            set {
                if (value != Success) {
                    Debug.Log(gameObject.name + " reports: " + value + " at " + Time.time);
                    InsideShadowChanged?.Invoke(value, GetInstanceID());
                }

                _success = value;
            }
        }

        public event Action<HiddenKids, int> InsideShadowChanged;

        private void OnTriggerEnter2D(Collider2D other) {
            if (!(other.CompareTag("Haensel") || other.CompareTag("Gretel"))) {
                return;
            }

            if (other.CompareTag("Haensel")) {
                _capturedHaensel[other.gameObject.name + other.gameObject.GetInstanceID()] = true;
                Success = _success & ~HiddenKids.Haensel;
            }
            else {
                _capturedGretel[other.gameObject.name + other.gameObject.GetInstanceID()] = true;
                Success = _success & ~HiddenKids.Gretel;
            }
        }

        private void OnTriggerExit2D(Collider2D other) {
            if (!(other.CompareTag("Haensel") || other.CompareTag("Gretel"))) {
                return;
            }

            string key = other.gameObject.name + other.gameObject.GetInstanceID();
            if (other.CompareTag("Haensel")) {
                _capturedHaensel[key] = false;
            }
            else {
                _capturedGretel[key] = false;
            }

            if (_capturedHaensel.Count == 8) {
                // we have seen all of Haensel's body parts, check if the inner collider also has all of them
                // otherwise it has gone completely outside
                bool maybe = true;
                foreach (KeyValuePair<string, bool> pair in _capturedHaensel) {
                    if (pair.Value || !partner.capturedHaensel.ContainsKey(pair.Key)) {
                        // part is still in this (outer) part or not in inner
                        maybe = false;
                        break;
                    }
                }

                Success = maybe ? _success | HiddenKids.Haensel : _success & ~HiddenKids.Haensel; // TODO collect and set Success at the end
            }

            if (_capturedGretel.Count == 8) {
                // we have seen all of Gretel's body parts, check if the inner collider also has all of them
                // otherwise it has gone completely outside
                bool maybe = true;
                foreach (KeyValuePair<string, bool> pair in _capturedGretel) {
                    if (pair.Value || !partner.capturedGretel.ContainsKey(pair.Key)) {
                        // part is still in this (outer) part or not in inner
                        maybe = false;
                        break;
                    }
                }

                Success = maybe ? _success | HiddenKids.Gretel : _success & ~HiddenKids.Gretel;
            }
        }
    }
}
