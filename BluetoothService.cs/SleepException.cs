using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

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
