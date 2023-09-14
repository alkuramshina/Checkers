using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Checkers
{
    public static class VictoryConditions
    {
        public static bool CheckIfAnyAlive(this IEnumerable<ChipComponent> chips,
            ColorType playerColor)
            => chips.Any(x => x.GetColor == playerColor);

        public static void Hooray(ColorType playerColor)
            => Debug.Log($@"Победа за {(playerColor switch
                { ColorType.White => "белыми",
                    ColorType.Black => "черными",
                    _ => "кем-то" })}");
    }
}