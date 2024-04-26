using FluentValidation;
using Immediate.Apis.Shared;
using Immediate.Handlers.Shared;
using Microsoft.AspNetCore.Http.HttpResults;
using TwitchProject1Model.Models;
using VSAApi.Shared;

namespace VSAApi.Features.Users.Endpoints;

[Handler]
[MapPost("api/v1/users")]
public static partial class CreateUser
{
    internal static Results<Ok<Response>, BadRequest<Error>> TransformResult(Result<Response> result)
    {
        return result.IsFailure
            ? TypedResults.BadRequest(result.Error)
            : TypedResults.Ok(result.Value);
    }

    public class Request
    {
        public string? Name { get; init; }
        public string? LastName1 { get; init; }
        public string? LastName2 { get; init; }
        public DateTime? BirthDate { get; init; }
        public string? Password { get; init; }
    }

    public class Response
    {
        public Guid Id { get; init; }
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(c => c.Name).NotEmpty();
            RuleFor(c => c.Password).NotEmpty();
        }
    }

    private static async ValueTask<Result<Response>> Handle(
        Request request,
        VSAApiDBContext dbContext,
        IValidator<Request> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = validator.Validate(request);

        if (!validationResult.IsValid)
        {
            return Result.Failure<Response>(new Error(
                "CreateArticle.Validation",
                validationResult.ToString()));
        }

        TwitchProject1Model.Model.User user = new()
        {
            Id = Guid.NewGuid(),
            BirthDate = request.BirthDate,
            LastName1 = request.LastName1,
            LastName2 = request.LastName2,
            Name = request.Name,
            Password = request.Password,
        };

        dbContext.Add(user);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new Response()
        {
            Id = user.Id,
        };
    }
}
