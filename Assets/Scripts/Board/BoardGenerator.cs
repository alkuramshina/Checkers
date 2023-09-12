using UnityEngine;

namespace Checkers
{
    public class BoardGenerator: MonoBehaviour
    {
        [SerializeField] private int rows = 8;
        [SerializeField] private int columns = 8;
        
        [SerializeField] private int startingChipRows = 3;
        
        [SerializeField] private ColorType playableCellColor = ColorType.Black;

        [SerializeField] private CellComponent cellPrefab;
        [SerializeField] private ChipComponent chipPrefab;

        private float CellOffsetX => cellPrefab.transform.localScale.x / 2;
        private float CellOffsetZ => cellPrefab.transform.localScale.z / 2;

        private void Start()
        {
            var currentCellColor = ColorType.Black;
            
            for (var row = 0; row < rows; row++)
            {
                for (var column = 0; column < columns; column++)
                {
                    var position = new Vector3(CellOffsetX + row, 0, CellOffsetZ + column);
                    CreateCell(position, currentCellColor);
                    
                    if (currentCellColor == playableCellColor 
                        && 0 <= row && row <= startingChipRows)
                    {
                        CreateChip(position, ColorType.White);
                    } 
                    else if (currentCellColor == playableCellColor
                             && rows >= row && row >= rows - startingChipRows)
                    {
                        CreateChip(position, ColorType.Black);
                    }

                    currentCellColor = SwapColor(currentCellColor);
                }
                
                currentCellColor = SwapColor(currentCellColor);
            }
        }

        private static ColorType SwapColor(ColorType currentColor)
            => currentColor == ColorType.Black ? ColorType.White : ColorType.Black;

        private CellComponent CreateCell(Vector3 cellPosition, ColorType color)
        {
            CellComponent newCell = Instantiate(cellPrefab, cellPosition, Quaternion.Euler(90f, 0f, 0f), transform);
            newCell.SetColor(color);

            return newCell;
        }
        
        private ChipComponent CreateChip(Vector3 chipPosition, ColorType color)
        {
            ChipComponent newChip = Instantiate(chipPrefab, chipPosition, Quaternion.identity, transform);
            newChip.SetColor(color);

            return newChip;
        }
    }
}