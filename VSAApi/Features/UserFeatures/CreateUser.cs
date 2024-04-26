using Carter;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TwitchProject1Model.Models;
using VSAApi.Shared;

namespace VSAApi.Features.UserFeatures;

public class CreateUser
{
    public class Request : IRequest<Result<Response>>
    {
        public string? Name { get; set; }
        public string? LastName1 { get; set; }
        public string? LastName2 { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? Password { get; set; }
    }

    public class Response
    {
        public Guid Id { get; set; }
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(c => c.Name).NotEmpty();
            RuleFor(c => c.Password).NotEmpty();
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
