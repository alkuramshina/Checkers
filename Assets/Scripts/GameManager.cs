using System.Collections.Generic;
using UnityEngine;

namespace Checkers
{
    public class GameManager: MonoBehaviour
    {
        private static float chipMovingSpeed = 4f; 
        
        private static ChipComponent _selectedChip;

        private static CameraMover _cameraMover;
        private static ColorType _currentPlayerColor;
        
        private void Awake()
        {
            var boardGenerator = FindObjectOfType<BoardGenerator>();
            var (cells, chips) = boardGenerator.CreateBoard();
            
            SetFocusHandling(cells);
            SetFocusHandling(chips);
            
            SetClickHandling(cells);
            SetClickHandling(chips);

            _cameraMover = FindObjectOfType<CameraMover>();

            PassTheTurn(ColorType.White);
        }
        
        private static void SetFocusHandling(IEnumerable<BaseClickComponent> elements)
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
        
        private static void SetClickHandling(IEnumerable<ChipComponent> chips)
        {
            foreach (var chip in chips)
            {
                chip.OnClickEventHandler += SelectPlayableChip;
            }
        }
        
        private static void SetClickHandling(IEnumerable<CellComponent> cells)
        {
            foreach (var cell in cells)
            {
                cell.OnClickEventHandler += MoveChip;
            }
        }

        private static void MoveChip(BaseClickComponent cell)
        {
            if (!cell.IsHighlighted || _selectedChip is null)
            {
                return;
            }

            _selectedChip.transform.position = Vector3.Lerp(_selectedChip.transform.position,
                cell.transform.position, chipMovingSpeed * Time.deltaTime);
            _selectedChip.Pair = cell;
            
            RemoveHighlight(_selectedChip);
            
            _selectedChip = null;
            
            PassTheTurn();
           // StartCoroutine(_cameraMover.MoveCameraToNextPov());
        }

        private static void SelectPlayableChip(BaseClickComponent chip)
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
                HighlightNeighborIfExists(cell, NeighborType.TopRight);
                HighlightNeighborIfExists(cell, NeighborType.TopLeft);
            }
            else
            {
                HighlightNeighborIfExists(cell, NeighborType.BottomLeft);
                HighlightNeighborIfExists(cell, NeighborType.BottomRight);
            }
            
            _selectedChip = (ChipComponent) chip;
        }

        private static void HighlightNeighborIfExists(CellComponent cell, NeighborType neighborType)
        {
            if (cell.Neighbors[neighborType] is not null 
                && cell.Neighbors[neighborType].Pair is null)
            {
                cell.Neighbors[neighborType].SetHighlighted(true);
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

        private static void PassTheTurn(ColorType? color = null)
        {
            _currentPlayerColor = color ?? BoardGenerator.SwapColor(_currentPlayerColor);
            Debug.Log(
                $"Ход у {(color switch { ColorType.White => "белых", ColorType.Black => "черных", _ => "кого-то" })}.");
        }
    }
}