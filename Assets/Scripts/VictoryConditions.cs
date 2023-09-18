using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Checkers
{
    public static class VictoryConditions
    {
        public static bool GameIsOver { get; private set; }
        public static bool CheckIfAnyAlive(this IEnumerable<ChipComponent> chips,
            ColorType playerColor)
            => chips.Any(x => x.GetColor == playerColor);

        public static bool CheckIfAtTheEndOfBoard(this ChipComponent chip,
            CellComponent cell)
            => cell.IsVictoriousFor == chip.GetColor;

        public static void Hooray(ColorType playerColor)
        {
            GameIsOver = true;

            Debug.Log($@"Победа за {(playerColor switch
            { ColorType.White => "белыми",
                ColorType.Black => "черными",
                _ => "кем-то" })}!");
        }
    }
}