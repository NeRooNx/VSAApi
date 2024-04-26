using Carter;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TwitchProject1Model.Model;
using TwitchProject1Model.Models;
using VSAApi.Shared;

namespace VSAApi.Features.UserFeatures;

public class EditUser
{
    public class Request : IRequest<Result<Response>>
    {
        public Guid id;
        public Dto user = new();

        public class Dto
        {
            public string? Name { get; set; }
            public string? LastName1 { get; set; }
            public string? LastName2 { get; set; }
            public DateTime? BirthDate { get; set; }
            public string? Password { get; set; }
        }
    }

    public class Response
    {
        public Guid Id { get; set; }
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
            FluentValidation.Results.ValidationResult validationResult = _validator.Validate(request);
            if (!validationResult.IsValid)
            {
                return Result.Failure<Response>(new Error(
                    "EditUser.Validation",
                    validationResult.ToString()));
            }

            User? user =_dbContext.Users.Where(x => x.Id == request.id).FirstOrDefault();

            if(user is null)
            {
                return Result.Failure<Response>(new Error(
                    "EditUser.Validation",
                    "User does not exist"));
            }

            user.BirthDate = request.user.BirthDate;
            user.LastName1 = request.user.LastName1;
            user.LastName2 = request.user.LastName2;
            user.Name = request.user.Name;
            user.Password = request.user.Password;

            _dbContext.Add(user);

            await _dbContext.SaveChangesAsync(cancellationToken);

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
        app.MapPut("api/v1/users/{id}", async ([FromRoute] Guid id, [FromBody] EditUser.Request.Dto user, ISender sender) =>
        {
            EditUser.Request request = new()
            {
                id = id,
                user = user,
            };

            Result<EditUser.Response> result = await sender.Send(request);

            return result.IsFailure ? Results.BadRequest(result.Error) : Results.Ok(result);
        });
    }
}
