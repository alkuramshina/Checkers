using System.Threading.Tasks;

namespace Checkers.Observer
{
    public interface IObserver
    {
        Task Log(ActionType actionType, ColorType playerColor, BoardCoordinate coordinate, BoardCoordinate destinationCoordinate = null);
    }
}