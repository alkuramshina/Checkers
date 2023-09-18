using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Checkers.Files
{
    public class FileSaveDataManager : ISaveDataManager
    {
        private readonly string _fileName;

        public FileSaveDataManager(string fileName)
        {
            _fileName = fileName.EndsWith(".txt") ? fileName : $"{fileName}.txt";
        }

        public async Task WriteActionAsync(string data)
        {
            await using var fileStream = new FileStream(_fileName, FileMode.Append);
            await using var streamWriter = new StreamWriter(fileStream);

            await streamWriter.WriteLineAsync(data);
        }

        public Stack<string> ReadActions()
        {
            var actionList = new List<string>();
            
            using var fileStream = new FileStream(_fileName, FileMode.Open);
            using var streamReader = new StreamReader(fileStream);

            while (!streamReader.EndOfStream)
            {
                actionList.Add(streamReader.ReadLine());
            }

            actionList.Reverse();
            return new Stack<string>(actionList);
        }

        public void ClearSave()
        {
            if (File.Exists(_fileName))
            {
                File.Delete(_fileName);
            }
        }
    }
}