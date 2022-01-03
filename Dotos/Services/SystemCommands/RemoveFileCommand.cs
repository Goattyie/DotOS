using Dotos.Utils;
using System.Text.RegularExpressions;

namespace Dotos.Services.SystemCommands
{
    internal class RemoveFileCommand : ISystemCommand
    {
        private readonly FileSystem _fileSystem;
        private readonly Session _session;


        public RemoveFileCommand(FileSystem fs, Session session)
        {
            _fileSystem = fs;
            _session = session;
        }

        public bool CanExecute(string command) => Regex.IsMatch(command, "remove.file ([/A-Za-z1-9]+)");

        public async Task Execute(string command)
        {
            var pathStr = command.Split(' ')[1];
            var path = pathStr.GetPath();
            //var path
            //Считываем файл из диска
            var directory = await _fileSystem.GetDir(path.DirectoryPath ?? _session.CurrentDirectory);
            var data = await _fileSystem.ReadData(_fileSystem.StartDataAreaByte + _fileSystem.ClusterSize * directory.Cluster, _fileSystem.ClusterSize);
            var list = data.AsFileList();
            var file = list.Find(x => x.Name == path.Filename && x.Type == 0) ?? throw new Exception($"File {command.Split(' ')[1]} not found");

            //Удаляем файл из папки
            list.Remove(file);

            //Очищаем данные директории
            var directoryStartPosition = _fileSystem.StartDataAreaByte + directory.Cluster * _fileSystem.ClusterSize;
            await _fileSystem.WriteData(new byte[_fileSystem.ClusterSize], directoryStartPosition);

            foreach (var x in list)
            {
                await _fileSystem.WriteData((byte[])x, directoryStartPosition, directoryStartPosition + _fileSystem.ClusterSize);
            }


            var cluster = file.Cluster;
            //Удаляем данные из области данных и фат таблицы
            //Получаем статус кластера
            while (true)
            {
                var nextClusterData = await _fileSystem.ReadData(_fileSystem.StartTableByte + cluster * 4 - 4, 4);
                //Удаляем значение фат-таблицы
                await _fileSystem.WriteData(new byte[4], _fileSystem.StartTableByte + cluster * 4 - 4);
                await _fileSystem.WriteData(new byte[4], _fileSystem.StartTableByte + _fileSystem.TableSize + cluster * 4 - 4);
                //Очищаем данные с области данных
                await _fileSystem.WriteData(new byte[_fileSystem.ClusterSize], _fileSystem.StartDataAreaByte + cluster * _fileSystem.ClusterSize);

                var nextCluster = nextClusterData.ToInt();

                if (nextCluster == 1)
                    break;

                cluster = nextCluster;
            }

            await _fileSystem.ResizeParentDirectory(directory, directory.Size - 33);
        }

        public void Info()
        {
            Console.WriteLine("remove.file *name*\t remove file");
        }
    }
}
