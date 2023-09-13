using UnityEngine;
using UnityEngine.EventSystems;

namespace Checkers
{
    public class ChipComponent : BaseClickComponent
    {
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
        
        protected override Material GetMaterialForSelected()
            => Resources.Load<Material>($"Materials/SelectedCellMaterial");
    }
}