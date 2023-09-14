using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Checkers
{
    public class GameManager: MonoBehaviour
    {
        private static float chipMovingSpeed = 2f; 
        
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

        private void MakeMove(BaseClickComponent cell)
        {
            if (!cell.IsHighlighted || _selectedChip is null)
            {
                return;
            }

            StartCoroutine(MoveChip(_selectedChip, (CellComponent) cell));

            RemoveHighlight(_selectedChip);
            
            _selectedChip.Pair = cell;
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

        private void HighlightNeighborIfExists(CellComponent cell, NeighborType neighborType)
        {
            if (cell.Neighbors[neighborType] is not null 
                && cell.Neighbors[neighborType].Pair is null)
            {
                cell.Neighbors[neighborType].SetHighlighted(true);
            }
        }

        private void RemoveHighlight(ChipComponent chip)
        {
            chip.SetHighlighted(false);
                
            var cell = (CellComponent)chip.Pair;
            cell.SetHighlighted(false);
            
            foreach (var cellNeighbor in cell.Neighbors)
            {
                cellNeighbor.Value?.SetHighlighted(false);
            }
        }

        private void PassTheTurn()
        {
            _currentPlayerColor = BoardGenerator.SwapColor(_currentPlayerColor);
            StartCoroutine(_cameraMover.MoveCameraToNextPov());
            Debug.Log(
                $@"Ход у {(_currentPlayerColor switch
                { ColorType.White => "белых",
                    ColorType.Black => "черных",
                    _ => "кого-то" })}.");
        }

        private IEnumerator MoveChip(ChipComponent chip, CellComponent cell)
        {
            while (Vector3.Distance(chip.transform.position, cell.transform.position) >= 0.01f)
            {
                chip.transform.position = Vector3.Lerp(chip.transform.position,
                    cell.transform.position, chipMovingSpeed * Time.deltaTime);

                yield return null;
            }

            yield return new WaitForSeconds(2);
            
            PassTheTurn();
        }
    }
}