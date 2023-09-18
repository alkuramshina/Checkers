using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Checkers.Files;
using UnityEngine;

namespace Checkers.Observer
{
    public class Observer : MonoBehaviour, IObserver
    {
        private IFileManager _fileManager;

        private void Awake()
        {
            _fileManager = new FileManager("DataSave.txt");
        }

        public async Task Log(ActionType actionType, ColorType playerColor,
            BoardCoordinate coordinate,
            BoardCoordinate destinationCoordinate = null)
        {
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

            await _fileManager.WriteLineAsync(actionData);
        }

        public void Replay()
        {
            var moveList = _fileManager.ReadAllLines();
            foreach (var move in moveList)
            {
                Replay(move);
            }
        }

        public void Replay(string actionData)
        {
            var (originCoordinate, destinationCoordinate) = GetCoordinates(actionData);
            var playerColor = GetPlayerColor(actionData);
            var actionType = GetActionType(actionData);
        }

        private (BoardCoordinate, BoardCoordinate) GetCoordinates(string actionData)
        {
            var coordinatesMatch = Regex.Matches(actionData, BoardCoordinateExtensions.BoardCoordinatePattern);
            var originCoordinate = new BoardCoordinate(int.Parse(coordinatesMatch[0].Groups[1].Value),
                int.Parse(coordinatesMatch[0].Groups[2].Value));

            if (coordinatesMatch.Count == 1)
            {
                return (originCoordinate, null);
            }

            var destinationCoordinate = new BoardCoordinate(int.Parse(coordinatesMatch[1].Groups[1].Value),
                int.Parse(coordinatesMatch[1].Groups[2].Value));

            return (originCoordinate, destinationCoordinate);
        }

        private ActionType GetActionType(string actionData)
        {
            var actionTypePattern = $"({ActionType.Moved}|{ActionType.Clicked}|{ActionType.Ate})";
            var actionMatch = Regex.Match(actionData, actionTypePattern);
            
            return Enum.Parse<ActionType>(actionMatch.Groups[1].Value);
        }

        private ColorType GetPlayerColor(string actionData)
        {
            var playerPattern = $"({ColorType.White}|{ColorType.Black}) player";
            var playerMatch = Regex.Match(actionData, playerPattern);
            
            return Enum.Parse<ColorType>(playerMatch.Groups[1].Value);
        }
    }
}