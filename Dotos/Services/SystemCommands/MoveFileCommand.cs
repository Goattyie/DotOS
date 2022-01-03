using Dotos.Models;
using Dotos.Utils;
using System.Text.RegularExpressions;

namespace Dotos.Services.SystemCommands
{
    internal class MoveFileCommand : ISystemCommand
    {
        private readonly FileSystem _fileSystem;
        private readonly Session _session;
        private readonly CopyFileCommand _copyCommand;
        private readonly RemoveFileCommand _removeCommand;

        public MoveFileCommand(FileSystem fs, Session session, CopyFileCommand copyCommand, RemoveFileCommand removeCommand)
        {
            _copyCommand = copyCommand;
            _removeCommand = removeCommand;
            _fileSystem = fs;
            _session = session;
        }
        public bool CanExecute(string command) => Regex.IsMatch(command, "move.file ([/A-Za-z1-9]+) ([/A-Za-z1-9]+)");

        public async Task Execute(string command)
        {
            var arr = command.Split(' ');
            var path1Str = arr[1];
            var path2Str = arr[2];

            var path1 = path1Str.GetPath();
            var path2 = path2Str.GetPath();
            //Проверяем правильность указанных путей

            var dir1 = await _fileSystem.GetDir(path1.DirectoryPath ?? _session.CurrentDirectory);
            var dir2 = await _fileSystem.GetDir(path2.DirectoryPath ?? _session.CurrentDirectory);

            _session.CanWrite(dir2);
            _session.CanRead(dir1);
            var filesDir1 = (await _fileSystem.ReadData(_fileSystem.StartDataAreaByte + _fileSystem.ClusterSize * dir1.Cluster, _fileSystem.ClusterSize)).AsFileList();
            var filesDir2 = (await _fileSystem.ReadData(_fileSystem.StartDataAreaByte + _fileSystem.ClusterSize * dir2.Cluster, _fileSystem.ClusterSize)).AsFileList();

            var file1 = filesDir1.Find(x => x.Name == path1.Filename && x.Type == 0);
            var file2 = filesDir2.Find(x => x.Name == path2.Filename && x.Type == 0);

            _session.CanRead(file1);

            await _copyCommand.Execute($"copy.file {arr[1]} {arr[2]}");
            await _removeCommand.Execute($"remove.file {arr[1]}");

            await _fileSystem.ResizeParentDirectory(dir2, dir2.Size + FileModel.SizeInBytes);
            await _fileSystem.ResizeParentDirectory(dir1, dir1.Size - FileModel.SizeInBytes);
        }

        public void Info()
        {
            Console.WriteLine("move.file");
        }
    }
}
