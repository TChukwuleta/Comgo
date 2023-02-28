using Comgo.Application.Common.Interfaces;
using Comgo.Application.Common.Interfaces.Validators.UserValidator;
using Comgo.Core.Entities;
using Comgo.Core.Model;
using MediatR;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comgo.Application.Users.Commands
{
    public class CreateUserCommand : IRequest<Result>, IUserRequestValidator
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result>
    {
        private readonly IAuthService _authService;
        private readonly IAppDbContext _context;
        private readonly IConfiguration _config;
        public CreateUserCommandHandler(IAuthService authService, IAppDbContext context, IConfiguration config)
        {
            _authService = authService;
            _context = context;
            _config = config;
        }

        public async Task<Result> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _authService.GetUserByEmail(request.Email);
                if (user.user != null)
                {
                    return Result.Failure("User already exist with this detail");
                }
                var allUsers = await _authService.GetAllUsers(0, 0);
                var existingUser = allUsers.users.FirstOrDefault(c => c.Name.ToLower() == request.Name.ToLower());
                if (existingUser != null)
                {
                    return Result.Failure("User already exist with this detail");
                }
                var newUser = new User
                {
                    Name = request.Name,
                    Email = request.Email,
                    Password = request.Password
                };
                var result = await _authService.CreateUserAsync(newUser);
                var error = string.IsNullOrEmpty(result.result.Message) ? result.result.Messages.FirstOrDefault() : result.result.Message;
                if (!result.result.Succeeded)
                {
                    return Result.Failure(error);
                }
                return Result.Success("User creation was successful", newUser);
            }
            catch (Exception ex)
            {
                return Result.Failure(new string[] { "User creation failed", ex?.Message ?? ex?.InnerException.Message });
            }
        }
    }
}
