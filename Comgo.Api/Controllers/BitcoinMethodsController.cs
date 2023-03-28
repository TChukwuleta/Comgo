using Comgo.Application.BitcoinMethods.Commands;
using Comgo.Core.Model;
using Comgo.Infrastructure.Utility;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Comgo.Api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class BitcoinMethodsController : ApiController
    {
        private readonly IMediator _mediator;
        protected readonly IHttpContextAccessor _contextAccessor;
        public BitcoinMethodsController(IMediator mediator, IHttpContextAccessor contextAccessor)
        {
            _mediator = mediator;
            _contextAccessor = contextAccessor;
            accessToken = _contextAccessor.HttpContext.Request.Headers["Authorization"].ToString()?.ExtractToken();
            if (accessToken == null)
            {
                throw new Exception("You are not authorized!");
            }
        }

        [HttpPost("generateaddress")]
        public async Task<ActionResult<Result>> CreateTransaction(GenerateMultisigAddressCommand command)
        {
            try
            {
                accessToken.ValidateToken(command.UserId);
                return await _mediator.Send(command);
            }
            catch (Exception ex)
            {
                return Result.Failure($"Failed to generate new address. Error: {ex?.Message ?? ex?.InnerException?.Message}");
            }
        }
    }
}
