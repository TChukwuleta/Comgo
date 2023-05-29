using Comgo.Application.Common.Interfaces;
using Comgo.Core.Model;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Comgo.Application.Users.Commands
{
    public class UpdateFundsLockingTimeCommand : IRequest<Result>
    {
        public int StartHour { get; set; }
        public int EndHour { get; set; }
        public string UserId { get; set; }
    }

    public class UpdateFundsLockingTimeCommandHandler : IRequestHandler<UpdateFundsLockingTimeCommand, Result>
    {
        private readonly IAppDbContext _context;
        private readonly IAuthService _authService;
        public UpdateFundsLockingTimeCommandHandler(IAppDbContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }
        public async Task<Result> Handle(UpdateFundsLockingTimeCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _authService.GetUserById(request.UserId);
                if (user.user == null)
                {
                    return Result.Failure("Unable to set funds locking time period. Invalid user specified");
                }
                var userSettings = await _context.UserSettings.FirstOrDefaultAsync(c => c.UserId == request.UserId);
                if (userSettings == null || userSettings?.SecurityQuestionId <= 0)
                {
                    return Result.Failure("Kindly set up your security question and response to proceed");
                }
                userSettings.LockTimeEndHour = request.EndHour;
                userSettings.LocktimeStartHour = request.StartHour;
                _context.UserSettings.Update(userSettings);
                await _context.SaveChangesAsync(cancellationToken);
                return Result.Success("User lock time set successfully", user.user);
            }
            catch (Exception ex)
            {
                return Result.Failure($"Error occured while updating funding time. {ex.Message}");
            }
        }
    }
}
