using Dotos.Models;
using Dotos.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Dotos.Services.SystemCommands
{
    internal class RemoveDirCommand : ISystemCommand
    {
        private readonly FileSystem _fileSystem;
        private readonly Session _session;


        public RemoveDirCommand(FileSystem fs, Session session)
        {
            _fileSystem = fs;
            _session = session;
        }

        public bool CanExecute(string command) => Regex.IsMatch(command, "remove.dir ([/A-Za-zА-Яа-я1-9])+");

        public async Task Execute(string command)
        {
            //TODO : Добавить рекурсивное удаление из папки
            var pathStr = command.Split(' ')[1];
            var path = pathStr.GetPath();
            //var path
            //Считываем файл из диска
            var directory = await _fileSystem.GetDir(path.DirectoryPath ?? _session.CurrentDirectory);
            _session.CanWrite(directory);
            //Считываем файл из диска
            var data = await _fileSystem.ReadData(_fileSystem.StartDataAreaByte + _fileSystem.ClusterSize * directory.Cluster, _fileSystem.ClusterSize);
            var list = data.AsFileList();
            var rmDir = list.Find(x => x.Name == path.Filename && x.Type == 1) ?? throw new Exception($"Directory {command.Split(' ')[1]} not found");

            await RemoveDir(rmDir, directory);
            list.Remove(rmDir);
            var directoryStartPosition = _fileSystem.StartDataAreaByte + directory.Cluster * _fileSystem.ClusterSize;
            await _fileSystem.WriteData(new byte[_fileSystem.ClusterSize], directoryStartPosition);
            foreach (var x in list)
            {
                await _fileSystem.WriteData((byte[])x, directoryStartPosition, directoryStartPosition + _fileSystem.ClusterSize);
            }
        }

        public void Info()
        {
            Console.WriteLine("remove.dir");
        }

        private async Task RemoveDir(FileModel dir, FileModel parentDir)
        {
            var data = await _fileSystem.ReadData(_fileSystem.StartDataAreaByte + _fileSystem.ClusterSize * dir.Cluster, _fileSystem.ClusterSize);
            var list = data.AsFileList();
            //Удаляем файлы в каталоге, и вызываем рекурсию если тип файла - каталог
            for(int i = 0; i < list.Count; i++)
            {
                if (list[i].Type == 1)
                {
                    await RemoveDir(list[i], dir);
                    list.RemoveAt(i);
                    i--;
                }
                else
                {
                    //Очищаем данные файла
                    await RemoveFile(list[i], dir);
                    list.RemoveAt(i);
                    i--;
                }
            }

            //Очищаем статус в фат таблице
            await _fileSystem.WriteData(new byte[4], _fileSystem.StartTableByte + dir.Cluster * 4 - 4);
            await _fileSystem.WriteData(new byte[4], _fileSystem.StartTableByte + _fileSystem.TableSize + dir.Cluster * 4 - 4);

            //Очищаем данные в кластере
            await _fileSystem.WriteData(new byte[_fileSystem.ClusterSize], _fileSystem.StartDataAreaByte + dir.Cluster * _fileSystem.ClusterSize);
            await _fileSystem.ResizeParentDirectory(parentDir, parentDir.Size - 33);

        }

        private async Task RemoveFile(FileModel file, FileModel dir)
        {
            var cluster = file.Cluster;

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
            
                await _fileSystem.ResizeParentDirectory(dir, dir.Size - 33);
            }
        }
    }
}
