using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Assertions;
using Vector2 = UnityEngine.Vector2;

namespace IntoTheWoods.Characters {
    /// <summary>
    /// Common base for follower actions, does not do anything on its own
    /// Is an interface because structs can't inherit structs
    /// </summary>
    public interface IFollowerAction {
    }

    public struct UseTransferZoneAction : IFollowerAction {
        public UseTransferZoneAction(Vector2 direction) {
            this.direction = direction;
        }

        public Vector2 direction;

        public override string ToString() {
            return "UseTransferZoneAction " + direction;
        }
    }

    public struct GoToPositionAction : IFollowerAction {
        public GoToPositionAction(Vector2 position, bool noDistance = false) {
            this.position = position;
            this.noDistance = noDistance;
        }

        public Vector2 position;
        public readonly bool noDistance;

        public override string ToString() {
            return "GoToPositionAction " + position;
        }
    }

    public class FollowerController : MonoBehaviour {
        /// <summary>
        /// which walker to follow
        /// </summary>
        public Walker leader;
        /// <summary>
        /// own walker
        /// </summary>
        private Walker _walker;

        public float maxDistance = 0.7f;

        [ShowInInspector] private readonly List<IFollowerAction> _actionList = new();

        private void Start() {
            // get components dynamically since FollowerController is instantiated at runtime
            // except leader which is set by GameManager on instantiation
            // use Start() since leader is not yet set in Awake()
            _walker = GetComponent<Walker>();
            Assert.IsNotNull(_walker);

            Assert.IsNotNull(leader);
            leader.Moved += LeaderOnMoved;
            leader.Transfering += LeaderOnTransfering;
            _walker.Moved += WalkerOnMoved;
        }

        private void Update() {
            if (_actionList.Count == 0) {
                return;
            }

            // check if there is anything to do for the next action
            switch (_actionList[0]) {
                case UseTransferZoneAction transferZoneAction:
                    // we should have reached a transfer zone by now, initiate transfer
                    if (_walker.CanTransfer) {
                        // wait until player has moved away, which we know when transfering is no longer the only action
                        if (_actionList.Count > 1) {
                            _walker.ActivateTransfer(transferZoneAction.direction);
                            // transfer actions are done immediately
                            _actionList.RemoveAt(0);
                        }
                    }
                    else {
                        // oh shit
                    }

                    break;
                case GoToPositionAction goToPositionAction:
                    if (!_walker.IsWalking) {
                        // initiate walking if that has not yet happened
                        Vector2 moveDirection = new(Math.Sign(goToPositionAction.position.x - transform.position.x), 0);
                        _walker.ActivateWalking(moveDirection);
                    }

                    break;
            }
        }

        private void LeaderOnTransfering(Vector2 inputVector) {
            // add transfer if leader has transfered
            // except if leader has transfered directly after transfering, in which case remove transfer because transfering twice is not transfering
            if (_actionList.Count > 0 && _actionList[^1] is UseTransferZoneAction) {
                _actionList.RemoveAt(_actionList.Count - 1);
            }
            else {
                _actionList.Add(new UseTransferZoneAction(inputVector));
            }
        }

        private void LeaderOnMoved(Vector2 newPosition, bool ignoreDistance, bool inTransferZone) {
            if (inTransferZone || (!ignoreDistance && Mathf.Abs(newPosition.x - transform.position.x) < maxDistance)) {
                // do not start moving if not far enough
                // and do not start moving if movement was in transfer zone to avoid jumping
                return;
            }

            // add or update moving
            if (_actionList.Count > 0 && _actionList[^1] is GoToPositionAction) {
                _actionList[^1] = new GoToPositionAction(newPosition, ignoreDistance);
            }
            else {
                _actionList.Add(new GoToPositionAction(newPosition, ignoreDistance));
            }
        }

        /// <param name="newPosition"></param>
        /// <param name="ignoreDistance">ignored for follower</param>
        /// <param name="inTransferZone">ignored for follower</param>
        private void WalkerOnMoved(Vector2 newPosition, bool ignoreDistance, bool inTransferZone) {
            // check if we have moved far enough
            // noDistance is set when the move is to a transfer zone instead of the player
            GoToPositionAction action = (GoToPositionAction)_actionList[0];
            float maxDistanceForThis = action.noDistance ? 0.1f : maxDistance;
            if (Mathf.Abs(newPosition.x - action.position.x) < maxDistanceForThis) {
                _walker.StopWalking();
                _actionList.RemoveAt(0);
            }
        }
    }
}
