namespace MarineLaceSpace.DTO.Responses;

public class RESTResult : IRESTResult
{
    public string? Message { get; set; }

    public bool Succeeded { get; set; }

    public DateTime Date { get; set; } = DateTime.Now;

    public static IRESTResult Fail(string? message = null) => new RESTResult { Succeeded = false, Message = message };

    public static IRESTResult Success(string? message = null) => new RESTResult { Succeeded = true, Message = message };
}

public class RESTErrorResult<T> : RESTResult<T>, IRESTErrorResult<T>
{
    public string? Exception { get; set; }

    public string? ErrorId { get; set; }

    public int StatusCode { get; set; }
}

public class RESTResult<T> : RESTResult, IRESTResult<T>
{
    public T? Data { get; set; }

    public static new RESTResult<T> Fail(string? message = null) => new() { Succeeded = false, Message = message };

    public static RESTResult<T> Fail(T data, string? message = null) => new() { Succeeded = false, Data = data, Message = message };

    public static RESTErrorResult<T> ReturnError() => new() { Succeeded = false, StatusCode = 500 };

    public static RESTErrorResult<T> ReturnError(string message) => new() { Succeeded = false, Message = message, StatusCode = 500 };

    public static new RESTResult<T> Success(string? message = null) => new() { Succeeded = true, Message = message };

    public static RESTResult<T> Success(T data, string? message = null) => new() { Succeeded = true, Data = data, Message = message };
}