using Carter;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TwitchProject1Model.Model;
using TwitchProject1Model.Models;
using VSAApi.Shared;

namespace VSAApi.Features.UserFeatures;

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
        public List<Dto> users = new List<Dto>();

        public class Dto
        {
            public Guid Id;
            public string? Name { get; set; }
            public string? LastName1 { get; set; }
            public string? LastName2 { get; set; }
            public DateTime? BirthDate { get; set; }
        }
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
        }
    }

    internal sealed class Handler : IRequestHandler<Request, Result<Response>>
    {
        private readonly VSAApiDBContext _dbContext;
        private readonly IValidator<Request> _validator;

        public Handler(VSAApiDBContext dbContext, IValidator<Request> validator)
        {
            _dbContext = dbContext;
            _validator = validator;
        }

        public async Task<Result<Response>> Handle(Request request, CancellationToken cancellationToken)
        {
            List<User> users = _dbContext.Users.ToList();

            if (users is null)
            {
                return Result.Failure<Response>(new Error(
                    "GetUser.null",
                    "User does not exist"));
            }

            Response response = new();

            users.ForEach(u =>
            {
                var dto = new Response.Dto()
                {
                    Id = u.Id,
                    BirthDate = u.BirthDate,
                    Name = u.Name,
                    LastName1 = u.LastName1,
                    LastName2 = u.LastName2,
                };
                response.users.Add(dto);
            });

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
