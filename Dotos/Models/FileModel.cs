using Dotos.Utils;

namespace Dotos.Models
{
    internal class FileModel
    {
        public static int SizeInBytes => 33;
        public string Name { get; set; }
        public byte Type { get; set; }
        public short Attributes { get; set; }
        public long DateTime { get; set; }
        public int Cluster { get; set; }
        public int Size { get; set; }
        public int UserId { get; set; }
        public static explicit operator byte[](FileModel model)
        {
            var arr = new byte[SizeInBytes];
            model.Name.ToBytes().CopyTo(arr, 0);
            new byte[1] { model.Type }.CopyTo(arr, 10);
            model.Attributes.ToBytes().CopyTo(arr, 11);
            model.DateTime.ToBytes().CopyTo(arr, 13);
            model.Cluster.ToBytes().CopyTo(arr, 21);
            model.Size.ToBytes().CopyTo(arr, 25);
            model.UserId.ToBytes().CopyTo(arr, 29);
            return arr;
        }
        public string ToString(List<User> userList)
        {
            var str = string.Empty;
            str += Name + "\t";
            str += Type == 1 ? "Directory\t" : "File\t";

            str += (userList.FirstOrDefault(x => x.Id == UserId)?.Name ?? "root") + "\t";
            for (int i = 5; i > -1; i--)
                str += Attributes >> i & 1;
            str += "\t";

            var date = DateTime.ToString();
            var dateArr = date.Insert(date.Length - 4, ":").Split(":");
            dateArr[0] = dateArr[0].Insert(dateArr[0].Length - 4, ".");
            dateArr[0] = dateArr[0].Insert(dateArr[0].Length - 7, ".");
            dateArr[1] = dateArr[1].Insert(2, ":");
            str += dateArr[0] + "\t";
            str += dateArr[1] + "\t";
            str += Size;

            return str;
        }
    }

}
