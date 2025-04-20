namespace MarineLaceSpace.DTO.Responses;

public interface IRESTResult
{
    string? Message { get; set; }
    bool Succeeded { get; set; }
    DateTime Date { get; set; }
}

public interface IRESTErrorResult<T> : IRESTResult<T>
{
    string? Exception { get; set; }
    string? ErrorId { get; set; }
    int StatusCode { get; set; }
}

public interface IRESTResult<out T> : IRESTResult
{
    T? Data { get; }
}
