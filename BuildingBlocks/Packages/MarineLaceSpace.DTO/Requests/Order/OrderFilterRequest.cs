namespace MarineLaceSpace.DTO.Requests.Order;

public class OrderFilterRequest
{
    public int? Page { get; set; }
    public int? PageSize { get; set; }
    public int? StatusId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? SortBy { get; set; }
    public bool? SortDesc { get; set; }
}
