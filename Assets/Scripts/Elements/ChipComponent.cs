using UnityEngine;
using UnityEngine.EventSystems;

namespace Checkers
{
    public class ChipComponent : BaseClickComponent
    {
        public event CrossAnotherChipHandler OnCrossAnotherChipHandler;
        
        public void OnCrossAnotherChip(ChipComponent otherChip)
        {
            OnCrossAnotherChipHandler?.Invoke(otherChip);
        }
        
        public override void OnPointerEnter(PointerEventData eventData)
        {
            CallBackEvent((CellComponent)Pair, true);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            CallBackEvent((CellComponent)Pair, false);
        }
        
        protected override Material GetBaseMaterial()
            => Resources.Load<Material>($"Materials/{GetColor}ChipMaterial");
        
        protected override Material GetMaterialForHighlighted()
            => Resources.Load<Material>($"Materials/SelectedCellMaterial");
    }
    
    public delegate void CrossAnotherChipHandler(ChipComponent crossedChip);
}