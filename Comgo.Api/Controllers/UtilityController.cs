using Comgo.Core.Enums;
using Comgo.Core.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Comgo.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UtilityController : ApiController
    {
        protected readonly IHttpContextAccessor _httpContextAccessor;



        [HttpGet("getpaymentmodetype")]
        public async Task<ActionResult<Result>> GetPaymentModeType()
        {
            try
            {
                return await Task.Run(() => Result.Success(
                  ((PaymentModeType[])Enum.GetValues(typeof(PaymentModeType))).Select(x => new { Value = (int)x, Name = x.ToString() }).ToList()
                  ));
            }
            catch (System.Exception ex)
            {
                return Result.Failure(new string[] { "Retrieval was not successful" + ex?.Message ?? ex?.InnerException?.Message });
            }
        }


        [HttpGet("getstatus")]
        public async Task<ActionResult<Result>> GetStatus()
        {
            try
            {
                return await Task.Run(() => Result.Success(
                  ((Status[])Enum.GetValues(typeof(Status))).Select(x => new { Value = (int)x, Name = x.ToString() }).ToList()
                  ));
            }
            catch (System.Exception ex)
            {
                return Result.Failure(new string[] { "Retrieval was not successful" + ex?.Message ?? ex?.InnerException?.Message });
            }
        }


        [HttpGet("gettransactionstatus")]
        public async Task<ActionResult<Result>> GetTransactionStatus()
        {
            try
            {
                return await Task.Run(() => Result.Success(
                  ((TransactionStatus[])Enum.GetValues(typeof(TransactionStatus))).Select(x => new { Value = (int)x, Name = x.ToString() }).ToList()
                  ));
            }
            catch (System.Exception ex)
            {
                return Result.Failure(new string[] { "Retrieval was not successful" + ex?.Message ?? ex?.InnerException?.Message });
            }
        }


        [HttpGet("gettransactiontype")]
        public async Task<ActionResult<Result>> GetTransactionType()
        {
            try
            {
                return await Task.Run(() => Result.Success(
                  ((TransactionType[])Enum.GetValues(typeof(TransactionType))).Select(x => new { Value = (int)x, Name = x.ToString() }).ToList()
                  ));
            }
            catch (System.Exception ex)
            {
                return Result.Failure(new string[] { "Retrieval was not successful" + ex?.Message ?? ex?.InnerException?.Message });
            }
        }

        [HttpGet("getusertype")]
        public async Task<ActionResult<Result>> GetUserType()
        {
            try
            {
                return await Task.Run(() => Result.Success(
                  ((UserType[])Enum.GetValues(typeof(UserType))).Select(x => new { Value = (int)x, Name = x.ToString() }).ToList()
                  ));
            }
            catch (System.Exception ex)
            {
                return Result.Failure(new string[] { "Retrieval was not successful" + ex?.Message ?? ex?.InnerException?.Message });
            }
        }
    }
}
