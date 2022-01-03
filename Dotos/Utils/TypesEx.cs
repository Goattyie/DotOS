using Dotos.Models;
using Dotos.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dotos.Utils
{
    internal static class TypesEx
    {
        public static byte[] ToBytes(this string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }

        public static byte[] ToBytes(this int value)
        {
            return BitConverter.GetBytes(value);
        }

        public static byte[] ToBytes(this short value)
        {
            return BitConverter.GetBytes(value);
        }

        public static byte[] ToBytes(this long value)
        {
            return BitConverter.GetBytes(value);
        }

        public static string ToStr(this byte[] arr)
        {
            return Encoding.UTF8.GetString(arr);
        }

        public static string Clear(this string str)
        {
            return str.Replace("\0", "");
        }

        public static int ToInt(this byte[] arr)
        {
            return BitConverter.ToInt32(arr);
        }

        public static short ToShort(this byte[] arr)
        {
            return BitConverter.ToInt16(arr);
        }

        public static long ToLong(this byte[] arr)
        {
            return BitConverter.ToInt64(arr);
        }

        public static long ToLong(this DateTime dateTime)
        {
            var date = dateTime.ToShortDateString().Replace(".", "");
            if (date.Length == 7)
                date = date.Insert(0, "0");
            var time = dateTime.ToShortTimeString().Replace(".", "").Replace(":", "");
            if (time.Length == 3)
                time = time.Insert(0, "0");

            return long.Parse(date + time);
        }

        public static int GetLastCharIndex(this string str, char c)
        {
            var lastIndex = -1;
            for(int i = 0; i < str.Length; i++)
            {
                if (str[i] == c)
                    lastIndex = i;
            }
            return lastIndex;
        }
        public static Models.Path GetPath(this string str)
        {
            var index1 = str.GetLastCharIndex('/');
            var directoryStr = index1 != -1 ? str.Substring(0, index1) : null;
            var Filename = (index1 != -1 ? str.Remove(0, index1) : str).Trim('/');

            return new Models.Path { DirectoryPath = directoryStr, Filename = Filename };
        }
        public static short[] AsAttributesArray(this short val)
        {
            var arr = new short[6];
            for (int i = 5; i > -1; i--)
                arr[5 - i] = (short)(val >> i & 1);
            return arr;
        }
        public static short BitsToShort(this string str)
        {
            return Convert.ToInt16(str, 2);
        }
        public static List<FileModel> AsFileList(this byte[] data)
        {
            var list = new List<FileModel>();
            for (int i = 0; i < data.Length; i += FileModel.SizeInBytes)
            {
                if (data.Length - i < FileModel.SizeInBytes)
                    return list;

                var name = new byte[10];
                var type = new byte[1];
                var attr = new byte[2];
                var dateTime = new byte[8];
                var cluster = new byte[4];
                var size = new byte[4];
                var userId = new byte[4];

                Array.Copy(data, i, name, 0, 10);
                Array.Copy(data, i + 10, type, 0, 1);
                Array.Copy(data, i + 11, attr, 0, 2);
                Array.Copy(data, i + 13, dateTime, 0, 8);
                Array.Copy(data, i + 21, cluster, 0, 4);
                Array.Copy(data, i + 25, size, 0, 4);
                Array.Copy(data, i + 29, userId, 0, 4);

                var file = new FileModel { Name = name.ToStr().Clear(), Type = type[0], Attributes = attr.ToShort(), DateTime = dateTime.ToLong(), Cluster = cluster.ToInt(), Size = size.ToInt(), UserId = userId.ToInt() };
                if(file.Name.Length != 0)
                    list.Add(file);
            }
            return list;

        }
        public static List<User> AsUserList(this byte[] data)
        {
            var list = new List<User>();
            for (int i = 0; i < data.Length; i += User.SizeInBytes)
            {
                var id = new byte[4];
                var name = new byte[10];
                var password = new byte[10];
                Array.Copy(data, i, id, 0, 4);
                Array.Copy(data, i + 4, name, 0, 10);
                Array.Copy(data, i + 14, password, 0, 10);

                var user = new User { Id = id.ToInt(), Name = name.ToStr().Clear(), Password = password.ToStr().Clear() };
                if (user.Id != 0)
                    list.Add(user);
            }
            return list;
        }
    }
}
