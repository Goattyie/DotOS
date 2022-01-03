using Dotos.Models;
using Dotos.Utils;

namespace Dotos.Services
{
    internal class Session
    {
        public User User { get; set; }
        public bool IsRoot { get; set; } = false;
        public string CurrentDirectory { get; set; } = "";
        public const string RootDirectory = "";
        public override string ToString()
        {
            return "[" + User?.Name + "]" + CurrentDirectory + ">";
        }
        public void RequestForAccess()
        {
            if (User?.Name != "root")
            {
                throw new Exception("Ошибка права доступа. Вы не являетесь администратором");
            }

            Console.WriteLine("Введите пароль: ");
            Console.Write(this);
            if (User.Password != Console.ReadLine())
            {
                throw new Exception("Ошибка доступа. Пароль был указан неверно.");
            }
        }
        public void CanWrite(FileModel file)
        {
            var attributes = file.Attributes.AsAttributesArray();
            if (IsRoot)
                return;

            if(User == null)
                throw new Exception("Access denied");

            if (User.Id == file.UserId)
            {
                if(attributes[1] != 1)
                    throw new Exception("Access denied");

                return;
            }
            
            if (attributes[4] != 1)
            {
                throw new Exception("Access denied");
            }
        }
        public void CanRead(FileModel file)
        {
            var attributes = file.Attributes.AsAttributesArray();
            if (IsRoot)
                return;

            if (User == null)
                throw new Exception("Access denied");

            if (User?.Id == file.UserId)
            {
                if (attributes[0] != 1)
                    throw new Exception("Access denied");

                return;
            }

            if (attributes[3] != 1)
            {
                throw new Exception("Access denied");
            }
        }
    }
}
