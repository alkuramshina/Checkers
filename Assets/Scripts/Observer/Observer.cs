using System.Threading.Tasks;
using Checkers.Files;
using UnityEngine;

namespace Checkers.Observer
{
    public class Observer: MonoBehaviour, IObserver
    {
        private IFileManager _fileManager;

        // Научите меня DI в Unity~, будем считать тут место для внедрения нужного нам обработчика записи и чтения
        private void Awake()
        {
            _fileManager = new FileManager("Data/Save");
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
    }
}