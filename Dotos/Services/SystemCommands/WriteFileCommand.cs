using Dotos.Models;
using Dotos.Utils;
using Dotos.Utils.Exceptions;
using System.Text.RegularExpressions;

namespace Dotos.Services.SystemCommands
{
    internal class WriteFileCommand : ISystemCommand
    {
        private readonly FileSystem _fileSystem;
        private readonly Session _session;
        private FileModel file;


        public WriteFileCommand(FileSystem fs, Session session)
        {
            _fileSystem = fs;
            _session = session;
        }
        public bool CanExecute(string command) => Regex.IsMatch(command, "write.file ([/A-Za-zА-Яа-я1-9])+( -a)?");

        public async Task Execute(string command)
        {
            var arr = command.Split(' ');
            var path = arr[1].GetPath();
            //Получаем файлы текущей директории
            var directory = await _fileSystem.GetDir(path.DirectoryPath ?? _session.CurrentDirectory);
            var data = await _fileSystem.ReadData(_fileSystem.StartDataAreaByte + _fileSystem.ClusterSize * directory.Cluster, _fileSystem.ClusterSize);
            var list = data.AsFileList();
            file = list.Find(x => x.Name == path.Filename && x.Type == 0) ?? throw new Exception($"File {command.Split(' ')[1]} not found");
            _session.CanWrite(file);
            //Считываем данные из консоли
            var inputData = string.Empty;
            Console.WriteLine("Write data:");
            inputData += Console.ReadLine();
            Console.WriteLine("To stop write next data - Press E.");
            while (Console.ReadKey().Key != ConsoleKey.E)
            {
                Console.WriteLine("Write data:");
                inputData += Console.ReadLine();
                Console.WriteLine("To stop write next data - Press E.");
            }
            file.Size = 0;
            if (arr.Length == 2)
                await RewriteData(inputData.ToBytes());
            else await AppendData(inputData.ToBytes());

            var index = list.IndexOf(file);
            await _fileSystem.WriteData(file.Size.ToBytes(), _fileSystem.StartDataAreaByte + _fileSystem.ClusterSize * directory.Cluster + index * FileModel.SizeInBytes + 25);
        }
        private async Task RewriteData(byte[] data)
        {
            var cluster = file.Cluster;
            var oldCluster = cluster;
            var nextCluster = 0;
            //Очищаем кластеры и область данных
            while(true)
            {
                //Очистка в области данных
                await _fileSystem.WriteData(new byte[_fileSystem.ClusterSize], _fileSystem.StartDataAreaByte + _fileSystem.ClusterSize * oldCluster);
                //Получаем следующий кластер
                var newClusterData = await _fileSystem.ReadData(_fileSystem.StartTableByte + oldCluster * 4 - 4, 4);
                nextCluster = newClusterData.ToInt();
                //Очистка значения кластера в таблице
                await _fileSystem.WriteData(new byte[4], _fileSystem.StartTableByte + oldCluster * 4 - 4);
                await _fileSystem.WriteData(new byte[4], _fileSystem.StartTableByte + _fileSystem.TableSize + oldCluster * 4 - 4);
                oldCluster = nextCluster;

                if(oldCluster == 1) break;
            }
            await WriteData(cluster, data);
        }
        private async Task AppendData(byte[] data)
        {
            var cluster = file.Cluster;
            //Находим последний кластер
            while (true)
            {
                var clusterStatusData = await _fileSystem.ReadData(_fileSystem.StartTableByte + cluster * 4 - 4, 4);
                var clusterStatus = clusterStatusData.ToInt();

                if (clusterStatus == 1)
                    break;


                cluster = clusterStatus;
                file.Size += _fileSystem.ClusterSize;
            }
            //cluster - последний кластер записи. Считываем данные, добавляем новые и записываем.
            var oldDataBytes = await _fileSystem.ReadData(_fileSystem.StartDataAreaByte + _fileSystem.ClusterSize * cluster, _fileSystem.ClusterSize);
            var oldData = oldDataBytes.ToStr().Trim('\0').ToBytes();
            var newData = new byte[oldData.Length + data.Length];
            oldData.CopyTo(newData, 0);
            data.CopyTo(newData, oldData.Length);

            await WriteData(cluster, newData);
        }
        private async Task WriteData(int cluster, byte[] data)
        {
            var newCluster = -1;
            //Записываем данные в кластеры
            for (int i = 0; i < data.Length; i += _fileSystem.ClusterSize)
            {
                if (newCluster != -1)
                {
                    //Записываем в значение предыдущего кластера значение следующего.
                    await _fileSystem.WriteData(newCluster.ToBytes(), _fileSystem.StartTableByte + cluster * 4 - 4);
                    await _fileSystem.WriteData(newCluster.ToBytes(), _fileSystem.StartTableByte + _fileSystem.TableSize + cluster * 4 - 4);
                    cluster = newCluster;
                }
                var partData = new byte[_fileSystem.ClusterSize];
                var size = (data.Length - i) >= _fileSystem.ClusterSize ? _fileSystem.ClusterSize : data.Length - i;
                Array.Copy(data, i, partData, 0, size);
                var startPosition = _fileSystem.StartDataAreaByte + _fileSystem.ClusterSize * cluster;
                await _fileSystem.WriteData(partData, startPosition);
                await _fileSystem.WriteData(1.ToBytes(), _fileSystem.StartTableByte + cluster * 4 - 4);
                await _fileSystem.WriteData(1.ToBytes(), _fileSystem.StartTableByte + _fileSystem.TableSize + cluster * 4 - 4);
                file.Size += size;

                //Записываем значение следующего кластера
                newCluster = await _fileSystem.GetFreeCluster();
            }
        }
        public void Info()
        {
            Console.WriteLine("write.file");
        }
    }
}
