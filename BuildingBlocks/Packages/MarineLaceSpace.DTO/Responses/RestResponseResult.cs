using System.Text.Json.Serialization;

namespace MarineLaceSpace.DTO.Responses;

public class RestResponseResult : IRestResponseResult
{
    [JsonPropertyName("is_success")]
    public bool IsSuccess { get; set; }

    [JsonPropertyName("date")]
    public DateTime Date { get; set; } = DateTime.Now;

    [JsonPropertyName("status_code")]
    public int StatusCode { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    public static IRestResponseResult Success(int statusCode = 200)
        => new RestResponseResult() { IsSuccess = true, StatusCode = statusCode };

    public static IRestResponseResult Success(string message, int statusCode = 200)
        => new RestResponseResult() { Message = message, IsSuccess = true, StatusCode = statusCode };

    public static IErrorRestResponseResult<object> Error(string message, string errorId, int statusCode = 400)
        => new ErrorRestResponseResult<object>() { Message = message, ErrorId = errorId, IsSuccess = false, StatusCode = statusCode };

    public static IRestResponseResult Fail(int statusCode = 400)
        => new RestResponseResult() { IsSuccess = false, StatusCode = statusCode };

    public static IRestResponseResult Fail(string message, int statusCode = 400)
        => new RestResponseResult() { Message = message, IsSuccess = false, StatusCode = statusCode };
}

public class RestResponseResult<T> : RestResponseResult, IRestResponseResult<T>
       where T : class
{
    [JsonPropertyName("data")]
    public T Data { get; set; }

    public static IRestResponseResult<T> Success(T data, int statusCode = 200)
        => new RestResponseResult<T> { StatusCode = statusCode, IsSuccess = true, Data = data };

    public static IRestResponseResult<T> Success(T data, string message, int statusCode = 200)
        => new RestResponseResult<T> { Data = data, Message = message, IsSuccess = true, StatusCode = statusCode };

    public static new IRestResponseResult<T> Success(string message, int statusCode = 200)
        => new RestResponseResult<T> { Message = message, IsSuccess = true, StatusCode = statusCode };

    public static new IErrorRestResponseResult<T> Error(string message, string errorId, int statusCode = 400)
        => new ErrorRestResponseResult<T>() { Message = message, ErrorId = errorId, IsSuccess = false, StatusCode = statusCode };

    public static new IRestResponseResult<T> Fail(int statusCode = 400)
        => new RestResponseResult<T>() { IsSuccess = false, StatusCode = statusCode };

    public static new IRestResponseResult<T> Fail(string message, int statusCode = 400)
        => new RestResponseResult<T>() { Message = message, IsSuccess = false, StatusCode = statusCode };
}


public class ErrorRestResponseResult<T> : RestResponseResult<T>, IErrorRestResponseResult<T>
    where T : class
{
    [JsonPropertyName("error_id")]
    public string ErrorId { get; set; }
}

