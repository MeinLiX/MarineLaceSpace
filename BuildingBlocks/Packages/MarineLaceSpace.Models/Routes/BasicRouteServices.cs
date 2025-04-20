using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarineLaceSpace.Models.Routes;

public record BasicRouteServices
{
    public required CancellationToken RequestAborted { get; init; }
}
