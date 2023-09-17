using UnityEngine;

namespace Checkers
{
    public class ChipCrosser: MonoBehaviour
    {
        private ChipComponent _chip;

        private void Awake()
        {
            _chip = GetComponent<ChipComponent>();
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent<ChipComponent>(out var otherChip))
            {
                _chip.OnCrossAnotherChip(otherChip);
            }
        }
    }
}