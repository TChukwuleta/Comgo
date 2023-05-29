using Comgo.Application.Common.Interfaces;
using Comgo.Core.Model;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Comgo.Application.SecurityQuestions.Queries
{
    public class GetSecurityQuestionByIdQuery : IRequest<Result>
    {
        public int Id { get; set; }
    }

    public class GetSecurityQuestionByIdQueryHandler : IRequestHandler<GetSecurityQuestionByIdQuery, Result>
    {
        private readonly IAppDbContext _context;
        public GetSecurityQuestionByIdQueryHandler(IAppDbContext context)
        {
            _context = context;
        }
        public async Task<Result> Handle(GetSecurityQuestionByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var securityQuestion = await _context.SecurityQuestions.FirstOrDefaultAsync(c => c.Id == request.Id);
                if (securityQuestion == null || securityQuestion?.Id <= 0)
                {
                    return Result.Failure("invalid security question specified");
                }
                return Result.Success("Security question retrieval was successful", securityQuestion);
            }
            catch (Exception ex)
            {
                return Result.Failure($"An error occured while retrieving system security questions by id {ex.Message}");
            }
        }
    }
}

