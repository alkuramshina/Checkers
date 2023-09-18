using System.Threading.Tasks;

namespace Checkers.Observer
{
    public interface IObserver
    {
        Task Log(ActionType actionType, ColorType playerColor, BoardCoordinate coordinate, BoardCoordinate destinationCoordinate = null);
    }

    public interface IObservable
    {
        void MoveFromTo(BoardCoordinate origin, BoardCoordinate destination);
        void EatOn(BoardCoordinate coordinate);
        void ClickOn(BoardCoordinate coordinate);
    }

    public enum ActionType
    {
        Clicked = 1,
        Moved,
        Ate
    }
}