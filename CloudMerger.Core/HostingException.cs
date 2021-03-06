﻿using System;

namespace CloudMerger.Core
{
    public class HostingException : Exception
    {
        public HostingException(string message, Exception innerException=null)
            : base(message, innerException)
        { }
    }

    public class OutOfSpaceLimit : HostingException
    {
        public OutOfSpaceLimit(string message = "", Exception innerException = null) : base(message, innerException)
        {
        }
    }

    public class UnexpectedItemType : HostingException
    {
        public UnexpectedItemType()
            : this("")
        { }

        public UnexpectedItemType(string message, Exception innerException = null) : base(message, innerException)
        { }
    }

    public class ItemNotFound : HostingException
    {
        public ItemNotFound()
            : this("")
        { }

        public ItemNotFound(string message, Exception innerException = null) : base(message, innerException)
        { }
        public ItemNotFound(Exception innerException) : base("", innerException)
        { }
    }

    public class HostUnavailable : HostingException
    {
        public HostUnavailable(string message, Exception innerException = null) : base(message, innerException)
        { }

        public HostUnavailable(Exception innerException) : this("", innerException)
        { }
    }

    public class InconsistentFileSystemState : HostingException
    {
        public InconsistentFileSystemState(string message = "", Exception innerException = null) : base(message, innerException)
        { }

        public InconsistentFileSystemState(Exception innerException) : this("", innerException)
        { }
    }
}