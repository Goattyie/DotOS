namespace Dotos.Utils.Exceptions
{
    internal class NonFreeSpaceException : Exception
    {
        public override string Message => "Недостаточно свободного места на диске для этого.";
    }
}
