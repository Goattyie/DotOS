using Dotos.Utils;

namespace Dotos.Models
{
    internal class User
    {
        public static int SizeInBytes => 24;

        private string _name;
        private string _password;

        public int Id { get; set; }
        public string Name { get => _name; set { if (value.Length < 10) _name = value; else throw new Exception("Длина никнейма должна быть не больше 10 символов"); } }
        public string Password { get => _password; set { if (value.Length < 10) _password = value; else throw new Exception("Длина пароля должна быть не больше 10 символов"); } }
        public static explicit operator byte[](User user)
        {
            var arr = new byte[24];
            user.Id.ToBytes().CopyTo(arr, 0);
            user.Name.ToBytes().CopyTo(arr, 4);
            user.Password.ToBytes().CopyTo(arr, 14);
            return arr;
        }
        public static int GenerateId()
        {
            return int.Parse(DateTime.Now.Day.ToString() + DateTime.Now.ToShortTimeString().Replace(":", "") + DateTime.Now.Second);
        }

        public override string ToString()
        {
            return $"{Id}\t{Name}\t";
        }
    }
}
