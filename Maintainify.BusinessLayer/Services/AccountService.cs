using Maintainify.BusinessLayer.Interface;
using Maintainify.Core.Entity.ApplicationData;
using Maintainify.Core.Entity.ProfessionData;
using Maintainify.Core.Helpers;
using Maintainify.Core.ModelView.AuthViewModel.ChangePasswordData;
using Maintainify.Core.ModelView.AuthViewModel.LoginData;
using Maintainify.Core.ModelView.AuthViewModel.RegisterData;
using Maintainify.Core.ModelView.AuthViewModel.RoleData;
using Maintainify.Core.ModelView.AuthViewModels;
using Maintainify.Core.ModelView.AuthViewModels.RegisterData;
using Maintainify.Core.ModelView.AuthViewModels.UpdateData;
using Maintainify.RepositoryLayer.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Maintainify.BusinessLayer.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileHandling _fileHandling;
        private readonly JWT _jwt;
        private readonly IHttpClientFactory _clientFactory;

        public AccountService(IHttpClientFactory clientFactory, UserManager<ApplicationUser> userManager, IFileHandling photoHandling,
            RoleManager<ApplicationRole> roleManager, IUnitOfWork unitOfWork,
            IOptions<JWT> jwt)
        {
            _clientFactory = clientFactory;
            _userManager = userManager;
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
            _jwt = jwt.Value;
            _fileHandling = photoHandling;
        }

        public async Task<List<ApplicationUser>> GetAllUsers()
        {
            return await _userManager.Users.ToListAsync();
        }

        public async Task<ApplicationUser> GetUserById(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return null;
            return user;
        }

        public async Task<ApplicationUser> GetUserByPhoneNumber(string phoneNumber)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber == phoneNumber);
            return user;
        }

        public async Task<ApplicationUser> UpdateUser(ApplicationUser user)
        {
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return user;
            }
            else
            {
                return null;
            }
        }

        //------------------------------------------------------------------------------------------------------------------
        public async Task<AuthModel> RegisterProviderAsync(RegisterProvider model)
        {
            if (await _userManager.FindByNameAsync(model.FullName) is not null)
                return new AuthModel { Message = "this Name is already Exist!", ArMessage = "هذا الاسم مستخدم من قبل", ErrorCode = 409 };

            if (await Task.Run(() => _userManager.Users.Any(item => item.PhoneNumber == model.PhoneNumber)))
                return new AuthModel { Message = "this phone number is already Exist!", ArMessage = "هذا الرقم المحمول مستخدم من قبل", ErrorCode = 409 };
            var profession = await _unitOfWork.Profession.FindByQuery(a => a.Id == model.ProfessionId).FirstAsync();
            if (profession == null)
                return new AuthModel { Message = "this Profession not found!", ArMessage = "هذه المهنه غير موجوده", ErrorCode = 409 };
            var user = new ApplicationUser
            {
                FullName = model.FullName,
                UserName = model.FullName.Replace(" ", ""),
                NormalizedUserName = model.FullName.Replace(" ", "").Normalize(),
                PhoneNumber = model.PhoneNumber,
                Description = model.Description,
                bankAccountNumber= model.bankAccountNumber,
                profession = profession,
                Status = true,
                Email=model.Email,
                RandomCode = GenerateRandomNo().ToString(),
                PhoneNumberConfirmed = true,
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Aggregate(string.Empty, (current, error) => current + $"{error.Description},");
                return new AuthModel { Message = errors, ArMessage = errors, ErrorCode = 500 };
            }
            await _userManager.AddToRoleAsync(user, "Provider");

            var Provider = await _userManager.FindByNameAsync(model.FullName.Replace(" ",""));
            Images img = await _fileHandling.ProfileImage(Provider.Id, "Profile");
            if (img == null)
                return new AuthModel { Message = "image profile is already Exist!", ArMessage = "لم يتم حفظ الصورة", ErrorCode = 500 };

            return new AuthModel
            {
                PhoneNumber = Provider.PhoneNumber,
                FullName = Provider.FullName,
                IsAuthenticated = true,
                Description = Provider.Description,
                Profession = profession,
                Roles = new List<string> { "Provider" },
                UserImgUrl = new List<string> { await _fileHandling.PathFile(img) },
                ArMessage = "تم انشاء الحساب بنجاح",
                Message = "Account created successfully",
                ErrorCode = 200,
                bankAccountNumber = Provider.bankAccountNumber,
            };
        }

        public async Task<AuthModel> RegisterSeekerAsync(RegisterSeeker model)
        {
            if (await _userManager.FindByNameAsync(model.FullName) is not null)
                return new AuthModel { Message = "this Name is already Exist!", ArMessage = "هذا الاسم مستخدم من قبل", ErrorCode = 409 };

            if (await Task.Run(() => _userManager.Users.Any(item => item.PhoneNumber == model.PhoneNumber)))
                return new AuthModel { Message = "this phone number is already Exist!", ArMessage = "هذا الرقم المحمول مستخدم من قبل", ErrorCode = 409 };

            var user = new ApplicationUser
            {
                FullName = model.FullName,
                UserName = model.FullName.Replace(" ", ""),
                NormalizedUserName = model.FullName.Replace(" ", "").Normalize(),
                PhoneNumber = model.PhoneNumber,
                Status = true,
                Email = model.Email,
                RandomCode = GenerateRandomNo().ToString(),
                PhoneNumberConfirmed = true,
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Aggregate(string.Empty, (current, error) => current + $"{error.Description},");
                return new AuthModel { Message = errors, ArMessage = errors, ErrorCode = 500 };
            }
            await _userManager.AddToRoleAsync(user, "Seeker");
            var Seeker = await _userManager.FindByNameAsync(model.FullName.Replace(" ", ""));
            Images img = await _fileHandling.ProfileImage(Seeker.Id, "Profile");
            if (img == null)
                return new AuthModel { Message = "image profile is already Exist!", ArMessage = "لم يتم حفظ الصورة", ErrorCode = 500 };

            return new AuthModel
            {
                PhoneNumber = Seeker.PhoneNumber,
                FullName = Seeker.FullName,
                IsAuthenticated = true,
                Roles = new List<string> { "Seeker" },
                UserImgUrl = new List<string> { await _fileHandling.PathFile(img) },
                ArMessage = "تم انشاء الحساب بنجاح",
                Message = "Account created successfully",
                ErrorCode = 200,
            };
        }

        //-------------------------------------------------------------------------------------------------------------------------
        public async Task<AuthModel> LoginAsync(LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Name.Replace(" ",""));
            if (user is null)
                return new AuthModel { Message = "Your Name is not Exist!", ArMessage = "المستخدم غير مسجل", ErrorCode = 401 };
            if (!await _userManager.CheckPasswordAsync(user, model.Password))
                return new AuthModel { Message = "Password is not correct!", ArMessage = "كلمة المرور غير صحيحة", ErrorCode = 401 };
            if (!user.Status)
                return new AuthModel { Message = "Your account has been suspended!", ArMessage = "حسابك تم إيقافة", ErrorCode = 401 };
            //if (!user.PhoneNumberConfirmed)
            //{
            //    var smsResult = await ReSendSms(user.PhoneNumber);
            //    return smsResult.IsAuthenticated ? new AuthModel { Message = "Your phone number is not confirmed!", ArMessage = "رقم الهاتف غير مؤكد ", ErrorCode = (int)Errors.ThisPhoneNumberNotConfirmed } : new AuthModel { Message = "Your phone number is not Correct!", ArMessage = "رقم الهاتف غير صحيح", ErrorCode = (int)Errors.ThisPhoneNumberNotConfirmed };
            //}


            var rolesList = _userManager.GetRolesAsync(user).Result.ToList();
            List<string> images = new List<string>();
            foreach (var img in _unitOfWork.images.FindByQuery(s=>s.UserId==user.Id))
            {
                images.Add(await _fileHandling.PathFile(img));
            }
            return new AuthModel
            {
                Status = true,
                UserId = user.Id,
                PhoneNumber = user.PhoneNumber,
                FullName = user.FullName,
                IsAuthenticated = true,
                Roles = rolesList,
                Token = new JwtSecurityTokenHandler().WriteToken(GenerateJwtToken(user).Result),
                UserImgUrl = images,
                Description = user.Description,
                bankAccountNumber = user.bankAccountNumber,
                ErrorCode = 200,
                Message = "Login successfully",
                ArMessage = "تم تسجيل الدخول بنجاح"
            };
        }

        public async Task<bool> Logout(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user is null)
                return false;
            await _userManager.UpdateAsync(user);
            return true;
        }

        //-------------------------------------------------------------------------------------------------------------------------
        public async Task<AuthModel> GetUserInfo(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return new AuthModel { ErrorCode = 404, Message = "User not found!", ArMessage = "المستخدم غير موجود" };

            var rolesList = _userManager.GetRolesAsync(user).Result.ToList();
            List<string> images = new List<string>();
            PathFiles pathFiles = await _unitOfWork.pathFiles.FindByQuery(x => x.type == "Profile").FirstAsync();
            foreach (var img in _unitOfWork.images.FindByQuery(s => s.UserId == user.Id && s.PathId == pathFiles.Id))
            {
                images.Add(await _fileHandling.PathFile(img));
            }
            return new AuthModel
            {
                Status = true,
                UserId = user.Id,
                PhoneNumber = user.PhoneNumber,
                FullName = user.FullName,
                IsAuthenticated = true,
                Roles = rolesList,
                Profession = user.profession,
                Token = new JwtSecurityTokenHandler().WriteToken(GenerateJwtToken(user).Result),
                UserImgUrl = images,
                Description = user.Description,
                bankAccountNumber = user.bankAccountNumber,
                ErrorCode = 200,
                Message = "information successfully",
                ArMessage = "تم حصول علي بيانات بنجاح"
            };
        }

        //-------------------------------------------------------------------------------------------------------------------------
        public async Task<string> AddRoleAsync(AddRoleModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user is null)
                return "User not found!";

            if (model.Roles != null && model.Roles.Count > 0)
            {
                foreach (var role in model.Roles)
                {
                    if (!await _roleManager.RoleExistsAsync(role))
                        return "Invalid Role";
                    if (await _userManager.IsInRoleAsync(user, role))
                        return "User already assigned to this role";
                }
                var result = await _userManager.AddToRolesAsync(user, model.Roles);

                return result.Succeeded ? string.Empty : "Something went wrong";
            }
            return " Role is empty";
        }

        public Task<List<string>> GetRoles()
        {
            return _roleManager.Roles.Select(x => x.Name).ToListAsync();
        }

        public IList<string> RoleUser(ApplicationUser user)
        {
            var role = _userManager.GetRolesAsync(user).Result.ToList();
            return role;
        }
        //------------------------------------------------------------------------------------------------------------
        public async Task<AuthModel> UpdateUserProfileProvider(string userId, UpdateProfileProvider updateUser)
        {
            try
            {
                var profession = await _unitOfWork.Profession.FindByQuery(a => a.Id == updateUser.ProfessionId).FirstAsync();
                if (profession == null)
                    return new AuthModel { Message = "this Profession not found!", ArMessage = "هذه المهنه غير موجوده", ErrorCode = 409 };
                var result = await _unitOfWork.Users.FindByQuery(s => s.Id == userId).FirstAsync();
                result.PhoneNumber = updateUser.PhoneNumber;
                result.Email = updateUser.Email;
                result.FullName = updateUser.FullName;
                result.UserName = updateUser.FullName.Replace(" ", "");
                result.Description = updateUser.Description;
                result.profession = profession;
                result.bankAccountNumber = updateUser.bankAccountNumber;
                _unitOfWork.Users.Update(result);
                await _unitOfWork.SaveChangesAsync();
                return new AuthModel
                {
                    UserId = userId,
                    PhoneNumber = result.PhoneNumber,
                    bankAccountNumber = result.bankAccountNumber,
                    Profession = result.profession,
                    FullName = result.FullName,
                    IsAuthenticated = true,
                    Roles = new List<string> {"Provider" },
                    ArMessage = "تم تحديث الحساب بنجاح",
                    Message = "Account Update successfully",
                    ErrorCode = 200,
                };
            }
            catch
            {
                return new AuthModel
                {
                    IsAuthenticated = false,
                    ArMessage = "فشل في تحديث البيانات",
                    Message = "Account Update Failed",
                    ErrorCode = 500,
                };

            }
        }

        public async Task<AuthModel> UpdateUserProfileSeeker(string userId, UpdateProfileSeeker updateUser)
        {
            try
            {
                var result = await _unitOfWork.Users.FindByQuery(s => s.Id == userId).FirstAsync();
                result.PhoneNumber = updateUser.PhoneNumber;
                result.Email = updateUser.Email;
                result.FullName = updateUser.FullName;
                result.UserName = updateUser.FullName.Replace(" ", "");
                _unitOfWork.Users.Update(result);
                await _unitOfWork.SaveChangesAsync();
                return new AuthModel
                {
                    UserId = userId,
                    PhoneNumber = result.PhoneNumber,
                    FullName = result.FullName,
                    IsAuthenticated = true,
                    Roles = new List<string> { "Seeker"},
                    ArMessage = "تم تحديث الحساب بنجاح",
                    Message = "Account Update successfully",
                    ErrorCode = 200,
                };
            }
            catch
            {
                return new AuthModel
                {
                    IsAuthenticated = false,
                    ArMessage = "فشل في تحديث البيانات",
                    Message = "Account Update Failed",
                    ErrorCode = 500,
                };

            }
        }

        //----------------------------------------------------------------------------------------------------------------
        public async Task<AuthModel> ChangeOldPassword(string userId, ChangeOldPassword changePassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return new AuthModel { Message = "Your Name is not Exist!", ArMessage = "المستخدم غير مسجل", ErrorCode = 401 };
            if (!await _userManager.CheckPasswordAsync(user, changePassword.OldPassword))
                return new AuthModel { Message = "Password is not correct!", ArMessage = "كلمة المرور غير صحيحة", ErrorCode = 401 };
            if (!user.Status)
                return new AuthModel { Message = "Your account has been suspended!", ArMessage = "حسابك تم إيقافة", ErrorCode = 401 };
            var result = await _userManager.ChangePasswordAsync(user,changePassword.OldPassword,changePassword.NewPassword);
            if (!result.Succeeded)
            {
                return new AuthModel
                {
                    UserId = userId,
                    PhoneNumber = user.PhoneNumber,
                    FullName = user.FullName,
                    IsAuthenticated = false,
                    Roles = (List<string>)RoleUser(user),
                    ArMessage = "لم تم تحديث الحساب ",
                    Message = result.Errors.ToString(),
                    ErrorCode = 500,
                };

            }
            return new AuthModel
            {
                UserId = userId,
                PhoneNumber = user.PhoneNumber,
                FullName = user.FullName,
                IsAuthenticated = true,
                Roles = (List<string>)RoleUser(user),
                ArMessage = "تم تحديث الحساب بنجاح",
                Message = "Account Update successfully",
                ErrorCode = 200,
            };

        }

        //----------------------------------------------------------------------------------------------------------------
        #region create and validate JWT token

        private async Task<JwtSecurityToken> GenerateJwtToken(ApplicationUser user, int? time = null)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            var roleClaims = roles.Select(role => new Claim("roles", role)).ToList();

            var claims = new[]
                {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("uid", user.Id),
                new Claim("Name", user.FullName),
                (user.IsAdmin) ? new Claim("isAdmin", "true") : new Claim("isAdmin", "false"),
            }
                .Union(userClaims)
                .Union(roleClaims);

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: (time != null) ? DateTime.Now.AddHours((double)time) : DateTime.Now.AddDays(_jwt.DurationInDays),
                signingCredentials: signingCredentials);

            return jwtSecurityToken;
        }


        public string ValidateJwtToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                if (token == null)
                    return null;
                if (token.StartsWith("Bearer "))
                    token = token.Replace("Bearer ", "");

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out var validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var accountId = jwtToken.Claims.First(x => x.Type == "uid").Value;

                return accountId;
            }
            catch
            {
                return null;
            }
        }

        #endregion create and validate JWT token

        #region Random number and string

        //Generate RandomNo
        public int GenerateRandomNo()
        {
            const int min = 1000;
            const int max = 9999;
            var rdm = new Random();
            return rdm.Next(min, max);
        }


        public string RandomString(int length)
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        #endregion Random number and string

    }
}
