using Comgo.Application.Common.Interfaces;
using Comgo.Core.Model;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Comgo.Application.SecurityQuestions.Queries
{
    public class GetAllSecurityQuestionsQuery : IRequest<Result>
    {
        public int Skip { get; set; }
        public int Take { get; set; }
    }

    public class GetAllSecurityQuestionsQueryHandler : IRequestHandler<GetAllSecurityQuestionsQuery, Result>
    {
        private readonly IAppDbContext _context;
        public GetAllSecurityQuestionsQueryHandler(IAppDbContext context)
        {
            _context = context;
        }
        public async Task<Result> Handle(GetAllSecurityQuestionsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var questions = await _context.SecurityQuestions.ToListAsync();
                if (questions == null || questions.Count() <= 0)
                {
                    return Result.Failure("No security questions found");
                }
                if (request.Skip == 0 && request.Take == 0)
                {
                    return Result.Success("Security questions retrieval was successful", questions);
                }
                return Result.Success("Security questions retrieval was successful", questions.Skip(request.Skip).Take(request.Take).ToList());
            }
            catch (Exception ex)
            {
                return Result.Failure($"An error occured while retrieving all system security questions. {ex.Message}");
            }
        }
    }
}
