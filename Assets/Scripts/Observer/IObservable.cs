namespace Checkers.Observer
{
    public interface IObservable
    {
        void Action(ActionType actionType, BoardCoordinate origin, BoardCoordinate destination);
    }
}