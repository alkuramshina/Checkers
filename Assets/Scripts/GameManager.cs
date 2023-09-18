﻿using System.Collections;
using System.Collections.Generic;
using Checkers.Observer;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Checkers
{
    public partial class GameManager: MonoBehaviour
    {
        private const float ChipMovingSpeed = 4f;

        private ChipComponent _selectedChip;

        private CameraMover _cameraMover;
        private ColorType _currentPlayerColor;

        private List<ChipComponent> _chips;
        private CellComponent[,] _cells;

        private IObserver _observer;
        private PhysicsRaycaster _physicsRaycaster;

        private void Awake()
        {
            TryGetComponent(out _observer);
            
            var boardGenerator = FindObjectOfType<BoardGenerator>();
            (_cells, _chips) = boardGenerator.CreateBoard();
            
            SetHandlers(_cells);
            SetHandlers(_chips);

            _physicsRaycaster = FindObjectOfType<PhysicsRaycaster>();
            _cameraMover = FindObjectOfType<CameraMover>();
            _currentPlayerColor = ColorType.White;
        }
        
        private void SetHandlers(CellComponent[,] cells)
        {
            for (var i = 0; i < cells.GetLength(0); i++)
            {
                for (var j = 0; j < cells.GetLength(1); j++)
                {
                    cells[i,j].OnFocusEventHandler += FocusOn;
                    cells[i,j].OnClickEventHandler += MakeMove;
                }
            }
        }
        
        private void SetHandlers(IEnumerable<ChipComponent> chips)
        {
            foreach (var chip in chips)
            {
                chip.OnFocusEventHandler += FocusOn;
                chip.OnClickEventHandler += SelectPlayableChip;
                chip.OnCrossAnotherChipHandler += Eat;
            }
        }

        private void FocusOn(BaseClickComponent component, bool focused)
        {
            if (component.IsHighlighted)
            {
                return;
            }
            
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
                GameIsOver();
                yield break;
            }

            _currentPlayerColor = BoardGenerator.GetOpponentColor(_currentPlayerColor);
            Debug.Log($@"Ход у {(_currentPlayerColor switch
            { ColorType.White => "белых",
                ColorType.Black => "черных",
                _ => "кого-то" })}.");

            yield return StartCoroutine(_cameraMover.MoveCameraToNextPov());
        }

        private void Eat(ChipComponent chipToEat)
        {
            // уже съели
            if (chipToEat is null)
            {
                return;
            }
            
            // Если триггер не для фишки игрока или пересечение не с фишкой оппонента
            if (_currentPlayerColor == chipToEat.GetColor)
            {
                return;
            }

            var cellToEatOn = (CellComponent)chipToEat.Pair; 
            chipToEat.SetNewPair(null);
            _chips.Remove(chipToEat);

            if (!_chips.CheckIfAnyAlive(chipToEat.GetColor))
            {
                GameIsOver();
            }

            chipToEat.OnFocusEventHandler -= FocusOn;
            chipToEat.OnClickEventHandler -= SelectPlayableChip;
            chipToEat.OnCrossAnotherChipHandler -= Eat;
            
            Destroy(chipToEat.gameObject);
            
            _observer?.Log(ActionType.Ate, _currentPlayerColor, cellToEatOn.Coordinate);
        }

        private void GameIsOver()
        {
            VictoryConditions.Hooray(_currentPlayerColor);
            _physicsRaycaster.enabled = false;
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
                chip.OnCrossAnotherChipHandler -= Eat;
            }
        }
    }
}