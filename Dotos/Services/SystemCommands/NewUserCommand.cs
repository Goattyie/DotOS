using Dotos.Models;
using Dotos.Utils;
using System.Text.RegularExpressions;

namespace Dotos.Services.SystemCommands
{
    internal class NewUserCommand : ISystemCommand
    {
        private readonly Session _session;
        private readonly FileSystem _fileSystem;

        public NewUserCommand(FileSystem fs, Session session)
        {
            _session = session;
            _fileSystem = fs;
        }
        public bool CanExecute(string command) => Regex.IsMatch(command, "new.user [A-Za-z]* [A-Za-z1-9]*");

        public async Task Execute(string command)
        {
            var arr = command.Split(' ');

            _session.RequestForAccess();

            //Проверим, если ли уже пользователь с таким ников
            var data = await _fileSystem.ReadData(_fileSystem.StartUsersByte, _fileSystem.UsersSize);
            for(int i = 0; i < data.Length; i += User.SizeInBytes)
            {
                var byteName = new byte[10];
                for (int j = 4; j < 14; j++)
                    byteName[j - 4] = data[j];

                if (byteName.ToStr().Trim('\0') == arr[1])
                    throw new Exception("Account with this name already exist.");
            }
            var usersData = await _fileSystem.ReadData(_fileSystem.StartUsersByte, _fileSystem.UsersSize);
            var userList = usersData.AsUserList();
            var user = new User() { Id = User.GenerateId(), Name = arr[1], Password = arr[2] };

            if (userList.FirstOrDefault(x => x.Name == user.Name) != null)
                throw new Exception("User already exist.");

            var cluster = await _fileSystem.GetFreeCluster();
            var userDirectory = new FileModel() { Name = user.Name, Type = 1, Attributes = 0b_111111, DateTime = DateTime.Now.ToLong(), UserId = user.Id, Cluster = cluster, Size = 0 };

            await _fileSystem.WriteData(1.ToBytes(), _fileSystem.StartTableByte + cluster * 4 - 4);
            await _fileSystem.WriteData(1.ToBytes(), _fileSystem.StartTableByte + _fileSystem.TableSize + cluster * 4 - 4);

            await _fileSystem.WriteData((byte[])user, _fileSystem.StartUsersByte, _fileSystem.StartRootDirectoryByte);
            await _fileSystem.WriteData((byte[])userDirectory, _fileSystem.StartRootDirectoryByte, _fileSystem.StartDataAreaByte);
            Console.WriteLine("User was created.");

        }
        public void Info()
        {
            Console.WriteLine("new.user *username* *password*\t create new user");
        }
    }

}
