// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Runtime.Serialization;

namespace MicrosoftGraphSDK
{
    /// <summary>
    /// Derived exception class to handle graph calls errors
    /// </summary>
    [Serializable]
    public sealed class GraphException : Exception
    {
        public object[]? Params { get; }
        public Exception innerException { get; }

        public GraphException() : base() { }

        public GraphException(string message) : base(message) { }

        public GraphException(string message, Exception inner) : base(message, inner)
        {
            innerException = inner;
        }

        public GraphException(string message, params object[] CallParams) : base(message)
        {
            Params = CallParams;
        }

        public GraphException(string message, Exception inner, params object[] CallParams) : base(message, inner)
        {
            innerException = inner;
            Params = CallParams;
        }

        protected GraphException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public override string ToString()
        {
            string parameters = Params == null ? string.Empty : $", Params: {string.Join(", ", Params)}";
            return $"{Message} {parameters} Exception: {innerException}";
        }
    }
}
