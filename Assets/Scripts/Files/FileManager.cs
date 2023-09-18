﻿using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.Files
{
    public class FileManager : IFileManager
    {
        private readonly string _fileName;

        public FileManager(string fileName)
        {
            _fileName = $"{fileName}.txt";

            // TODO: подумать, как будет перезаписываться файл
            if (!File.Exists(_fileName))
            {
                File.Create(_fileName);
            }
        }

        public async Task WriteLine(string data)
        {
            await using var fileStream = new FileStream(_fileName, FileMode.Append);
            await using var streamWriter = new StreamWriter(fileStream);

            await streamWriter.WriteLineAsync(data);
        }

        public string[] ReadAllLines()
        {
            using var fileStream = new FileStream(_fileName + ".txt", FileMode.Open);
            using var streamReader = new StreamReader(fileStream);

            var builder = new StringBuilder();

            while (!streamReader.EndOfStream)
            {
                builder.AppendLine(streamReader.ReadLine());
            }

            return builder.ToString()
                .Split(Environment.NewLine);
        }
    }
}