using Comgo.Application.SecurityQuestions.Commands;
using Comgo.Application.SecurityQuestions.Queries;
using Comgo.Application.Users.Commands;
using Comgo.Application.Users.Queries;
using Comgo.Core.Model;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Comgo.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SecurityQuestionController : ControllerBase
    {
        private readonly IMediator _mediator;
        public SecurityQuestionController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("create")]
        public async Task<ActionResult<Result>> Create(CreateSecurityQuestionsCommand command)
        {
            try
            {
                return await _mediator.Send(command);
            }
            catch (Exception ex)
            {
                return Result.Failure($"Failed to create security question. Error: {ex?.Message ?? ex?.InnerException?.Message}");
            }
        }

        [HttpGet("getall/{skip}/{take}")]
        public async Task<ActionResult<Result>> GenerateAddress(int skip, int take)
        {
            try
            {
                return await _mediator.Send(new GetAllSecurityQuestionsQuery
                {
                    Skip = skip,
                    Take = take
                });
            }
            catch (Exception ex)
            {
                return Result.Failure($"Security question retrieval was not successful. {ex?.Message ?? ex?.InnerException?.Message}");
            }
        }

        [HttpGet("getbyid/{id}")]
        public async Task<ActionResult<Result>> GetWalletBalance(int id)
        {
            try
            {
                return await _mediator.Send(new GetSecurityQuestionByIdQuery
                {
                    Id = id
                });
            }
            catch (Exception ex)
            {
                return Result.Failure($"Security question retrieval was not successful.{ex?.Message ?? ex?.InnerException?.Message}");
            }
        }
    }
}
