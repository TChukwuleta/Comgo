using Comgo.Application.Lightnings.Commands;
using Comgo.Application.Paystacks.Commands;
using Comgo.Application.Users;
using Comgo.Application.Users.Commands;
using Comgo.Core.Model;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Comgo.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("createdescriptor")]
        public async Task<ActionResult<Result>> CreateDescriptor(CreateDescriptorStringCommand command)
        {
            try
            {
                return await _mediator.Send(command);
            }
            catch (Exception ex)
            {
                return Result.Failure($"Failed to create descriptor. Error: {ex?.Message ?? ex?.InnerException?.Message}");
            }
        }

        [HttpPost("createuser")]
        public async Task<ActionResult<Result>> CreateUser(CreateUserCommand command)
        {
            try
            {
                return await _mediator.Send(command);
            }
            catch (Exception ex)
            {
                return Result.Failure($"User creation was not successful. Error: {ex?.Message ?? ex?.InnerException?.Message}");
            }
        }

        [HttpPost("emailverification")]
        public async Task<ActionResult<Result>> EmailVerification(EmailVerificationCommand command)
        {
            try
            {
                return await _mediator.Send(command);
            }
            catch (Exception ex)
            {
                return Result.Failure($"User verification was not successful. Error: {ex?.Message ?? ex?.InnerException?.Message}");
            }
        }

        [HttpPost("servicepayment")]
        public async Task<ActionResult<Result>> ServicePayment(PaymentServiceCommand command)
        {
            try
            {
                return await _mediator.Send(command);
            }
            catch (Exception ex)
            {
                return Result.Failure($"Service payment was not successful. Error: {ex?.Message ?? ex?.InnerException?.Message}");
            }
        }

        [HttpPost("verifypaystackpayment")]
        public async Task<ActionResult<Result>> VerifyPaystackPayment(VerifyPaystackCommand command)
        {
            try
            {
                return await _mediator.Send(command);
            }
            catch (Exception ex)
            {
                return Result.Failure($"Payment verification was not successful. Error: {ex?.Message ?? ex?.InnerException?.Message}");
            }
        }

        [HttpPost("listenforpayment")]
        public async Task<ActionResult<Result>> ListenForPayment(ListenForInvoiceCommand command)
        {
            try
            {
                return await _mediator.Send(command);
            }
            catch (Exception ex)
            {
                return Result.Failure($"Payment verification was not successful. Error: {ex?.Message ?? ex?.InnerException?.Message}");
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<Result>> Login(UserLoginCommand command)
        {
            try
            {
                return await _mediator.Send(command);
            }
            catch (Exception ex)
            {
                return Result.Failure($"User login was not successful. Error: {ex?.Message ?? ex?.InnerException?.Message}");
            }
        }
    }
}
