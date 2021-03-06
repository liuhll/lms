using System;

namespace Silky.Core.Exceptions
{
    public class CommunicatonException : SilkyException
    {
        public CommunicatonException(string message) : base(message, StatusCode.CommunicatonError)
        {
        }

        public CommunicatonException(string message, Exception innerException) : base(message, innerException,
            StatusCode.CommunicatonError)
        {
        }
    }
}