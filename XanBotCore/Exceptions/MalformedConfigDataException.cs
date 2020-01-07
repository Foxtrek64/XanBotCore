﻿using System;
using XanBotCore.DataPersistence;

namespace XanBotCore.Exceptions
{

    /// <summary>
    /// An exception that is thrown when data stored in an <see cref="XConfiguration"/> is in the incorrect form for the code loading the data.
    /// </summary>
    public class MalformedConfigDataException : Exception
    {
        public MalformedConfigDataException() : this("Malformed configuration value.") { }
        public MalformedConfigDataException(string message) : base(message) { }
    }
}
