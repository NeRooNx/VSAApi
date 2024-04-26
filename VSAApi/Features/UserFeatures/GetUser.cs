using Carter;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TwitchProject1Model.Model;
using TwitchProject1Model.Models;
using VSAApi.Shared;

namespace VSAApi.Features.UserFeatures;

public class GetUser
{
    public class Request : IRequest<Result<Response>>
    {
        public Guid Id;
    }

    public class Response
    {
        public Guid Id;
        public string? Name { get; set; }
        public string? LastName1 { get; set; }
        public string? LastName2 { get; set; }
        public DateTime? BirthDate { get; set; }
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
            User? user = _dbContext.Users.Where(x => x.Id == request.Id).Select(x => x).FirstOrDefault();

            if (user is null)
            {
                return Result.Failure<Response>(new Error(
                    "GetUser.null",
                    "User does not exist"));
            }

            return Result.Success(new Response()
            {
                Id = user.Id,
                BirthDate = user.BirthDate,
                Name = user.Name,
                LastName1 = user.LastName1,
                LastName2 = user.LastName2,
            });

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
