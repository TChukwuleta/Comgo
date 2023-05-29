using Comgo.Application.Common.Interfaces;
using Comgo.Core.Entities;
using Comgo.Core.Model;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Comgo.Application.SecurityQuestions.Commands
{
    public class CreateSecurityQuestionsCommand : IRequest<Result>
    {
        public string Question { get; set; }

    }

    public class CreateSecurityQuestionsCommandHandler : IRequestHandler<CreateSecurityQuestionsCommand, Result>
    {
        private readonly IAppDbContext _context;
        public CreateSecurityQuestionsCommandHandler(IAppDbContext context)
        {
            _context = context;
        }
        public async Task<Result> Handle(CreateSecurityQuestionsCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var existingQuestion = await _context.SecurityQuestions.FirstOrDefaultAsync(c => c.Question.Trim().ToLower() == request.Question.Trim().ToLower());
                if (existingQuestion != null)
                {
                    return Result.Failure("Security question already exists in the system");
                }
                var newQuestion = new SecurityQuestion
                {
                    Question = request.Question,
                    CreatedDate = DateTime.Now,
                    Status = Core.Enums.Status.Active
                };
                await _context.SecurityQuestions.AddAsync(newQuestion);
                await _context.SaveChangesAsync(cancellationToken);
                return Result.Success("Security questions created successfully");    
            }
            catch (Exception ex)
            {
                return Result.Failure($"An error occured while creating security questions. {ex.Message}");
            }
        }
    }
}
