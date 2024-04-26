using FluentValidation;
using Immediate.Apis.Shared;
using Immediate.Handlers.Shared;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using TwitchProject1Model.Models;
using VSAApi.Shared;

namespace VSAApi.Features.Users.Endpoints;

[Handler]
[MapGet("api/v1/users/{id:guid}")]
public static partial class GetUser
{
    internal static Results<Ok<Response>, BadRequest<Error>> TransformResult(Result<Response> result)
    {
        return result.IsFailure
            ? TypedResults.BadRequest(result.Error)
            : TypedResults.Ok(result.Value);
    }

    public class Request
    {
        public Guid Id { get; init; }
    }

    public class Response
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
            Response? user = await dbContext
                .Users
                .Where(x => x.Id == request.Id)
                .Select(u => new Response()
                {
                    Id = u.Id,
                    BirthDate = u.BirthDate,
                    Name = u.Name,
                    LastName1 = u.LastName1,
                    LastName2 = u.LastName2,
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (user is null)
            {
                return Result.Failure<Response>(new Error(
                    "GetUser.null",
                    "User does not exist"));
            }

            return Result.Success(user);

        }
    }
