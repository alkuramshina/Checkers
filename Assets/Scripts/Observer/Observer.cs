using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Checkers.Files;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Checkers.Observer
{
    public class Observer : MonoBehaviour, IObserver
    {
        [SerializeField] private bool replayModeOn;
        [SerializeField] private float replayMoveDelay;
        
        private PhysicsRaycaster _raycaster;
        private ISaveDataManager _saveDataManager;
        private Stack<string> _actions;

        private IObservable _observable;

        private void Awake()
        {
            _saveDataManager = new FileSaveDataManager("DataSave.txt");
            _observable = GetComponent<IObservable>();
            _raycaster = FindObjectOfType<PhysicsRaycaster>();
        }

        private void Start()
        {
            if (!replayModeOn)
            {
                _raycaster.enabled = true;
                _saveDataManager.ClearSave();
            }
            else
            {
                _raycaster.enabled = false;

                _actions = _saveDataManager.ReadActions();
                StartCoroutine(Replay());
            }
        }
        
        // White player clicked on 1,1
        // White player moved from 1,1 to 2,2
        // White player ate on 2,2
        public async Task Log(ActionType actionType, ColorType playerColor,
            BoardCoordinate coordinate,
            BoardCoordinate destinationCoordinate = null)
        {
            if (replayModeOn)
            {
                return;
            }
            
            var actionData = $"{playerColor} player {actionType} ";

            if (actionType == ActionType.Moved
                && destinationCoordinate is not null)
            {
                actionData += $"from {coordinate.ToLogString()} to {destinationCoordinate.ToLogString()}";
            }
            else
            {
                actionData += $"on {coordinate.ToLogString()}";
            }

            Debug.Log(actionData);
            await _saveDataManager.WriteActionAsync(actionData);
        }

        private IEnumerator Replay()
        {
            yield return new WaitForSeconds(replayMoveDelay);
            
            while (_actions.Count > 0)
            {
                Replay(_actions.Pop());
                yield return new WaitForSeconds(replayMoveDelay);
            }
            
            _raycaster.enabled = true;
            replayModeOn = false;
        }

        private void Replay(string actionData)
        {
            var (originCoordinate, destinationCoordinate) = GetCoordinates(actionData);
            var actionType = GetActionType(actionData);

            Debug.Log(actionData);
            
            _observable.Action(actionType, originCoordinate, destinationCoordinate);
        }

        private static (BoardCoordinate, BoardCoordinate) GetCoordinates(string actionData)
        {
            var coordinatesMatch = Regex.Matches(actionData, BoardCoordinateExtensions.BoardCoordinatePattern);
            var originCoordinate = (coordinatesMatch[0].Groups[1].Value,
                    coordinatesMatch[0].Groups[2].Value)
                .ToBoardCoordinate();

            if (coordinatesMatch.Count == 1)
            {
                return (originCoordinate, null);
            }

            var destinationCoordinate = (coordinatesMatch[1].Groups[1].Value,
                    coordinatesMatch[1].Groups[2].Value)
                .ToBoardCoordinate();

            return (originCoordinate, destinationCoordinate);
        }

        private static ActionType GetActionType(string actionData)
        {
            var actionTypePattern = $"({ActionType.Moved}|{ActionType.Clicked}|{ActionType.Ate})";
            var actionMatch = Regex.Match(actionData, actionTypePattern);
            
            return Enum.Parse<ActionType>(actionMatch.Groups[1].Value);
        }
    }
}