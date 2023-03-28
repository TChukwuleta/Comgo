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
                return Result.Failure(new string[] { "User retrieval was not successful" + ex?.Message ?? ex?.InnerException?.Message });
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
                return Result.Failure(new string[] { "Users retrieval was not successful" + ex?.Message ?? ex?.InnerException?.Message });
            }
        }
    }
}


/*command.AccessToken = accessToken.RawData;
accessToken.ValidateToken(command.UserId);*/