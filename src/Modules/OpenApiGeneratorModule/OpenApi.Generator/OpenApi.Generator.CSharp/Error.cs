using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenApi.Generator
{

    public class Error
    {
        public Exception Exception { get; }
        public string Message { get; }

        public Error(Exception ex, string message) =>
            (Exception, Message) = (ex, message);
    }
}