using System.Collections;
using System.Collections.Generic;
using Checkers.Observer;
using UnityEngine;

namespace Checkers
{
    public class GameManager: MonoBehaviour
    {
        private const float ChipMovingSpeed = 4f;

        private ChipComponent _selectedChip;

        private CameraMover _cameraMover;
        private ColorType _currentPlayerColor;

        private List<ChipComponent> _chips;
        private List<CellComponent> _cells;

        private IObserver _observer;

        private void Awake()
        {
            TryGetComponent(out _observer);
            
            var boardGenerator = FindObjectOfType<BoardGenerator>();
            (_cells, _chips) = boardGenerator.CreateBoard();
            
            SetFocusHandling(_cells);
            SetFocusHandling(_chips);
            
            SetClickHandling(_cells);
            SetClickHandling(_chips);

            SetEatingHandling(_chips);

            _cameraMover = FindObjectOfType<CameraMover>();
            _currentPlayerColor = ColorType.White;
        }
        
        private void SetFocusHandling(IEnumerable<BaseClickComponent> elements)
        {
            foreach (var boardElement in elements)
            {
                boardElement.OnFocusEventHandler += FocusOn;
            }
        }
        
        private void SetClickHandling(IEnumerable<ChipComponent> chips)
        {
            foreach (var chip in chips)
            {
                chip.OnClickEventHandler += SelectPlayableChip;
            }
        }
        
        private void SetClickHandling(IEnumerable<CellComponent> cells)
        {
            foreach (var cell in cells)
            {
                cell.OnClickEventHandler += MakeMove;
            }
        }
        
        private void SetEatingHandling(IEnumerable<ChipComponent> chips)
        {
            foreach (var chip in chips)
            {
                chip.OnCrossAnotherChipHandler += CheckToEat;
            }
        }

        private void FocusOn(BaseClickComponent component, bool focused)
        {
            if (component.IsHighlighted) return;
            component.SetFocused(focused);
            component.Pair?.SetFocused(focused);
        }

        private void MakeMove(BaseClickComponent nextCell)
        {
            if (!nextCell.IsHighlighted || _selectedChip is null)
            {
                return;
            }

            var nextCellComponent = (CellComponent)nextCell;
            StartCoroutine(Move(_selectedChip, nextCellComponent));

            var currentCell = (CellComponent)_selectedChip.Pair;
            _observer?.Log(ActionType.Moved, _currentPlayerColor, currentCell.Coordinate, 
                nextCellComponent.Coordinate);
            
            RemoveHighlight(_selectedChip);

            _selectedChip.SetNewPair(nextCell);
            nextCell.SetNewPair(_selectedChip);
            
            _selectedChip = null;
        }

        private void SelectPlayableChip(BaseClickComponent chip)
        {
            if (chip.GetColor != _currentPlayerColor)
            {
                return;
            }
            
            if (_selectedChip is not null)
            {
                RemoveHighlight(_selectedChip);
            }

            chip.SetHighlighted(true);

            var cell = (CellComponent)chip.Pair;
            cell.SetHighlighted(true);

            if (chip.GetColor == ColorType.White)
            {
                HighlightNeighborIfAvailable(cell, NeighborType.TopRight);
                HighlightNeighborIfAvailable(cell, NeighborType.TopLeft);
            }
            else
            {
                HighlightNeighborIfAvailable(cell, NeighborType.BottomLeft);
                HighlightNeighborIfAvailable(cell, NeighborType.BottomRight);
            }
            
            _selectedChip = (ChipComponent) chip;
            _observer?.Log(ActionType.Clicked, _currentPlayerColor, cell.Coordinate);
        }

        private void HighlightNeighborIfAvailable(CellComponent currentCell, NeighborType neighborType)
        {
            var availableCell = currentCell.Neighbors[neighborType];
            if (availableCell is null)
            {
                return;
            }
            
            if (availableCell.IsEmpty)
            {
                availableCell.SetHighlighted(true);
            }
            else if (availableCell.Pair.GetColor == BoardGenerator.GetOpponentColor(_currentPlayerColor))
            {
                var cellToMoveAfterEating = availableCell.Neighbors[neighborType];
                if (cellToMoveAfterEating is not null && cellToMoveAfterEating.IsEmpty)
                {
                    cellToMoveAfterEating.SetHighlighted(true);
                }
            }
        }

        private static void RemoveHighlight(ChipComponent chip)
        {
            chip.SetHighlighted(false);
                
            var cell = (CellComponent)chip.Pair;
            cell.SetHighlighted(false);
            
            foreach (var cellNeighbor in cell.Neighbors)
            {
                cellNeighbor.Value?.SetHighlighted(false);
            }
        }

        private IEnumerator Move(ChipComponent chip, CellComponent nextCell)
        {
            while (Vector3.Distance(chip.transform.position, nextCell.transform.position) >= 0.01f)
            {
                chip.transform.position = Vector3.Lerp(chip.transform.position,
                    nextCell.transform.position, ChipMovingSpeed * Time.deltaTime);

                yield return null;
            }

            if (chip.CheckIfAtTheEndOfBoard(nextCell))
            {
                VictoryConditions.Hooray(chip);
                yield break;
            }

            _currentPlayerColor = BoardGenerator.GetOpponentColor(_currentPlayerColor);
            Debug.Log($@"Ход у {(_currentPlayerColor switch
            { ColorType.White => "белых",
                ColorType.Black => "черных",
                _ => "кого-то" })}.");

            yield return StartCoroutine(_cameraMover.MoveCameraToNextPov());
        }

        private void CheckToEat(ChipComponent chip, ChipComponent chipToEat)
        {
            // Если триггер не для фишки игрока или пересечение не с фишкой оппонента
            if (chip.GetColor != _currentPlayerColor
                || chip.GetColor == chipToEat.GetColor)
            {
                return;
            }

            var cellToEatOn = (CellComponent)chipToEat.Pair;
            _observer?.Log(ActionType.Ate, _currentPlayerColor, cellToEatOn.Coordinate);

            chipToEat.SetNewPair(null);
            _chips.Remove(chipToEat);

            if (!_chips.CheckIfAnyAlive(chipToEat.GetColor))
            {
                VictoryConditions.Hooray(chip);
            }

            chipToEat.OnFocusEventHandler -= FocusOn;
            chipToEat.OnClickEventHandler -= SelectPlayableChip;
            chipToEat.OnCrossAnotherChipHandler -= CheckToEat;
            
            Destroy(chipToEat.gameObject);
        }

        private void OnDestroy()
        {
            foreach (var boardElement in _chips)
            {
                boardElement.OnFocusEventHandler -= FocusOn;
            }
            
            foreach (var boardElement in _cells)
            {
                boardElement.OnFocusEventHandler -= FocusOn;
            }
        
            foreach (var chip in _chips)
            {
                chip.OnClickEventHandler -= SelectPlayableChip;
            }
        
            foreach (var cell in _cells)
            {
                cell.OnClickEventHandler -= MakeMove;
            }
        
            foreach (var chip in _chips)
            {
                chip.OnCrossAnotherChipHandler -= CheckToEat;
            }
        }
    }
}