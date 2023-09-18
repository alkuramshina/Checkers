using System.Threading.Tasks;

namespace Checkers.Files
{
    // Всегда можно добавить реализацию не в физической файловой системе
    public interface IFileManager
    {
        Task WriteLineAsync(string data);
        string[] ReadAllLines();
    }
}