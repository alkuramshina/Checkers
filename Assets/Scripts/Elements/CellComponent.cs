using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Checkers
{
    public class CellComponent : BaseClickComponent
    {
        public Dictionary<NeighborType, CellComponent> Neighbors { get; private set; }
        public bool IsEmpty => Pair is null;
        public ColorType? IsVictoriousFor { get; private set; }
        public BoardCoordinate Coordinate { get; private set; }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            CallBackEvent(this, true);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            CallBackEvent(this, false);
        }

        protected override Material GetBaseMaterial()
            => Resources.Load<Material>($"Materials/{GetColor}CellMaterial");
        
        protected override Material GetMaterialForHighlighted()
            => Resources.Load<Material>($"Materials/SelectedChipMaterial");

        /// <summary>
        /// Конфигурирование связей клеток
        /// </summary>
		public void Configuration(int row, int column,
            Dictionary<NeighborType, CellComponent> neighbors)
		{
            if (Neighbors != null)
            {
                return;
            }
            
            Neighbors = neighbors;
            Coordinate = new BoardCoordinate(row, column);
            
            IsVictoriousFor = neighbors[NeighborType.TopLeft] is null && neighbors[NeighborType.TopRight] is null
                ? ColorType.White
                : neighbors[NeighborType.BottomLeft] is null &&
                  neighbors[NeighborType.BottomRight] is null
                    ? ColorType.Black
                    : null;
        }
    }

    /// <summary>
    /// Тип соседа клетки
    /// </summary>
    public enum NeighborType : byte
    {
        /// <summary>
        /// Клетка сверху и слева от данной
        /// </summary>
        TopLeft,
        /// <summary>
        /// Клетка сверху и справа от данной
        /// </summary>
        TopRight,
        /// <summary>
        /// Клетка снизу и слева от данной
        /// </summary>
        BottomLeft,
        /// <summary>
        /// Клетка снизу и справа от данной
        /// </summary>
        BottomRight
    }
}