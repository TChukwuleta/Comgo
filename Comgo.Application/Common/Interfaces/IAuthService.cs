﻿using Comgo.Application.Common.Model;
using Comgo.Core.Entities;
using Comgo.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comgo.Application.Common.Interfaces
{
    public interface IAuthService
    {
        Task<(Result result, User user)> CreateUserAsync(User user);
        Task<Result> UpdateUserAsync(User user);
        Task<Result> UpdateUserPaymentAsync(User user, bool paid);
        Task<Result> ChangePasswordAsync(string userId, string oldPassword, string newPassword);
        Task<Result> ResetPassword(string email, string password);
        Task<Result> EmailVerification(string email, string otp);
        Task<UserLogin> Login(string email, string password);
        Task<UserLogin> GetUserToken(User user);
        Task<Result> GenerateOTP(string email, string purpose);
        Task<Result> ValidationOTP(string email, string otp, string purpose);
        Task<(Result result, User user)> GetUserByEmail(string email);
        Task<(Result result, User user)> GetUserById(string userid);
        Task<(Result result, List<User> users)> GetAllUsers(int skip, int take);
    }
}
