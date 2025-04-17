using System.Text.Json.Serialization;

namespace MarineLaceSpace.DTO.Responses;

public interface IRestResponseResult
{
    [JsonPropertyName("is_success")]
    public bool IsSuccess { get; set; }

    [JsonPropertyName("date")]
    public DateTime Date { get; set; }

    [JsonPropertyName("status_code")]
    public int StatusCode { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }
}

public interface IRestResponseResult<T> : IRestResponseResult
    where T : class
{
    [JsonPropertyName("data")]
    public T? Data { get; set; }
}

public interface IErrorRestResponseResult
{
    [JsonPropertyName("error_id")]
    public string ErrorId { get; set; }
}

public interface IErrorRestResponseResult<T> : IErrorRestResponseResult, IRestResponseResult<T>
    where T : class
{
}
