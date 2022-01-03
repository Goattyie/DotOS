using System;
using System.IO;
using System.Threading.Tasks;

namespace Dotos.Utils
{
    internal class StreamWorker : IDisposable
    {
        private FileStream _fs;

        public void Dispose()
        {
            _fs?.Close();
            _fs?.Dispose();
        }

        public void Open(string filename)
        {
            _fs = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        }

        public async Task WriteAsync(byte[] buffer, long position)
        {
            _fs.Position = position;
            await _fs.WriteAsync(buffer, 0, buffer.Length);
            await _fs.FlushAsync();
        }

        public async Task ReadAsync(byte[] buffer, long position)
        {
            _fs.Position = position;
            await _fs.ReadAsync(buffer,0, buffer.Length);
        }
    }
}
