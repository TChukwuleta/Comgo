using Comgo.Application.Users.Queries;
using Comgo.Core.Model;
using Comgo.Infrastructure.Utility;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Comgo.Api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UserController : ApiController
    {
        private readonly IMediator _mediator;
        protected readonly IHttpContextAccessor _contextAccessor;
        public UserController(IMediator mediator, IHttpContextAccessor contextAccessor)
        {
            _mediator = mediator;
            _contextAccessor = contextAccessor;
            accessToken = _contextAccessor.HttpContext.Request.Headers["Authorization"].ToString()?.ExtractToken();
            if (accessToken == null)
            {
                throw new Exception("You are not authorized!");
            }
        }

        [HttpGet("generateaddress/{userId}")]
        public async Task<ActionResult<Result>> GenerateAddress(string userId)
        {
            try
            {
                accessToken.ValidateToken(userId);
                return await _mediator.Send(new GetNewUserMultisigAddressQuery
                {
                    UserId = userId
                });
            }
            catch (Exception ex)
            {
                return Result.Failure($"Address generation was not successful. {ex?.Message ?? ex?.InnerException?.Message }");
            }
        }

        [HttpGet("getwalletbalance/{userId}")]
        public async Task<ActionResult<Result>> GetWalletBalance(string userId)
        {
            try
            {
                accessToken.ValidateToken(userId);
                return await _mediator.Send(new GetUserWalletBalanceQuery
                {
                    UserId = userId
                });
            }
            catch (Exception ex)
            {
                return Result.Failure($"User wallet balance retrieval was not successful.{ ex?.Message ?? ex?.InnerException?.Message }");
            }
        }
        

        [HttpGet("getusersbyid/{userId}")]
        public async Task<ActionResult<Result>> GetUserByRoleId(string userId)
        {
            try
            {
                accessToken.ValidateToken(userId);
                return await _mediator.Send(new GetUserByIdQuery
                {
                    UserId = userId
                });
            }
            catch (Exception ex)
            {
                return Result.Failure($"User retrieval was not successful.{ ex?.Message ?? ex?.InnerException?.Message }");
            }
        }

        [HttpGet("getall/{skip}/{take}/{email}")]
        public async Task<ActionResult<Result>> GetAllUsers(int skip, int take, string email)
        {
            try
            {
                return await _mediator.Send(new GetAllUsersQuery
                {
                    Email = email,
                    Skip = skip,
                    Take = take
                });
            }
            catch (Exception ex)
            {
                return Result.Failure($"Users retrieval was not successful.{ ex?.Message ?? ex?.InnerException?.Message }");
            }
        }
    }
}