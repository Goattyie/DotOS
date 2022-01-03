namespace Dotos.Models
{
    internal class Superblock
    {
        public string Name { get; private set; } = "DotOS";
        public int ClusterSize { get; private set; } = 16384;
        public int SuperblockSize { get; private set; } = 27;
        public short CountTables { get; private set; } = 2;
        public int TableSize { get; private set; } = 600;
        public int UsersSize { get; private set; } = 120;
        public int RootDirectorySize { get; private set; } = 165;
        public int DataAreaSize { get; private set; } = 2457600;

    }
}
