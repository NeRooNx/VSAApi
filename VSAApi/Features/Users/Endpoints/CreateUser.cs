using Carter;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TwitchProject1Model.Models;
using VSAApi.Shared;

namespace VSAApi.Features.Users.Endpoints;

public class CreateUser
{
    public class Request : IRequest<Result<Response>>
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

    internal sealed class Handler(VSAApiDBContext dbContext, IValidator<Request> validator)
        : IRequestHandler<Request, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Request request, CancellationToken cancellationToken)
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
}

//TODO:
//POST: we take the request dto directly as body json and pass it to sender.
//Is this ok?
public class CreateUserEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/v1/users", async ([FromBody] CreateUser.Request request, ISender sender) =>
        {
            Result<CreateUser.Response> result = await sender.Send(request);

            return result.IsFailure ? Results.BadRequest(result.Error) : Results.Ok(result);
        });
    }
}
