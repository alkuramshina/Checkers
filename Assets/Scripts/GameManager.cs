using System.Collections;
using System.Collections.Generic;
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

        private void Awake()
        {
            var boardGenerator = FindObjectOfType<BoardGenerator>();
            (_cells, _chips) = boardGenerator.CreateBoard();
            
            SetFocusHandling(_cells);
            SetFocusHandling(_chips);
            
            SetClickHandling(_cells);
            SetClickHandling(_chips);

            _cameraMover = FindObjectOfType<CameraMover>();
            _currentPlayerColor = ColorType.White;
        }
        
        private void SetFocusHandling(IEnumerable<BaseClickComponent> elements)
        {
            foreach (var boardElement in elements)
            {
                boardElement.OnFocusEventHandler += (component, focus) =>
                {
                    if (!component.IsHighlighted)
                    {
                        component.SetFocused(focus);
                        component.Pair?.SetFocused(focus);
                    }
                };
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

        private void MakeMove(BaseClickComponent nextCell)
        {
            if (!nextCell.IsHighlighted || _selectedChip is null)
            {
                return;
            }

            StartCoroutine(Move(_selectedChip, (CellComponent) nextCell));

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
                if (cellToMoveAfterEating.IsEmpty)
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

            if (nextCell.IsVictorious)
            {
                VictoryConditions.Hooray(chip.GetColor);
            }

            CheckToEat(chip, nextCell);

            _currentPlayerColor = BoardGenerator.GetOpponentColor(_currentPlayerColor);
            Debug.Log($@"Ход у {(_currentPlayerColor switch
            { ColorType.White => "белых",
                ColorType.Black => "черных",
                _ => "кого-то" })}.");

            yield return StartCoroutine(_cameraMover.MoveCameraToNextPov());
        }

        private void CheckToEat(ChipComponent chip, CellComponent nextCell)
        {
            var currentCell = (CellComponent)chip.Pair;
            if (currentCell.Neighbors.ContainsValue(nextCell))
            {
                return;
            }

            // не next, а ту, через которую перескакиваем. надо подумать
            var chipToBeEaten = (ChipComponent)nextCell.Pair;

            nextCell.SetNewPair(null);
            _chips.Remove(chipToBeEaten);

            if (!_chips.CheckIfAnyAlive(chipToBeEaten.GetColor))
            {
                VictoryConditions.Hooray(chip.GetColor);
            }

            Destroy(chipToBeEaten);
        }
    }
}