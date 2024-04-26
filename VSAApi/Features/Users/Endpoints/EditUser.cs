using Carter;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TwitchProject1Model.Models;
using VSAApi.Shared;

namespace VSAApi.Features.Users.Endpoints;

public class EditUser
{
    public class Request : IRequest<Result<Response>>
    {
        public required Guid Id { get; init; }
        public required User User { get; init; }
    }

    public class User
    {
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

    internal sealed class Handler(VSAApiDBContext dbContext, IValidator<Request> validator)
        : IRequestHandler<Request, Result<Response>>
    {
        public async Task<Result<Response>> Handle(
            Request request, 
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

            user.BirthDate = request.User.BirthDate;
            user.LastName1 = request.User.LastName1;
            user.LastName2 = request.User.LastName2;
            user.Name = request.User.Name;
            user.Password = request.User.Password;

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
//PUT: we take the id from url and the data from body, make a request item and pass it to sender.
//Is this ok?
public class EditUserEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("api/v1/users/{id}", async ([FromRoute] Guid id, [FromBody] EditUser.User user, ISender sender) =>
        {
            EditUser.Request request = new()
            {
                Id = id,
                User = user,
            };

            Result<EditUser.Response> result = await sender.Send(request);

            return result.IsFailure ? Results.BadRequest(result.Error) : Results.Ok(result);
        });
    }
}
