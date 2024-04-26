using Carter;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TwitchProject1Model.Models;
using VSAApi.Shared;

namespace VSAApi.Features.Users.Endpoints;

public class GetUser
{
    public class Request : IRequest<Result<Response>>
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
}

//TODO:
//GET: we take the id from the url and then make a new dto object and put the id in it. Then pass it to sender.
//Is this ok?
public class GetUserEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/v1/users/{id}", async ([FromRoute] Guid id, ISender sender) =>
        {
            GetUser.Request request = new()
            {
                Id = id,
            };

            Result<GetUser.Response> result = await sender.Send(request);

            return result.IsFailure ? Results.BadRequest(result.Error) : Results.Ok(result);
        });
    }
}
