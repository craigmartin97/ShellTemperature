using System;

namespace BluetoothService
{
    public class SleepException : Exception
    {
        public SleepException()
        {
        }

        public SleepException(string message) : base(message)
        {
        }

        public SleepException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
