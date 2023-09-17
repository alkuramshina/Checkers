using UnityEngine;
using UnityEngine.EventSystems;

namespace Checkers
{
    public class ChipComponent : BaseClickComponent
    {
        public event CrossAnotherChipHandler OnCrossAnotherChipHandler;
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

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent<ChipComponent>(out var otherChip))
            {
                OnCrossAnotherChipHandler?.Invoke(this, otherChip);
            }
        }
    }
    
    public delegate void CrossAnotherChipHandler(ChipComponent playerChip, ChipComponent crossedChip);
}