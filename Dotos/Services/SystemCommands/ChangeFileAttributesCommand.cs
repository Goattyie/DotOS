using Dotos.Models;
using Dotos.Utils;
using System.Text.RegularExpressions;

namespace Dotos.Services.SystemCommands
{
    internal class ChangeFileAttributesCommand : ISystemCommand
    {
        private readonly FileSystem _fileSystem;
        private readonly Session _session;

        public ChangeFileAttributesCommand(FileSystem fs, Session session)
        {
            _fileSystem = fs;
            _session = session;
        }
        public bool CanExecute(string command) => Regex.IsMatch(command, "chmod.file [/A-Za-z1-9]+ (0|1)(0|1)(0|1)(0|1)(0|1)(0|1)");

        public async Task Execute(string command)
        {
            var arr = command.Split(' ');
            var path = arr[1].GetPath();

            var dir = await _fileSystem.GetDir(path.DirectoryPath ?? _session.CurrentDirectory);

            var list = (await _fileSystem.ReadData(_fileSystem.StartDataAreaByte + _fileSystem.ClusterSize * dir.Cluster, _fileSystem.ClusterSize)).AsFileList();
            //Проверка на права доступа к файлу на запись
            for(int i = 0; i < list.Count; i++)
            {
                if (list[i].Name == path.Filename && list[i].Type == 0)
                {
                    //Нашли нужный файл
                    //Проверяем аттрибуты
                    _session.CanWrite(list[i]);

                    //Если доступ разрешен, перезаписываем права
                    list[i].Attributes = arr[2].BitsToShort();
                    await _fileSystem.WriteData((byte[])list[i], _fileSystem.StartDataAreaByte + _fileSystem.ClusterSize * dir.Cluster + i * FileModel.SizeInBytes);
                    break;
                }
            }
        }

        public void Info()
        {
            Console.WriteLine("chmod.file");
        }
    }
}
