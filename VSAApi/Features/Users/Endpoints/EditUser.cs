using FluentValidation;
using Immediate.Apis.Shared;
using Immediate.Handlers.Shared;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using TwitchProject1Model.Models;
using VSAApi.Shared;

namespace VSAApi.Features.Users.Endpoints;

[Handler]
[MapPut("api/v1/users")]
public static partial class EditUser
{
    internal static Results<Ok<Response>, BadRequest<Error>> TransformResult(Result<Response> result)
    {
        return result.IsFailure 
            ? TypedResults.BadRequest(result.Error)
            : TypedResults.Ok(result.Value);
    }

    public class Request
    {
        public required Guid Id { get; init; }
        public required string? Name { get; init; }
        public required string? LastName1 { get; init; }
        public required string? LastName2 { get; init; }
        public required DateTime? BirthDate { get; init; }
        public required string? Password { get; init; }
    }

    public class Response
    {
        public Guid Id { get; init; }
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
        }
    }

    private static async ValueTask<Result<Response>> Handle(
            Request request,
            VSAApiDBContext dbContext,
            IValidator<Request> validator,
            CancellationToken cancellationToken
        )
    {
        var validationResult = validator.Validate(request);

        if (!validationResult.IsValid)
        {
            return Result.Failure<Response>(new Error(
                "EditUser.Validation",
                validationResult.ToString()));
        }

        var user = await dbContext.Users
            .Where(x => x.Id == request.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            return Result.Failure<Response>(new Error(
                "EditUser.Validation",
                "User does not exist"));
        }

        user.BirthDate = request.BirthDate;
        user.LastName1 = request.LastName1;
        user.LastName2 = request.LastName2;
        user.Name = request.Name;
        user.Password = request.Password;

        dbContext.Add(user);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new Response()
        {
            Id = user.Id,
        };
    }
}
