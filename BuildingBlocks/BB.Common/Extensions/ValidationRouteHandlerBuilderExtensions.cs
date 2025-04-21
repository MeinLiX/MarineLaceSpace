using MarineLaceSpace.DTO.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace BB.Common.Extensions;

public static class ValidationRouteHandlerBuilderExtensions
{
    //WARN! openapi display field 'data' like 'null'
    public static RouteHandlerBuilder AddValidationResponseType(this RouteHandlerBuilder builder) => builder.Produces<IRESTResult<IDictionary<string, string[]>>>(statusCode: 422);
}
