using Dotos.Models;
using Dotos.Utils;
using Dotos.Utils.Exceptions;

namespace Dotos.Services
{
    internal class FileSystem
    {
        private readonly string _diskname = "disk";
        private readonly StreamWorker _streamWorker;
        private readonly Session _session;
        private readonly Superblock _superblock;

        public FileSystem(StreamWorker streamWorker, Superblock superblock, Session session)
        {
            _session = session;
            _superblock = superblock;
            //File.Delete(_diskname);
            _streamWorker = streamWorker;
            if (!File.Exists("disk"))
            {
                _streamWorker.Open(_diskname);
                Formatting().GetAwaiter();
            }
            else _streamWorker.Open(_diskname);
        }
        public int StartTableByte => _superblock.SuperblockSize + 1;
        public int TableSize => _superblock.TableSize;
        public int StartUsersByte => StartTableByte + _superblock.TableSize * _superblock.CountTables + 1;
        public int UsersSize => _superblock.UsersSize;
        public int StartRootDirectoryByte => StartUsersByte + _superblock.UsersSize + 1;
        public int RootDirectorySize => _superblock.RootDirectorySize;
        public int StartDataAreaByte => StartRootDirectoryByte + _superblock.RootDirectorySize + 1;
        public int DataAreaSize => _superblock.DataAreaSize;
        public int ClusterSize => _superblock.ClusterSize;
        public async Task Formatting()
        {
            await _streamWorker.WriteAsync(new byte[2459085], 0);

            var i = 0;
            await _streamWorker.WriteAsync(_superblock.Name.ToBytes(), i);
            i += _superblock.Name.Length;
            await _streamWorker.WriteAsync(_superblock.ClusterSize.ToBytes(), i);
            i += 4;
            await _streamWorker.WriteAsync(_superblock.CountTables.ToBytes(), i);
            i += 2;
            await _streamWorker.WriteAsync(_superblock.TableSize.ToBytes(), i);
            i += 4;
            await _streamWorker.WriteAsync(_superblock.UsersSize.ToBytes(), i);
            i += 4;
            await _streamWorker.WriteAsync(_superblock.RootDirectorySize.ToBytes(), i);
            i += 4;
            await _streamWorker.WriteAsync(_superblock.DataAreaSize.ToBytes(), i);

            var user = new User() { Id = 1, Name = "root", Password = "root" };
            await _streamWorker.WriteAsync((byte[])user, StartUsersByte);

            var userDirectory = new FileModel() { Name = "root", Type = 1, Attributes = 0b_111111, DateTime = DateTime.Now.ToLong(), UserId = 1, Cluster = 3, Size = 0 };
            await _streamWorker.WriteAsync(1.ToBytes(), StartTableByte + 8);
            await _streamWorker.WriteAsync(1.ToBytes(), StartTableByte + TableSize + 8);
            await _streamWorker.WriteAsync((byte[])userDirectory, StartRootDirectoryByte);
        }
        /// <summary>
        /// Записывает массив байтов в свободное пространство промежутка
        /// </summary>
        /// <param name="array">массив с записью</param>
        /// <param name="startPosition">начальный байт</param>
        /// <param name="endPosition">конечныый байт</param>
        /// <returns></returns>
        public async Task WriteData(byte[] array, int startPosition, int endPosition)
        {
            for(int i = startPosition; i <= endPosition; i+=array.Length)
            {
                var arr = new byte[array.Length];
                await _streamWorker.ReadAsync(arr, i);
                bool isClear = true;
                for(int j = 0; j < arr.Length; j++)
                {
                    if (arr[j] != '\0')
                    {
                        isClear = false;
                        break;
                    }
                }

                if (isClear)
                {
                    await _streamWorker.WriteAsync(array, i);
                    return;
                }
            }

            throw new NonFreeSpaceException();
        }
        public async Task WriteData(byte[] array, int position)
        {
            await _streamWorker.WriteAsync(array, position);
        }
        public async Task<int> GetFreeCluster()
        {
            for(int i = StartTableByte + 12; i < StartTableByte + _superblock.TableSize; i+=4)
            {
                var data = new byte[4];
                await _streamWorker.ReadAsync(data, i);
                bool isClear = true;
                for (int j = 0; j < data.Length; j++)
                {
                    if (data[j] != 0)
                    {
                        isClear = false;
                        break;
                    }
                }
                if (isClear)
                    return (i - StartTableByte) / 4 + 1;
            }
            throw new Exception("Ошибка. Нет свободных кластеров.");
        }
        public async Task<byte[]> ReadData(int startPosition, int size)
        {
            var bytes = new byte[size];
            await _streamWorker.ReadAsync(bytes, startPosition);
            return bytes;
        }
        public async Task<FileModel> GetDir(string filepath)
        {
            FileModel? item = null;

            var directoryArray = filepath.Split('/');

            //Получаем список файлов из корневого каталога
            var data = await ReadData(StartRootDirectoryByte, StartDataAreaByte - StartRootDirectoryByte);
            var list = data.AsFileList();

            if (filepath == "")
            {
                return new FileModel() { Name = "" };
            }

            for (int i = 1; i < directoryArray.Length; i++)
            {
                item = list.Where(x => x.Name == directoryArray[i] && x.Type == 1).FirstOrDefault();
                if (item == null) throw new NotFileException();
                //проверка на доступ к файлу
                _session.CanRead(item);

                var cluster = item?.Cluster ?? throw new NotFileException();

                data = await ReadData(StartDataAreaByte + cluster * ClusterSize, ClusterSize);
                list = data.AsFileList();
            }
            if (item == null) throw new NotFileException();
            return item;
        }
        public async Task ResizeParentDirectory(FileModel dir, int newSize)
        {
            dir.Size = newSize;

            //Получим размер предыдущего каталога
            var pastDirStr = string.Empty;
            var dirArr = _session.CurrentDirectory.Split('/');
            for (int i = 1; i < dirArr.Length - 1; i++)
                pastDirStr += "/" + dirArr[i];

            var pastDir = await GetDir(pastDirStr);
            int startDataByte;
            int dataSize;
            if (pastDir.Name != Session.RootDirectory)
            {
                startDataByte = StartDataAreaByte + pastDir.Cluster * ClusterSize;
                dataSize = ClusterSize;
            }
            else
            {
                startDataByte = StartRootDirectoryByte;
                dataSize = RootDirectorySize;
            }
            var data = await ReadData(startDataByte, dataSize);
            var list = data.AsFileList();

            int index = -1;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Name == dir.Name && list[i].Type == 1)
                {
                    index = i;
                    break;
                }
            }
            if (index != -1)
                await WriteData(dir.Size.ToBytes(), startDataByte + index * FileModel.SizeInBytes + 25);
        }
    }
}
