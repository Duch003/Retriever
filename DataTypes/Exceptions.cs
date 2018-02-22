using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataTypes
{
    public class EmptyNetworkListException : Exception
    {
        public EmptyNetworkListException() : base() { }
        public EmptyNetworkListException(string message) : base(message) { }
        public EmptyNetworkListException(string message, Exception inner) : base (message, inner) { }
    }

    public class DriversNotInstalledException : Exception
    {
        public DriversNotInstalledException() : base() { }
        public DriversNotInstalledException(string message) : base(message) { }
        public DriversNotInstalledException(string message, Exception inner) : base(message, inner) { }
    }
}
