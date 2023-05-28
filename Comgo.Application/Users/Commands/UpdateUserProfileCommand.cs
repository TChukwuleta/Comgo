using Comgo.Application.Common.Interfaces;
using Comgo.Core.Model;
using MediatR;

namespace Comgo.Application.Users.Commands
{
    public class UpdateUserProfileCommand : IRequest<Result>
    {
        public string Name { get; set; }
        public string Bio { get; set; }
        public string Location { get; set; }
        public string UserId { get; set; }
    }

    public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, Result>
    {
        private readonly IAuthService _authService;
        private readonly IAppDbContext _context;
        public UpdateUserProfileCommandHandler(IAuthService authService, IAppDbContext context)
        {
            _context = context;
            _authService = authService;
        }
        public async Task<Result> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var existingUser = await _authService.GetUserById(request.UserId);
                if (existingUser.user == null)
                {
                    return Result.Failure("Invalid user details");
                }
                existingUser.user.Bio = request.Bio;
                existingUser.user.Location = request.Location;
                existingUser.user.Name = request.Name;
                var updatedUser = await _authService.UpdateUserAsync(existingUser.user);
                if (!updatedUser.Succeeded)
                {
                    return Result.Failure(updatedUser.Message);
                }
                return Result.Success("User update was successful", existingUser.user);
            }
            catch (Exception ex)
            {
                return Result.Failure($"User profile update was not successful. {ex?.Message ?? ex?.InnerException.Message }");
            }
        }
    }
}
