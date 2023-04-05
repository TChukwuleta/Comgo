using Comgo.Application.Transactions.Commands;
using Comgo.Application.Transactions.Queries;
using Comgo.Core.Model;
using Comgo.Infrastructure.Utility;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Comgo.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController : ApiController
    {
        private readonly IMediator _mediator;
        protected readonly IHttpContextAccessor _contextAccessor;
        public TransactionController(IHttpContextAccessor contextAccessor, IMediator mediator)
        {
            _mediator = mediator;
            _contextAccessor = contextAccessor;
            accessToken = _contextAccessor.HttpContext.Request.Headers["Authorization"].ToString()?.ExtractToken();
            if (accessToken == null)
            {
                throw new Exception("You are not authorized!");
            }
        }

        [HttpPost("createtransaction")]
        public async Task<ActionResult<Result>> CreateTransaction(CreateTransactionCommand command)
        {
            try
            {
                accessToken.ValidateToken(command.UserId);
                return await _mediator.Send(command);
            }
            catch (Exception ex)
            {
                return Result.Failure($"Failed to create transaction. Error: {ex?.Message ?? ex?.InnerException?.Message}");
            }
        }

        [HttpGet("getbitcointransactions/{userid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<Result>> GetBitcoinTransactions(string userid)
        {
            try
            {
                accessToken.ValidateToken(userid);
                return await _mediator.Send(new GetAllBitcoinTransactionQuery {UserId = userid });
            }
            catch (Exception ex)
            {
                return Result.Failure($"Transactions retrieval by user failed. Error: {ex?.Message ?? ex?.InnerException?.Message}");
            }
        }


        [HttpGet("gettransactionsbyid/{skip}/{take}/{userid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<Result>> GetAllTransactionsByUser(int skip, int take, string userid)
        {
            try
            {
                accessToken.ValidateToken(userid);
                return await _mediator.Send(new GetAllTransactionsQuery { Skip = skip, Take = take, UserId = userid });
            }
            catch (Exception ex)
            {
                return Result.Failure($"Transactions retrieval by user failed. Error: {ex?.Message ?? ex?.InnerException?.Message}");
            }
        }

        [HttpGet("getbyid/{id}/{userid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<Result>> GetTransactionById(int id, string userid)
        {
            try
            {
                accessToken.ValidateToken(userid);
                return await _mediator.Send(new GetTransactionByIdQuery { Id = id, UserId = userid });
            }
            catch (Exception ex)
            {
                return Result.Failure($"Transaction retrieval by id failed. Error: {ex?.Message ?? ex?.InnerException?.Message}");
            }
        }

        [HttpGet("getbytxnid/{txnref}/{userid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<Result>> GetTransactionByTxnId(string txnref, string userid)
        {
            try
            {
                accessToken.ValidateToken(userid);
                return await _mediator.Send(new GetTransactionByReferenceQuery { Reference = txnref, UserId = userid });
            }
            catch (Exception ex)
            {
                return Result.Failure($"Transaction retrieval by reference failed. Error: {ex?.Message ?? ex?.InnerException?.Message}");
            }
        }

        [HttpGet("getallcredit/{skip}/{take}/{userid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<Result>> GetAllDebitTransactionsByUser(int skip, int take, string userid)
        {
            try
            {
                accessToken.ValidateToken(userid);
                return await _mediator.Send(new GetCreditTransactionByUserIdQuery { UserId = userid, Skip = skip, Take = take });
            }
            catch (Exception ex)
            {
                return Result.Failure($"Credit transactions retrieval failed. Error: {ex?.Message ?? ex?.InnerException?.Message}");
            }
        }

        [HttpGet("getalldebit/{skip}/{take}/{userid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<Result>> GetDebitTransactionsByUer(int skip, int take, string userid)
        {
            try
            {
                accessToken.ValidateToken(userid);
                return await _mediator.Send(new GetDebitTransactionByUserIdQuery { UserId = userid, Skip = skip, Take = take });
            }
            catch (Exception ex)
            {
                return Result.Failure($"Debit transactions retrieval failed. Error: {ex?.Message ?? ex?.InnerException?.Message}");
            }
        }
    }
}
