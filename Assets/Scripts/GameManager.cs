using UnityEngine;

namespace Checkers
{
    public class GameManager: MonoBehaviour
    {
        private BoardGenerator _boardGenerator;
        private void Awake()
        {
            _boardGenerator = FindObjectOfType<BoardGenerator>();
            var (cells, chips) = _boardGenerator.StartGame();
        }
    }
}