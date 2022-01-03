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
    internal class SignInCommand : ISystemCommand
    {
        private readonly FileSystem _fileSystem;
        private readonly Session _session;

        public bool CanExecute(string command) => Regex.IsMatch(command, "sign.in [A-Za-z1-9]+ [A-Za-z1-9]+");

        public SignInCommand(FileSystem fs, Session session)
        {
            _fileSystem = fs;
            _session = session;
        }
        public async Task Execute(string command)
        {
            if (_session.User != null)
                throw new Exception("Authorization error. You yet in session.");

            var arr = command.Split(' ');

            var data = await _fileSystem.ReadData(_fileSystem.StartUsersByte, _fileSystem.UsersSize);
            for (int i = 0; i < data.Length; i += User.SizeInBytes)
            {
                var id = new byte[4];
                var name = new byte[10];
                var password = new byte[10];
                Array.Copy(data, i, id, 0, 4);
                Array.Copy(data, i + 4, name, 0, 10);
                Array.Copy(data, i + 14, password, 0, 10);

                if (name.ToStr().Replace("\0", "") == arr[1] && password.ToStr().Replace("\0", "") == arr[2])
                {
                    _session.User = new User() { Id = id.ToInt(), Name = arr[1], Password = arr[2] };
                    _session.IsRoot = _session.User.Name == "root";
                    _session.CurrentDirectory = "";
                    Console.WriteLine("Authorization succes.");
                    return;
                }
            }
            Console.WriteLine("Denied. Check your data.");
        }

        public void Info()
        {
            Console.WriteLine("sign.in *username* *password*\t~");
        }
    }
}
