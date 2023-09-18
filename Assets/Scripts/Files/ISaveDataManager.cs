using System.Collections.Generic;
using System.Threading.Tasks;

namespace Checkers.Files
{
    // Всегда можно добавить реализацию не в физической файловой системе
    public interface ISaveDataManager
    {
        Task WriteActionAsync(string data);
        Stack<string> ReadActions();
        void ClearSave();
    }
}