namespace MarineLaceSpace.Options;

public class RabbitMQOption
{
    public required string Exchange { get; set; }
    public required string Queue { get; set; }
    public required string RoutingKey { get; set; }
}
