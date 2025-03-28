using System;

namespace UnionAvatars.API
{
    public class WebResponse
    {
        public ResponseStatus status;
        public string responseErrorMessage;
    }

    public class WebResponse<T> : WebResponse
    {
        public T data;
    }

    public class ErrorResponse
    {
        public string detail;
    }

    public class CheckoutCreate
    {
        public string ClientSecret { get; set;}
        public Uri SessionUrl { get; set;}
        public Guid CartId { get; set;}
    }

    public class CheckoutStatus
    {
        public bool completed { get; set;}
    }

    public class PaidAssets
    {
        public Guid[] Assets { get; set;}
    }

    public enum ResponseStatus
    {
        Success,
        Failed,
        Dropped
    }
}
