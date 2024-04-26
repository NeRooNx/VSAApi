using Immediate.Apis.Shared;
using Immediate.Handlers.Shared;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using TwitchProject1Model.Models;
using VSAApi.Shared;

namespace VSAApi.Features.Users.Endpoints;

[Handler]
[MapGet("api/v1/users")]
public static partial class GetUsers
{
    internal static Results<Ok<Response>, BadRequest<Error>> TransformResult(Result<Response> result)
    {
        return result.IsFailure
            ? TypedResults.BadRequest(result.Error)
            : TypedResults.Ok(result.Value);
    }

    public class Request;

    public class Response
    {
        public required IReadOnlyList<User> Users { get; init; }
    }

    public class User
    {
        public required Guid Id { get; init; }
        public required string? Name { get; init; }
        public required string? LastName1 { get; init; }
        public required string? LastName2 { get; init; }
        public required DateTime? BirthDate { get; init; }
    }

    private static async ValueTask<Result<Response>> Handle(
        Request request,
        VSAApiDBContext dbContext,
        CancellationToken cancellationToken
    )
    {
        var users = await dbContext.Users
            .Select(u => new User()
            {
                Id = u.Id,
                BirthDate = u.BirthDate,
                Name = u.Name,
                LastName1 = u.LastName1,
                LastName2 = u.LastName2,
            })
            .ToListAsync(cancellationToken);

        Response response = new()
        {
            Users = users
        };

        return Result.Success(response);
    }
}
