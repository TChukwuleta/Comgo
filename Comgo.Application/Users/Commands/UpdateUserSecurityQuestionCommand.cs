using Comgo.Application.Common.Interfaces;
using Comgo.Core.Entities;
using Comgo.Core.Model;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Comgo.Application.Users.Commands
{
    public class UpdateUserSecurityQuestionCommand : IRequest<Result>
    {
        public int SecurityQuestionId { get; set; }
        public string SecurityQuestionResponse { get; set; }
        public string UserId { get; set; }
    }

    public class UpdateUserSecurityQuestionCommandHandler : IRequestHandler<UpdateUserSecurityQuestionCommand, Result>
    {
        private readonly IAppDbContext _context;
        private readonly IAuthService _authService;
        private readonly IEncryptionService _encryptionService;
        public UpdateUserSecurityQuestionCommandHandler(IAppDbContext context, IAuthService authService, IEncryptionService encryptionService)
        {
            _authService = authService;
            _context = context;
            _encryptionService = encryptionService;
        }
        public async Task<Result> Handle(UpdateUserSecurityQuestionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _authService.GetUserById(request.UserId);
                if (user.user == null)
                {
                    return Result.Failure("Unable to update user security question. Invalid user details");
                }
                var securityQuestion = await _context.SecurityQuestions.FirstOrDefaultAsync(c => c.Id == request.SecurityQuestionId);
                if (securityQuestion == null)
                {
                    return Result.Failure("Invalid security question selected");
                }
                var userSecurityQuestion = await _context.UserSettings.FirstOrDefaultAsync(c => c.UserId == request.UserId);
                if (userSecurityQuestion == null || userSecurityQuestion?.SecurityQuestionId <= 0)
                {
                    userSecurityQuestion = new UserSetting
                    {
                        SecurityQuestionId = securityQuestion.Id,
                        Email = user.user.Email,
                        SecurityQuestionResponse = _encryptionService.EncryptData(request.SecurityQuestionResponse),
                        CreatedDate = DateTime.Now,
                        Status = Core.Enums.Status.Active,
                        UserId = request.UserId
                    };
                    await _context.UserSettings.AddAsync(userSecurityQuestion);
                }
                else
                {
                    userSecurityQuestion.SecurityQuestionId = securityQuestion.Id;
                    userSecurityQuestion.SecurityQuestionResponse = request.SecurityQuestionResponse;
                    _context.UserSettings.Update(userSecurityQuestion);
                }
                await _context.SaveChangesAsync(cancellationToken);
                return Result.Success("User Settings updated successfully", user.user);
            }
            catch (Exception ex)
            {
                return Result.Failure($"An error occured while trying to update user settings. {ex.Message}");
            }
        }
    }
}
