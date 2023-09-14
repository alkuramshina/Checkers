﻿using System.Collections.Generic;
using UnityEngine;

namespace Checkers
{
    public class BoardGenerator : MonoBehaviour
    {
        [SerializeField] private int rows = 8;
        [SerializeField] private int columns = 8;

        [SerializeField] private int startingChipRows = 3;

        [SerializeField] private ColorType playableCellColor = ColorType.Black;

        [SerializeField] private CellComponent cellPrefab;
        [SerializeField] private ChipComponent chipPrefab;

        private float CellOffsetX => cellPrefab.transform.localScale.x / 2;
        private float CellOffsetZ => cellPrefab.transform.localScale.z / 2;

        public (IEnumerable<CellComponent>, IEnumerable<ChipComponent>) CreateBoard()
        {
            if (FindAnyObjectByType<CellComponent>() is not null)
            {
                Debug.LogError("Game has already started!");
            }

            var board = new CellComponent[rows, columns];

            var currentCellColor = ColorType.Black;
            var chips = new List<ChipComponent>();

            for (var row = 0; row < rows; row++)
            {
                for (var column = 0; column < columns; column++)
                {
                    var position = new Vector3(CellOffsetX + row, 0, CellOffsetZ + column);

                    CellComponent newCell = CreateCell(position, currentCellColor);
                    ChipComponent possibleNewChip = null;

                    // если это начало доски и мы на играбельном цвете - белые шашки
                    if (currentCellColor == playableCellColor && 0 <= row && row <= startingChipRows)
                    {
                        possibleNewChip = CreateChip(newCell, ColorType.White);
                    }
                    else
                        // если это конец доски и мы на играбельном цвете - черные шашки
                    if (currentCellColor == playableCellColor && rows >= row && row >= rows - startingChipRows)
                    {
                        possibleNewChip = CreateChip(newCell, ColorType.Black);
                    }

                    if (possibleNewChip is not null)
                    {
                        newCell.Pair = possibleNewChip;
                        chips.Add(possibleNewChip);
                    }

                    board[row, column] = newCell;

                    currentCellColor = SwapColor(currentCellColor);
                }

                currentCellColor = SwapColor(currentCellColor);
            }

            return (ConfigureCells(board), chips);
        }

        public static ColorType SwapColor(ColorType currentColor)
            => currentColor == ColorType.Black ? ColorType.White : ColorType.Black;

        private CellComponent CreateCell(Vector3 cellPosition, ColorType color)
        {
            CellComponent newCell = Instantiate(cellPrefab, cellPosition, Quaternion.Euler(90f, 0f, 0f), transform);
            newCell.SetColor(color);

            return newCell;
        }

        private ChipComponent CreateChip(CellComponent cell, ColorType color)
        {
            ChipComponent newChip = Instantiate(chipPrefab, cell.transform.position, Quaternion.identity, transform);
            newChip.SetColor(color);
            newChip.Pair = cell;

            return newChip;
        }

        private IEnumerable<CellComponent> ConfigureCells(CellComponent[,] board)
        {
            var cells = new List<CellComponent>();
            for (var row = 0; row < rows; row++)
            {
                for (var column = 0; column < columns; column++)
                {
                    var cellNeighbors = new Dictionary<NeighborType, CellComponent>();
                    if (row - 1 >= 0 && column - 1 >= 0)
                    {
                        cellNeighbors.Add(NeighborType.BottomLeft, board[row - 1, column - 1]);
                    }
                    
                    if (row - 1 >= 0 && column + 1 < columns)
                    {
                        cellNeighbors.Add(NeighborType.BottomRight, board[row - 1, column + 1]);
                    }
                    
                    if (row + 1 < rows && column - 1 >= 0)
                    {
                        cellNeighbors.Add(NeighborType.TopLeft, board[row + 1, column - 1]);
                    }
                    
                    if (row + 1 < rows && column + 1 < columns)
                    {
                        cellNeighbors.Add(NeighborType.TopRight, board[row + 1, column + 1]);
                    }
                    
                    board[row, column].Configuration(cellNeighbors);
                    cells.Add(board[row, column]);
                }
            }

            return cells;
        }
    }
}