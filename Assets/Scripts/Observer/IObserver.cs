using System.Threading.Tasks;

namespace Checkers.Observer
{
    public interface IObserver
    {
        Task Log(ActionType actionType, ColorType playerColor, BoardCoordinate coordinate, BoardCoordinate destinationCoordinate = null);
    }

    public enum ActionType
    {
        Clicked = 1,
        Moved,
        Ate
    }
}

// выбор фишек, выбор клеток для перемещения фишек и уничтожение фишек оппонента
// White player clicked on 1,1
// White player moved from 1,1 to 2,2
// White player ate on 2,2