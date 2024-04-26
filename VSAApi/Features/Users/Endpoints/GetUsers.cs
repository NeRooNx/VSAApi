using Carter;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TwitchProject1Model.Models;
using VSAApi.Shared;

namespace VSAApi.Features.Users.Endpoints;

public class GetUsers
{
    public class Request : IRequest<Result<Response>>
    {
    }

    //TODO:
    //My response is a list of users, so i need to declare a sub dto to be able to fill the list with dto objects instead of entities.
    //Is this ok?
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

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
        }
    }

    internal sealed class Handler(VSAApiDBContext dbContext)
        : IRequestHandler<Request, Result<Response>>
    {
        public async Task<Result<Response>> Handle(
            Request request,
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
}

//TODO:
//GET list: we don't take an input, but we create a new request dto to be able to pass it to sender.
//Is this ok?
public class GetUsersEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/v1/users", async (ISender sender) =>
        {
            Result<GetUsers.Response> result = await sender.Send(new GetUsers.Request());

            return result.IsFailure ? Results.BadRequest(result.Error) : Results.Ok(result);
        });
    }
}
