using System;
using Checkers.Observer;

namespace Checkers
{
    public partial class GameManager: IObservable
    {
        public void Action(ActionType actionType, BoardCoordinate origin, BoardCoordinate destination)
        {
            switch (actionType)
            {
                case ActionType.Clicked:
                    var chipToSelect = (ChipComponent) _cells[origin.Row, origin.Column].Pair;
                    SelectPlayableChip(chipToSelect);
                    break;
                case ActionType.Moved:
                    var nextCell = _cells[destination.Row, destination.Column];
                    MakeMove(nextCell);
                    break;
                case ActionType.Ate:
                    var chipToEat = (ChipComponent)_cells[origin.Row, origin.Column].Pair;
                    Eat(chipToEat);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(actionType), actionType, null);
            }
        }
    }
}