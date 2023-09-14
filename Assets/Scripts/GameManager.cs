using System.Collections.Generic;
using UnityEngine;

namespace Checkers
{
    public class GameManager: MonoBehaviour
    {
        private static BaseClickComponent _selectedChip;
        private void Awake()
        {
            var boardGenerator = FindObjectOfType<BoardGenerator>();
            var (cells, chips) = boardGenerator.CreateBoard();
            
            SetFocusSelection(cells);
            SetFocusSelection(chips);
            
            SetClickHandling(chips);
        }
        
        private static void SetFocusSelection(IEnumerable<BaseClickComponent> elements)
        {
            foreach (var boardElement in elements)
            {
                boardElement.OnFocusEventHandler += (component, select) =>
                {
                    if (!component.IsClicked)
                    {
                        component.SetSelected(select);
                        component.Pair?.SetSelected(select);
                    }
                };
            }
        }
        
        private static void SetClickHandling(IEnumerable<ChipComponent> chips)
        {
            foreach (var chip in chips)
            {
                chip.OnClickEventHandler += SetComponentClicked;
            }
        }

        private static void SetComponentClicked(BaseClickComponent element)
        {
            if (_selectedChip is not null)
            {
                _selectedChip.SetClicked(false);
                _selectedChip.Pair?.SetClicked(false);
            }

            element.SetClicked(true);
            element.Pair?.SetClicked(true);

            _selectedChip = element;
        }
    }
}