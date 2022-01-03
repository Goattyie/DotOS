using Dotos.Models;
using Dotos.Utils;
using System.Text.RegularExpressions;

namespace Dotos.Services.SystemCommands
{
    internal class CopyFileCommand : ISystemCommand
    {
        private readonly FileSystem _fileSystem;
        private readonly Session _session;


        public CopyFileCommand(FileSystem fs, Session session)
        {
            _fileSystem = fs;
            _session = session;
        }
        public bool CanExecute(string command) => Regex.IsMatch(command, "copy.file ([/A-Za-z1-9]+) ([/A-Za-z1-9]+)");

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

            if (file2 != null)
                throw new Exception($"File {path2.Filename} already exist");
            if(file1 == null)
                throw new Exception($"File {path1.Filename} not exist");

            file2 = new FileModel() { Name = path2.Filename, DateTime = DateTime.Now.ToLong(), Type = 0, Size = file1.Size, Attributes = file1.Attributes, UserId = _session.User.Id, Cluster = await _fileSystem.GetFreeCluster() };

            //Запишем во вторую директорию файл 2
            var startPos = _fileSystem.StartDataAreaByte + _fileSystem.ClusterSize * dir2.Cluster;
            await _fileSystem.WriteData((byte[])file2, startPos, startPos + _fileSystem.ClusterSize );

            var cluster = file1.Cluster;
            var newFileCluster = file2.Cluster;
            var nextCluster = 1;
            //Пока кластер копируемого файла не имеет статус конца
            while(true)
            {
                
                //Считываем статус кластера старого файла
                var statusCluster = await _fileSystem.ReadData(_fileSystem.StartTableByte + cluster * 4 - 4, 4);

                if (statusCluster.ToInt() != 1)
                    nextCluster = await _fileSystem.GetFreeCluster();
                else nextCluster = 1;

                //Вставляем этот статус в новый кластер нового файла
                await _fileSystem.WriteData(nextCluster.ToBytes(), _fileSystem.StartTableByte + newFileCluster * 4 - 4);
                await _fileSystem.WriteData(nextCluster.ToBytes(), _fileSystem.StartTableByte + _fileSystem.TableSize + newFileCluster * 4 - 4);

                //Копируем данные из кластера в новый кластер
                var data = await _fileSystem.ReadData(_fileSystem.StartDataAreaByte + _fileSystem.ClusterSize * cluster, _fileSystem.ClusterSize);
                //Записываем в новый кластер
                await _fileSystem.WriteData(data.ToStr().Clear().ToBytes(), _fileSystem.StartDataAreaByte + _fileSystem.ClusterSize * newFileCluster);

                cluster = statusCluster.ToInt();
                newFileCluster = nextCluster;
                if (cluster == 1)
                    break;
            }

            await _fileSystem.ResizeParentDirectory(dir2, dir2.Size + FileModel.SizeInBytes);
        }

        public void Info()
        {
            Console.WriteLine("copy.file");
        }
    }
}
