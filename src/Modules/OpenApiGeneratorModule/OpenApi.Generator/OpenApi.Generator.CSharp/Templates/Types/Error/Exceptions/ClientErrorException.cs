/*
MIT License

Copyright (c) 2020 Trementa AB

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

namespace Templates.Types.Error.Exceptions
{
    using System;
    using Types;

    public class ClientErrorException : ResponseErrorException
    {
        public override string Description => $"Unspecified client error with code \"{StatusCode}\"";

        public ClientErrorException(StatusCode statusCode, object content, string message, Exception innerException) :
        base(statusCode, content, message, innerException)
        { }

        #region Suppress compiler warning
        ClientErrorException(string message = null, Exception innerException = null) : base(message, innerException)
        { }

        ClientErrorException()
        { }

        ClientErrorException(string message) : base(message)
        { }
        #endregion
    }
}
