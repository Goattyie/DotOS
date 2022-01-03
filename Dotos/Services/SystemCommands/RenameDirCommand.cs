using Dotos.Models;
using Dotos.Utils;
using Dotos.Utils.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Dotos.Services.SystemCommands
{
    internal class RenameDirCommand : ISystemCommand
    {
        private readonly FileSystem _fileSystem;
        private readonly Session _session;


        public RenameDirCommand(FileSystem fs, Session session)
        {
            _fileSystem = fs;
            _session = session;
        }
        public bool CanExecute(string command) => Regex.IsMatch(command, "rename.dir ([A-Za-zА-Яа-я1-9])+ ([A-Za-zА-Яа-я1-9])+");

        public async Task Execute(string command)
        {
            if (_session.CurrentDirectory.Split('/').Length <= 1)
                throw new Exception("Can't execute this command there");

            var arr = command.Split(' ');
            if (arr[1].Length > 10 || arr[2].Length > 10)
                throw new Exception("File length can be not more 10 symbols");

            var oldName = arr[1];
            var newName = arr[2];


            var dir = await _fileSystem.GetDir(_session.CurrentDirectory);
            _session.CanWrite(dir);
            var data = await _fileSystem.ReadData(_fileSystem.StartDataAreaByte + dir.Cluster * _fileSystem.ClusterSize, _fileSystem.ClusterSize);
            var list = data.AsFileList();
            var file = list.Find(x => x.Name == oldName && x.Type == 1) ?? throw new NotFileException();
            var index = list.IndexOf(file);
            _session.CanWrite(file);
            var newNameBytes = new byte[10];
            newName.ToBytes().CopyTo(newNameBytes, 0);
            await _fileSystem.WriteData(newNameBytes, _fileSystem.StartDataAreaByte + dir.Cluster * _fileSystem.ClusterSize + index * FileModel.SizeInBytes);
        }


        public void Info()
        {
            Console.WriteLine("rename.dir");
        }
    }
}
