using Comgo.Application.BitcoinMethods.Commands;
using Comgo.Application.Users;
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

        [HttpPost("importdescriptor")]
        public async Task<ActionResult<Result>> ImportDescriptor(ImportDescriptorCommand command)
        {
            try
            {
                accessToken.ValidateToken(command.UserId);
                return await _mediator.Send(command);
            }
            catch (Exception ex)
            {
                return Result.Failure($"Failed to import descriptor. Error: {ex?.Message ?? ex?.InnerException?.Message}");
            }
        }

        [HttpPost("generatedescriptorr")]
        public async Task<ActionResult<Result>> GenerateDescriptorr(GenerateDescriptorCommand command)
        {
            try
            {
                return await _mediator.Send(command);
            }
            catch (Exception ex)
            {
                return Result.Failure($"Failed to generate new address. Error: {ex?.Message ?? ex?.InnerException?.Message}");
            }
        }


        [HttpGet("getwalletdescriptor/{userId}")]
        public async Task<ActionResult<Result>> GetWalletDescriptor(string userId)
        {
            try
            {
                accessToken.ValidateToken(userId);
                return await _mediator.Send(new CreateDescriptorStringCommand
                {
                    UserId = userId
                });
            }
            catch (Exception ex)
            {
                return Result.Failure($"Failed to generate wallet descriptor.{ex?.Message ?? ex?.InnerException?.Message}");
            }
        }
    }
}
