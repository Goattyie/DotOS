using Dotos.Models;
using Dotos.Utils;
using System.Text.RegularExpressions;

namespace Dotos.Services.SystemCommands
{
    internal class RemoveUserCommand : ISystemCommand
    {
        private readonly Session _session;
        private readonly FileSystem _fileSystem;

        public RemoveUserCommand(FileSystem fs, Session session)
        {
            _session = session;
            _fileSystem = fs;
        }
        public bool CanExecute(string command) => Regex.IsMatch(command, "remove.user [A-Za-z]+");

        public async Task Execute(string command)
        {
            var arr = command.Split(' ');
            var user = arr[1];

            if (user == "root")
                throw new Exception("Access denied.");

            _session.RequestForAccess();

            var data = await _fileSystem.ReadData(_fileSystem.StartUsersByte, _fileSystem.StartRootDirectoryByte - _fileSystem.StartUsersByte);
            for (int i = 0; i < data.Length; i += User.SizeInBytes)
            {
                var name = new byte[10];
                Array.Copy(data, i + 4, name, 0, 10);

                //Если пользователь существует
                if(name.ToStr().Clear() == user)
                {

                    //Переносим домашний каталог к root и удаляем старый
                    var list = (await _fileSystem.ReadData(_fileSystem.StartRootDirectoryByte, _fileSystem.RootDirectorySize)).AsFileList();
                    var userObj = list.First(x => x.Name.Clear() == user);
                    //Создаем каталог с кластеров в root
                    var newName = userObj.Name.Clear();
                    if(newName.Length > 6)
                        newName.Remove(5);
                    newName += DateTime.Now.Date.ToString().Remove(5).Replace(".", "");
                    var newDir = new FileModel { Name = newName, Attributes = 0b_111000, Cluster = userObj.Cluster, DateTime = DateTime.Now.ToLong(), Size = userObj.Size, Type = 1, UserId = 1 };
                    await _fileSystem.WriteData((byte[])newDir, _fileSystem.StartDataAreaByte + _fileSystem.ClusterSize * 3, _fileSystem.StartDataAreaByte + _fileSystem.ClusterSize * 3 + _fileSystem.ClusterSize);

                    //Удаляем его в области с пользователями
                    await _fileSystem.WriteData(new byte[User.SizeInBytes], _fileSystem.StartUsersByte + i);
                    await _fileSystem.WriteData(new byte[FileModel.SizeInBytes], _fileSystem.StartRootDirectoryByte + FileModel.SizeInBytes * i / User.SizeInBytes);

                    var dir = await _fileSystem.GetDir("/root");
                    await _fileSystem.ResizeParentDirectory(dir, dir.Size + 33);
                    Console.WriteLine("User was removed.");
                    return;
                }
            }
            throw new Exception("User not found.");

        }
        public void Info()
        {
            Console.WriteLine("remove.user *username*\t delete users");
        }
    }
}
