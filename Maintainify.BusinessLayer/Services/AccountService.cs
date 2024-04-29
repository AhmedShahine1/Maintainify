using Maintainify.BusinessLayer.Interface;
using Maintainify.Core.Entity.ApplicationData;
using Maintainify.Core.Helpers;
using Maintainify.Core.ModelView.AuthViewModel.RegisterData;
using Maintainify.Core.ModelView.AuthViewModel.RoleData;
using Maintainify.Core.ModelView.AuthViewModels;
using Maintainify.Core.ModelView.AuthViewModels.RegisterData;
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

        //public async Task<ApplicationUser> UpdateUser(ApplicationUser user)
        //{
        //    var result = await _userManager.UpdateAsync(user);
        //    if (result.Succeeded)
        //    {
        //        return user;
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}



        //------------------------------------------------------------------------------------------------------------------
        public async Task<AuthModel> RegisterProviderAsync(RegisterProvider model)
        {
            if (await _userManager.FindByNameAsync(model.FullName) is not null)
                return new AuthModel { Message = "this Name is already Exist!", ArMessage = "هذا الاسم مستخدم من قبل", ErrorCode = 409 };

            if (await Task.Run(() => _userManager.Users.Any(item => item.PhoneNumber == model.PhoneNumber)))
                return new AuthModel { Message = "this phone number is already Exist!", ArMessage = "هذا الرقم المحمول مستخدم من قبل", ErrorCode = 409 };

            Images img = await _fileHandling.ProfileImage();
            if(img == null)
                return new AuthModel { Message = "image profile is already Exist!", ArMessage = "لم يتم حفظ الصورة", ErrorCode = 500 };
            var user = new ApplicationUser
            {
                FullName = model.FullName,
                UserName = model.FullName.Replace(" ", ""),
                NormalizedUserName = model.FullName.Replace(" ", "").Normalize(),
                PhoneNumber = model.PhoneNumber,
                images = new List<Images> { img },
                Description = model.Description,
                bankAccountNumber= model.bankAccountNumber,
                profession = await _unitOfWork.Profession.FindByQuery(a => a.Id == model.ProfessionId).FirstAsync(),
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
            return new AuthModel
            {
                PhoneNumber = Provider.PhoneNumber,
                FullName = Provider.FullName,
                IsAuthenticated = true,
                Description = Provider.Description,
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

            Images img = await _fileHandling.ProfileImage();
            if (img == null)
                return new AuthModel { Message = "image profile is already Exist!", ArMessage = "لم يتم حفظ الصورة", ErrorCode = 500 };
            var user = new ApplicationUser
            {
                FullName = model.FullName,
                UserName = model.FullName.Replace(" ", ""),
                NormalizedUserName = model.FullName.Replace(" ", "").Normalize(),
                PhoneNumber = model.PhoneNumber,
                images = new List<Images> { img },
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

        ////public async Task<AuthModel> RegisterAdminAsync(RegisterAdminMv model)
        ////{
        ////    if (await _userManager.FindByEmailAsync(model.Email) is not null)
        ////        return new AuthModel { Message = "this email is already Exist!", ArMessage = "هذا البريد الالكتروني مستخدم من قبل", ErrorCode = (int)Errors.ThisEmailAlreadyExist };

        ////    if (await Task.Run(() => _userManager.Users.Any(item => item.PhoneNumber == model.PhoneNumber)))
        ////        return new AuthModel { Message = "this phone number is already Exist!", ArMessage = "هذا الرقم المحمول مستخدم من قبل", ErrorCode = (int)Errors.ThisPhoneNumberAlreadyExist };


        ////    string imgUrl;
        ////    try
        ////    {
        ////        if (model.Img is not null)
        ////            imgUrl = await _fileHandling.UploadFile(model.Img, "AdminImg");
        ////        else
        ////            return new AuthModel { Message = "please select your Img !", ArMessage = " من فضلك حدد صورة شخصية لك ", ErrorCode = (int)Errors.NoPhoto };
        ////    }
        ////    catch
        ////    {
        ////        return new AuthModel { Message = "error in uploading Img!", ArMessage = "حدث خطأ في رفع الصورة", ErrorCode = (int)Errors.ErrorInUploadingImg };
        ////    }

        ////    var user = new ApplicationUser
        ////    {
        ////        FullName = model.FullName,
        ////        UserName = model.PhoneNumber,
        ////        NormalizedUserName = model.PhoneNumber,
        ////        PhoneNumber = model.PhoneNumber,
        ////        Email = model.Email,
        ////        UserImgUrl = imgUrl,
        ////        Status = true,
        ////        RandomCode = GenerateRandomNo().ToString(),
        ////        PhoneNumberConfirmed = true,
        ////        UserType = UserType.Admin,
        ////        IsAdmin = true,
        ////        IsApproved = true
        ////    };
        ////    var result = await _userManager.CreateAsync(user, model.Password);
        ////    if (!result.Succeeded)
        ////    {
        ////        var errors =
        ////            result.Errors.Aggregate(string.Empty, (current, error) => current + $"{error.Description},");
        ////        return new AuthModel { Message = errors, ArMessage = errors, ErrorCode = (int)Errors.ErrorWithCreateAccount };
        ////    }
        ////    await _userManager.AddToRoleAsync(user, "Admin");

        ////    var smsResult = await SmsService.SendMessage(user.PhoneNumber, "تم أنشاء الحساب بنجاح");
        ////    if (smsResult != null)
        ////    {
        ////        var admin = await _userManager.FindByNameAsync(model.PhoneNumber);
        ////        if (admin is null)
        ////            return new AuthModel
        ////            {
        ////                Message = "An error occurred creating the account!",
        ////                ArMessage = "حدث خطأ أثناء إنشاء الحساب!",
        ////                ErrorCode = (int)Errors.ErrorWithCreateAccount
        ////            };
        ////        return new AuthModel
        ////        {
        ////            Email = admin.Email,
        ////            PhoneNumber = admin.PhoneNumber,
        ////            FullName = admin.FullName,
        ////            IsAuthenticated = true,
        ////            Roles = new List<string> { "Admin" },
        ////            UserType = UserType.Admin,
        ////            IsAdmin = true,
        ////            IsApproved = admin.IsApproved,
        ////            UserImgUrl = admin.UserImgUrl,
        ////        };
        ////    }
        ////    else
        ////        return new AuthModel { Message = "Error in sending sms!", ArMessage = "حدث خطأ في ارسال الرسالة", ErrorCode = (int)Errors.ErrorWithSendingCode };

        ////}

        ////public async Task<AuthModel> RegisterUserAsync(RegisterUserMv model)
        ////{
        ////    if (await _userManager.FindByEmailAsync(model.Email) is not null)
        ////        return new AuthModel { Message = "this email is already Exist!", ArMessage = "هذا البريد الالكتروني مستخدم من قبل", ErrorCode = (int)Errors.ThisEmailAlreadyExist };

        ////    if (await Task.Run(() => _userManager.Users.Any(item => item.PhoneNumber == model.PhoneNumber)))
        ////        return new AuthModel { Message = "this phone number is already Exist!", ArMessage = "هذا الرقم المحمول مستخدم من قبل", ErrorCode = (int)Errors.ThisPhoneNumberAlreadyExist };

        ////    var city = await _unitOfWork.Cities.FindAsync(s => s.Id == model.CityId && s.IsDeleted == false);
        ////    if (city is null)
        ////        return new AuthModel { Message = "this city is not Exist!", ArMessage = "هذه المدينة غير موجودة", ErrorCode = (int)Errors.ThisCityIsNotExist };

        ////    string imgUrl;
        ////    try
        ////    {
        ////        if (!string.IsNullOrEmpty(model.Img))
        ////            imgUrl = await _fileHandling.UploadPhotoBase64(model.Img, "UserImg");
        ////        else
        ////            return new AuthModel { Message = "please select  Img for Center!", ArMessage = " من فضلك حدد صورة شخصية للمركز ", ErrorCode = (int)Errors.NoPhoto };
        ////    }
        ////    catch
        ////    {
        ////        return new AuthModel { Message = "error in uploading Img!", ArMessage = "حدث خطأ في رفع الصورة", ErrorCode = (int)Errors.ErrorInUploadingImg };
        ////    }

        ////    var user = new ApplicationUser
        ////    {
        ////        FullName = model.FullName,
        ////        UserName = model.PhoneNumber,
        ////        NormalizedUserName = model.PhoneNumber,
        ////        PhoneNumber = model.PhoneNumber,
        ////        Email = model.Email,
        ////        UserImgUrl = imgUrl,
        ////        Status = true,
        ////        RandomCode = GenerateRandomNo().ToString(),
        ////        PhoneNumberConfirmed = false,
        ////        UserType = UserType.User,
        ////        IsAdmin = false,
        ////        IsApproved = true,
        ////        CityId = model.CityId
        ////    };
        ////    var result = await _userManager.CreateAsync(user, model.Password);
        ////    if (!result.Succeeded)
        ////    {
        ////        var errors =
        ////            result.Errors.Aggregate(string.Empty, (current, error) => current + $"{error.Description},");
        ////        return new AuthModel { Message = errors, ArMessage = errors, ErrorCode = (int)Errors.ErrorWithCreateAccount };
        ////    }
        ////    await _userManager.AddToRoleAsync(user, "User");

        ////    var smsResult = await SmsService.SendMessage(user.PhoneNumber, user.RandomCode);
        ////    if (smsResult != null)
        ////    {
        ////        var normalUser = await _userManager.FindByNameAsync(model.PhoneNumber);
        ////        if (normalUser is null)
        ////            return new AuthModel
        ////            {
        ////                Message = "An error occurred creating the account!",
        ////                ArMessage = "حدث خطأ أثناء إنشاء الحساب!",
        ////                ErrorCode = (int)Errors.ErrorWithCreateAccount
        ////            };
        ////        return new AuthModel
        ////        {
        ////            Email = normalUser.Email,
        ////            PhoneNumber = normalUser.PhoneNumber,
        ////            FullName = normalUser.FullName,
        ////            IsAuthenticated = true,
        ////            Roles = new List<string> { "User" },
        ////            UserType = UserType.User,
        ////            UserImgUrl = normalUser.UserImgUrl,
        ////            PhoneVerify = normalUser.PhoneNumberConfirmed,
        ////            Message = "Message sent successfully Check your phone",
        ////            ArMessage = "تم أرسال الرسالة بنجاح تحقق من هاتفك "
        ////        };

        ////    }
        ////    else
        ////        return new AuthModel { Message = "Error in sending code!", ArMessage = "حدث خطأ في ارسال الكود", ErrorCode = (int)Errors.ErrorWithSendingCode };
        ////}

        //////-------------------------------------------------------------------------------------------------------------------------
        ////public async Task<AuthModel> LoginAsync(LoginModel model)
        ////{
        ////    var user = await _userManager.FindByNameAsync(model.PhoneNumber);
        ////    if (user is null)
        ////        return new AuthModel { Message = "Your phone number is not Exist!", ArMessage = "رقم الهاتف غير مسجل", ErrorCode = (int)Errors.ThisPhoneNumberNotExist };
        ////    if (!await _userManager.CheckPasswordAsync(user, model.Password))
        ////        return new AuthModel { Message = "Password is not correct!", ArMessage = "كلمة المرور غير صحيحة", ErrorCode = (int)Errors.TheUsernameOrPasswordIsIncorrect };
        ////    if (!user.Status)
        ////        return new AuthModel { Message = "Your account has been suspended!", ArMessage = "حسابك تم إيقافة", ErrorCode = (int)Errors.UserIsBlocked };
        ////    if (!user.IsApproved)
        ////        return new AuthModel { Message = "Your account has not been approved!", ArMessage = "حسابك لم يتم الموافقة عليه", ErrorCode = (int)Errors.UserIsNotApproved };
        ////    if (!user.PhoneNumberConfirmed)
        ////    {
        ////        var smsResult = await ReSendSms(user.PhoneNumber);
        ////        return smsResult.IsAuthenticated ? new AuthModel { Message = "Your phone number is not confirmed!", ArMessage = "رقم الهاتف غير مؤكد ", ErrorCode = (int)Errors.ThisPhoneNumberNotConfirmed } : new AuthModel { Message = "Your phone number is not Correct!", ArMessage = "رقم الهاتف غير صحيح", ErrorCode = (int)Errors.ThisPhoneNumberNotConfirmed };
        ////    }


        ////    var rolesList = _userManager.GetRolesAsync(user).Result.ToList();
        ////    return new AuthModel
        ////    {
        ////        UserId = user.Id,
        ////        Email = user.Email,
        ////        PhoneNumber = user.PhoneNumber,
        ////        FullName = user.FullName,
        ////        IsAuthenticated = true,
        ////        Roles = rolesList,
        ////        UserType = user.UserType,
        ////        IsAdmin = user.IsAdmin,
        ////        IsApproved = user.IsApproved,
        ////        PhoneVerify = user.PhoneNumberConfirmed,
        ////        Token = new JwtSecurityTokenHandler().WriteToken(GenerateJwtToken(user).Result),
        ////        UserImgUrl = user.UserImgUrl,
        ////        CityId = user.CityId ?? 0,
        ////        City = user.City?.NameAr,
        ////        Description = user.Description,
        ////        Message = "Login successfully",
        ////        ArMessage = "تم تسجيل الدخول بنجاح"


        ////    };
        ////}

        //public async Task<bool> Logout(string userName)
        //{
        //    var user = await _userManager.FindByNameAsync(userName);
        //    if (user is null)
        //        return false;
        //    await _userManager.UpdateAsync(user);
        //    return true;
        //}

        //public async Task<AuthModel> ReSendSms(string phoneNumber)
        //{
        //    //var user = await _userManager.FindByNameAsync(phoneNumber);
        //    //if (user is null)
        //    //    return new AuthModel { Message = "Your phone number is not Exist!", ArMessage = "رقم الهاتف غير مسجل", ErrorCode = (int)Errors.ThisPhoneNumberNotExist };
        //    //if (user.PhoneNumberConfirmed)
        //    //    return new AuthModel { Message = "Your phone number is Confirmed!", ArMessage = "رقم الهاتف مؤكد", ErrorCode = (int)Errors.ThisPhoneNumberAlreadyConfirmed111 };

        //    //user.RandomCode = GenerateRandomNo().ToString();
        //    //await _userManager.UpdateAsync(user);
        //    //var smsResult = await SmsService.SendMessage(user.PhoneNumber, user.RandomCode);
        //    //return smsResult != null ? new AuthModel { Message = "Message sent successfully Check your phone", ArMessage = "تم أرسال الرسالة بنجاح تحقق من هاتفك ", IsAuthenticated = true } : new AuthModel { Message = "Your phone number is not Correct!", ArMessage = "رقم الهاتف غير صحيح", ErrorCode = (int)Errors.ThisPhoneNumberNotConfirmed };

        //}


        ////-------------------------------------------------------------------------------------------------------------------------
        ////public async Task<AuthModel> ForgetPassword(ForgotPasswordMv forgotPasswordModelView)
        ////{
        ////    var user = await _userManager.FindByNameAsync(forgotPasswordModelView.PhoneNumber);

        ////    if (user is null)
        ////        return new AuthModel { Message = "User not found!", ArMessage = "المستخدم غير موجود", ErrorCode = (int)Errors.TheUserNotExistOrDeleted };

        ////    user.RandomCode = GenerateRandomNo().ToString();
        ////    await _userManager.UpdateAsync(user);
        ////    var result = await SmsService.SendMessage(user.PhoneNumber, user.RandomCode);
        ////    if (result != null)
        ////    {
        ////        return new AuthModel
        ////        {
        ////            Email = user.Email,
        ////            PhoneNumber = user.PhoneNumber,
        ////            Message = "SMS sent successfully!",
        ////            ArMessage = "تم ارسال الرسالة بنجاح",
        ////            IsAuthenticated = true,
        ////        };
        ////    }
        ////    else
        ////        return new AuthModel { Message = "Error in sending code!", ArMessage = "حدث خطأ في ارسال الكود", ErrorCode = (int)Errors.ErrorWithSendingCode };
        ////}

        ////public async Task<AuthModel> ConfirmSmsAsync(ConfirmSms confirmSms)
        ////{
        ////    var user = await _userManager.FindByNameAsync(confirmSms.PhoneNumber);

        ////    if (user is null)
        ////        return new AuthModel { Message = "User not found!", ArMessage = "المستخدم غير موجود", ErrorCode = (int)Errors.TheUserNotExistOrDeleted };

        ////    if (user.RandomCode != confirmSms.RandomCode && confirmSms.RandomCode != "3241")
        ////        return new AuthModel { Message = "Invalid random number!", ArMessage = "رقم التحقق غير صحيح", ErrorCode = (int)Errors.VerificationNumberIsIncorrect };

        ////    var jwtSecurityToken = await GenerateJwtToken(user, 1);
        ////    var rolesList = await _userManager.GetRolesAsync(user);

        ////    var result = new AuthModel
        ////    {
        ////        UserId = user.Id,
        ////        UserImgUrl = user.UserImgUrl,
        ////        Message = "Validation code is correct",
        ////        ArMessage = "رمز التحقق صحيح",
        ////        Email = user.Email,
        ////        PhoneNumber = user.PhoneNumber,
        ////        FullName = user.FullName,
        ////        IsAuthenticated = true,
        ////        UserType = user.UserType,
        ////        IsAdmin = user.IsAdmin,
        ////        IsApproved = user.IsApproved,
        ////        PhoneVerify = user.PhoneNumberConfirmed,
        ////        Roles = rolesList.ToList(),
        ////        Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken)
        ////    };
        ////    return result;
        ////}

        //public async Task<AuthModel> ChangePassword(string userId, string password)
        //{
        //    var user = await _userManager.FindByIdAsync(userId);
        //    if (user is null)
        //        return new AuthModel { Message = "User not found!", ArMessage = "المستخدم غير موجود", ErrorCode = (int)Errors.TheUserNotExistOrDeleted };

        //    user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, password);
        //    await _userManager.UpdateAsync(user);

        //    var jwtSecurityToken = await GenerateJwtToken(user, 1);
        //    var rolesList = await _userManager.GetRolesAsync(user);

        //    var result = new AuthModel
        //    {
        //        Message = "The password has been changed successfully",
        //        ArMessage = "تم تغيير كلمة المرور بنجاح",
        //        Email = user.Email,
        //        PhoneNumber = user.PhoneNumber,
        //        FullName = user.FullName,
        //        UserImgUrl = user.UserImgUrl,
        //        IsAuthenticated = true,
        //        Roles = rolesList.ToList(),
        //        UserType = user.UserType,
        //        IsAdmin = user.IsAdmin,
        //        IsApproved = user.IsApproved,
        //        PhoneVerify = user.PhoneNumberConfirmed,
        //        Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken)
        //    };
        //    return result;
        //}

        ////public async Task<AuthModel> ChangeOldPasswordAsync(string userId, ChangeOldPassword changePassword)
        ////{
        ////    var user = await _userManager.FindByIdAsync(userId);
        ////    if (user is null)
        ////        return new AuthModel { Message = "User not found!", ArMessage = "المستخدم غير موجود", ErrorCode = (int)Errors.TheUserNotExistOrDeleted };

        ////    if (user.PasswordHash != null)
        ////    {
        ////        var isOldCorrect = _userManager.PasswordHasher.VerifyHashedPassword(user, user.PasswordHash, changePassword.OldPassword);
        ////        if (!isOldCorrect.Equals(PasswordVerificationResult.Success))
        ////            return new AuthModel { Message = "Old password is incorrect!", ArMessage = "كلمة المرور القديمة غير صحيحة", ErrorCode = (int)Errors.OldPasswordIsIncorrect };
        ////    }
        ////    else
        ////    {
        ////        return new AuthModel { Message = "Old password is not Exist!", ArMessage = "كلمة المرور القديمة غير موجودة", ErrorCode = (int)Errors.OldPasswordIsExist };
        ////    }

        ////    user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, changePassword.NewPassword);
        ////    await _userManager.UpdateAsync(user);

        ////    var jwtSecurityToken = await GenerateJwtToken(user, 1);
        ////    var rolesList = await _userManager.GetRolesAsync(user);

        ////    var result = new AuthModel
        ////    {
        ////        Message = "The password has been changed successfully",
        ////        ArMessage = "تم تغيير كلمة المرور بنجاح",
        ////        Email = user.Email,
        ////        PhoneNumber = user.PhoneNumber,
        ////        UserImgUrl = user.UserImgUrl,
        ////        FullName = user.FullName,
        ////        IsAuthenticated = true,
        ////        Roles = rolesList.ToList(),
        ////        UserType = user.UserType,
        ////        IsAdmin = user.IsAdmin,
        ////        IsApproved = user.IsApproved,
        ////        PhoneVerify = user.PhoneNumberConfirmed,
        ////        Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken)
        ////    };
        ////    return result;
        ////}

        ////-------------------------------------------------------------------------------------------------------------------------

        ////public async Task<AuthModel> UpdateFreeAgentProfile(string userId, UpdateFreeAgentMv model)
        ////{
        ////    var user = await _userManager.FindByIdAsync(userId);
        ////    if (user is null)
        ////        return new AuthModel { ErrorCode = (int)Errors.TheUserNotExistOrDeleted, Message = "User not found!", ArMessage = "المستخدم غير موجود" };
        ////    if (await Task.Run(() => _userManager.Users.Any(item => (item.PhoneNumber == model.PhoneNumber) && (item.Id != userId))))
        ////        return new AuthModel { Message = "this phone number is already Exist!", ArMessage = "هذا الرقم المحمول مستخدم من قبل" };
        ////    if (await Task.Run(() => _userManager.Users.Any(item => (item.Email == model.Email) && (item.Id != userId))))
        ////        return new AuthModel { Message = "this email is already Exist!", ArMessage = "هذا البريد الالكتروني مستخدم من قبل" };
        ////    string imgUrl = null;
        ////    try
        ////    {
        ////        if (!string.IsNullOrEmpty(model.Img))
        ////            imgUrl = await _fileHandling.UploadPhotoBase64(model.Img, "freeLancerImg", user.UserImgUrl);
        ////    }
        ////    catch
        ////    {
        ////        return new AuthModel { Message = "error in uploading Img!", ArMessage = "حدث خطأ في رفع الصورة", ErrorCode = (int)Errors.ErrorInUploadingImg };
        ////    }
        ////    string formImgUrl = null;
        ////    try
        ////    {
        ////        if (!string.IsNullOrEmpty(model.FreelanceFormImg))
        ////            formImgUrl = await _fileHandling.UploadPhotoBase64(model.FreelanceFormImg, "freeLancerFormImg", user.FreelanceFormImgUrl);
        ////    }
        ////    catch
        ////    {
        ////        return new AuthModel { Message = "error in uploading Img!", ArMessage = "حدث خطأ في رفع الصورة", ErrorCode = (int)Errors.ErrorInUploadingImg };
        ////    }

        ////    user.FullName = model.FullName;
        ////    user.PhoneNumber = model.PhoneNumber;
        ////    user.UserName = model.PhoneNumber;
        ////    user.NormalizedUserName = model.PhoneNumber;
        ////    user.Email = model.Email;
        ////    user.NormalizedEmail = model.Email;
        ////    user.UserImgUrl = (!string.IsNullOrEmpty(imgUrl)) ? imgUrl : user.UserImgUrl;
        ////    user.FreelanceFormImgUrl = (!string.IsNullOrEmpty(formImgUrl)) ? formImgUrl : user.FreelanceFormImgUrl;
        ////    user.Lat = model.Lat ?? user.Lat;
        ////    user.Lng = model.Lng ?? user.Lng;
        ////    user.Description = model.Description;

        ////    await _userManager.UpdateAsync(user);

        ////    var jwtSecurityToken = await GenerateJwtToken(user);
        ////    var rolesList = await _userManager.GetRolesAsync(user);

        ////    var result = new AuthModel
        ////    {
        ////        Message = "The profile has been updated successfully",
        ////        ArMessage = "تم تحديث الملف الشخصي بنجاح",
        ////        Email = user.Email,
        ////        PhoneNumber = user.PhoneNumber,
        ////        FullName = user.FullName,
        ////        IsAuthenticated = true,
        ////        UserType = user.UserType,
        ////        IsAdmin = user.IsAdmin,
        ////        Roles = rolesList.ToList(),
        ////        IsApproved = user.IsApproved,
        ////        PhoneVerify = user.PhoneNumberConfirmed,
        ////        UserImgUrl = user.UserImgUrl,
        ////        Description = user.Description,
        ////        Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken)
        ////    };
        ////    return result;
        ////}

        ////public async Task<AuthModel> UpdateCenterProfile(string userId, UpdateCenterMv model)
        ////{
        ////    var user = await _userManager.FindByIdAsync(userId);
        ////    if (user is null)
        ////        return new AuthModel { ErrorCode = (int)Errors.TheUserNotExistOrDeleted, Message = "User not found!", ArMessage = "المستخدم غير موجود" };
        ////    if (await Task.Run(() => _userManager.Users.Any(item => (item.PhoneNumber == model.PhoneNumber) && (item.Id != userId))))
        ////        return new AuthModel { Message = "this phone number is already Exist!", ArMessage = "هذا الرقم المحمول مستخدم من قبل" };
        ////    if (await Task.Run(() => _userManager.Users.Any(item => (item.Email == model.Email) && (item.Id != userId))))
        ////        return new AuthModel { Message = "this email is already Exist!", ArMessage = "هذا البريد الالكتروني مستخدم من قبل" };

        ////    string imgUrl = null;
        ////    try
        ////    {
        ////        if (!string.IsNullOrEmpty(model.Img))
        ////            imgUrl = await _fileHandling.UploadPhotoBase64(model.Img, "CenterImg", user.UserImgUrl);
        ////    }
        ////    catch
        ////    {
        ////        return new AuthModel { Message = "error in uploading Img!", ArMessage = "حدث خطأ في رفع الصورة", ErrorCode = (int)Errors.ErrorInUploadingImg };
        ////    }

        ////    user.FullName = model.FullName;
        ////    user.PhoneNumber = model.PhoneNumber;
        ////    user.UserName = model.PhoneNumber;
        ////    user.NormalizedUserName = model.PhoneNumber;
        ////    user.Email = model.Email;
        ////    user.NormalizedEmail = model.Email;
        ////    user.TaxNumber = model.TaxNumber;
        ////    user.Lat = model.Lat ?? user.Lat;
        ////    user.Lng = model.Lng ?? user.Lng;
        ////    user.UserImgUrl = (!string.IsNullOrEmpty(imgUrl)) ? imgUrl : user.UserImgUrl;
        ////    user.Description = model.Description;

        ////    await _userManager.UpdateAsync(user);

        ////    var jwtSecurityToken = await GenerateJwtToken(user);
        ////    var rolesList = await _userManager.GetRolesAsync(user);

        ////    var result = new AuthModel
        ////    {
        ////        Message = "The profile has been updated successfully",
        ////        ArMessage = "تم تحديث الملف الشخصي بنجاح",
        ////        Email = user.Email,
        ////        PhoneNumber = user.PhoneNumber,
        ////        FullName = user.FullName,
        ////        IsAuthenticated = true,
        ////        UserType = user.UserType,
        ////        IsAdmin = user.IsAdmin,
        ////        IsApproved = user.IsApproved,
        ////        PhoneVerify = user.PhoneNumberConfirmed,
        ////        UserImgUrl = user.UserImgUrl,
        ////        Description = user.Description,
        ////        Roles = rolesList.ToList(),
        ////        Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken)
        ////    };
        ////    return result;
        ////}

        ////public async Task<AuthModel> UpdateAdminProfile(string userId, UpdateAdminMv model)
        ////{
        ////    var user = await _userManager.FindByIdAsync(userId);
        ////    if (user is null)
        ////        return new AuthModel { ErrorCode = (int)Errors.TheUserNotExistOrDeleted, Message = "User not found!", ArMessage = "المستخدم غير موجود" };
        ////    if (await Task.Run(() => _userManager.Users.Any(item => (item.PhoneNumber == model.PhoneNumber) && (item.Id != userId))))
        ////        return new AuthModel { Message = "this phone number is already Exist!", ArMessage = "هذا الرقم المحمول مستخدم من قبل" };
        ////    if (await Task.Run(() => _userManager.Users.Any(item => (item.Email == model.Email) && (item.Id != userId))))
        ////        return new AuthModel { Message = "this email is already Exist!", ArMessage = "هذا البريد الالكتروني مستخدم من قبل" };

        ////    string imgUrl = null;
        ////    try
        ////    {
        ////        if (model.Img != null)
        ////            imgUrl = await _fileHandling.UploadFile(model.Img, "AdminImg", user.UserImgUrl);
        ////    }
        ////    catch
        ////    {
        ////        return new AuthModel { Message = "error in uploading Img!", ArMessage = "حدث خطأ في رفع الصورة", ErrorCode = (int)Errors.ErrorInUploadingImg };
        ////    }


        ////    user.FullName = model.FullName;
        ////    user.PhoneNumber = model.PhoneNumber;
        ////    user.UserName = model.PhoneNumber;
        ////    user.NormalizedUserName = model.PhoneNumber;
        ////    user.Email = model.Email;
        ////    user.NormalizedEmail = model.Email;
        ////    user.UserImgUrl = (!string.IsNullOrEmpty(imgUrl)) ? imgUrl : user.UserImgUrl;


        ////    await _userManager.UpdateAsync(user);

        ////    var jwtSecurityToken = await GenerateJwtToken(user);
        ////    var rolesList = await _userManager.GetRolesAsync(user);

        ////    var result = new AuthModel
        ////    {
        ////        Message = "The profile has been updated successfully",
        ////        ArMessage = "تم تحديث الملف الشخصي بنجاح",
        ////        Email = user.Email,
        ////        PhoneNumber = user.PhoneNumber,
        ////        FullName = user.FullName,
        ////        IsAuthenticated = true,
        ////        UserType = user.UserType,
        ////        IsAdmin = user.IsAdmin,
        ////        IsApproved = user.IsApproved,
        ////        Roles = rolesList.ToList(),
        ////        PhoneVerify = user.PhoneNumberConfirmed,
        ////        UserImgUrl = user.UserImgUrl,
        ////        Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken)
        ////    };
        ////    return result;
        ////}

        ////public async Task<AuthModel> UpdateUserProfile(string userId, UpdateUserMv model)
        ////{
        ////    var user = await _userManager.FindByIdAsync(userId);
        ////    if (user is null)
        ////        return new AuthModel { ErrorCode = (int)Errors.TheUserNotExistOrDeleted, Message = "User not found!", ArMessage = "المستخدم غير موجود" };
        ////    if (await Task.Run(() => _userManager.Users.Any(item => (item.PhoneNumber == model.PhoneNumber) && (item.Id != userId))))
        ////        return new AuthModel { Message = "this phone number is already Exist!", ArMessage = "هذا الرقم المحمول مستخدم من قبل" };
        ////    if (await Task.Run(() => _userManager.Users.Any(item => (item.Email == model.Email) && (item.Id != userId))))
        ////        return new AuthModel { Message = "this email is already Exist!", ArMessage = "هذا البريد الالكتروني مستخدم من قبل" };

        ////    var city = await _unitOfWork.Cities.FindAsync(s => s.Id == model.CityId && s.IsDeleted == false);
        ////    if (city is null)
        ////        return new AuthModel { Message = "this city is not Exist!", ArMessage = "هذه المدينة غير موجودة", ErrorCode = (int)Errors.ThisCityIsNotExist };


        ////    string imgUrl = null;
        ////    try
        ////    {
        ////        if (!string.IsNullOrEmpty(model.Img))
        ////            imgUrl = await _fileHandling.UploadPhotoBase64(model.Img, "UserImg", user.UserImgUrl);
        ////    }
        ////    catch
        ////    {
        ////        return new AuthModel { Message = "error in uploading Img!", ArMessage = "حدث خطأ في رفع الصورة", ErrorCode = (int)Errors.ErrorInUploadingImg };
        ////    }



        ////    user.FullName = model.FullName;
        ////    user.PhoneNumber = model.PhoneNumber;
        ////    user.UserName = model.PhoneNumber;
        ////    user.NormalizedUserName = model.PhoneNumber;
        ////    user.Email = model.Email;
        ////    user.NormalizedEmail = model.Email;
        ////    user.Lat = model.Lat ?? user.Lat;
        ////    user.Lng = model.Lng ?? user.Lng;
        ////    user.UserImgUrl = (!string.IsNullOrEmpty(imgUrl)) ? imgUrl : user.UserImgUrl;
        ////    user.CityId = model.CityId;


        ////    await _userManager.UpdateAsync(user);

        ////    var jwtSecurityToken = await GenerateJwtToken(user);
        ////    var rolesList = await _userManager.GetRolesAsync(user);

        ////    var result = new AuthModel
        ////    {
        ////        Message = "The profile has been updated successfully",
        ////        ArMessage = "تم تحديث الملف الشخصي بنجاح",
        ////        Email = user.Email,
        ////        PhoneNumber = user.PhoneNumber,
        ////        FullName = user.FullName,
        ////        IsAuthenticated = true,
        ////        UserType = user.UserType,
        ////        IsAdmin = user.IsAdmin,
        ////        IsApproved = user.IsApproved,
        ////        PhoneVerify = user.PhoneNumberConfirmed,
        ////        Roles = rolesList.ToList(),
        ////        UserImgUrl = user.UserImgUrl,
        ////        Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken)
        ////    };
        ////    return result;
        ////}

        ////public async Task<AuthModel> UpdateLocation(string userId, UpdateUserLocation model)
        ////{

        ////    var user = await _userManager.FindByIdAsync(userId);
        ////    if (user is null)
        ////        return new AuthModel { ErrorCode = (int)Errors.TheUserNotExistOrDeleted, Message = "User not found!", ArMessage = "المستخدم غير موجود" };

        ////    user.Lat = model.Lat;
        ////    user.Lng = model.Lng;

        ////    await _userManager.UpdateAsync(user);
        ////    return new AuthModel()
        ////    {
        ////        ErrorCode = (int)Errors.Success,
        ////        ArMessage = "تم تحديث الموقع بنجاح",
        ////        Message = "Location Updated Successfully",
        ////        FullName = user.FullName,
        ////        Email = user.Email,
        ////        PhoneNumber = user.PhoneNumber,
        ////        IsAuthenticated = true,
        ////        UserImgUrl = user.UserImgUrl,
        ////        UserType = user.UserType,
        ////    };
        ////}

        //public async Task<AuthModel> GetUserInfo(string userId)
        //{
        //    var user = await _userManager.FindByIdAsync(userId);
        //    if (user is null)
        //        return new AuthModel { ErrorCode = (int)Errors.TheUserNotExistOrDeleted, Message = "User not found!", ArMessage = "المستخدم غير موجود" };

        //    var rolesList = await _userManager.GetRolesAsync(user);
        //    var result = new AuthModel
        //    {
        //        Email = user.Email,
        //        PhoneNumber = user.PhoneNumber,
        //        FullName = user.FullName,
        //        IsAuthenticated = true,
        //        UserType = user.UserType,
        //        IsAdmin = user.IsAdmin,
        //        IsApproved = user.IsApproved,
        //        PhoneVerify = user.PhoneNumberConfirmed,
        //        Roles = rolesList.ToList(),
        //        Lat = user.Lat,
        //        Lng = user.Lng,
        //        TaxNumber = user.TaxNumber,
        //        FreelanceFormUrl = user.FreelanceFormImgUrl,
        //        Status = user.Status,
        //        UserImgUrl = user.UserImgUrl,
        //        CityId = user.CityId ?? 0,
        //        Description = user.Description,
        //        DeviceToken = user.DeviceToken,
        //    };
        //    return result;
        //}

        ////----------------------*------------------------------------------------------------------------------------

        ////public async Task<AuthModel> UpdateCenterProfileAdmin(string userId, UpdateCenterModel model)
        ////{
        ////    var user = await _userManager.FindByIdAsync(userId);
        ////    if (user is null)
        ////        return new AuthModel { ErrorCode = (int)Errors.TheUserNotExistOrDeleted, Message = "User not found!", ArMessage = "المستخدم غير موجود" };
        ////    if (await Task.Run(() => _userManager.Users.Any(item => (item.PhoneNumber == model.PhoneNumber) && (item.Id != userId))))
        ////        return new AuthModel { Message = "this phone number is already Exist!", ArMessage = "هذا الرقم المحمول مستخدم من قبل" };
        ////    if (await Task.Run(() => _userManager.Users.Any(item => (item.Email == model.Email) && (item.Id != userId))))
        ////        return new AuthModel { Message = "this email is already Exist!", ArMessage = "هذا البريد الالكتروني مستخدم من قبل" };

        ////    string imgUrl = null;
        ////    try
        ////    {
        ////        if (model.ImgFile != null)
        ////            imgUrl = await _fileHandling.UploadFile(model.ImgFile, "AdminImg", user.UserImgUrl);
        ////    }
        ////    catch
        ////    {
        ////        return new AuthModel { Message = "error in uploading Img!", ArMessage = "حدث خطأ في رفع الصورة", ErrorCode = (int)Errors.ErrorInUploadingImg };
        ////    }

        ////    user.FullName = model.FullName;
        ////    user.PhoneNumber = model.PhoneNumber;
        ////    user.UserName = model.PhoneNumber;
        ////    user.NormalizedUserName = model.PhoneNumber;
        ////    user.Email = model.Email;
        ////    user.NormalizedEmail = model.Email;
        ////    user.TaxNumber = model.TaxNumber;
        ////    user.UserImgUrl = (!string.IsNullOrEmpty(imgUrl)) ? imgUrl : user.UserImgUrl;
        ////    user.Description = model.Description;
        ////    user.CityId = model.CityId;

        ////    await _userManager.UpdateAsync(user);

        ////    var jwtSecurityToken = await GenerateJwtToken(user);
        ////    var rolesList = await _userManager.GetRolesAsync(user);

        ////    var result = new AuthModel
        ////    {
        ////        Message = "The profile has been updated successfully",
        ////        ArMessage = "تم تحديث الملف الشخصي بنجاح",
        ////        Email = user.Email,
        ////        PhoneNumber = user.PhoneNumber,
        ////        FullName = user.FullName,
        ////        IsAuthenticated = true,
        ////        UserType = user.UserType,
        ////        IsAdmin = user.IsAdmin,
        ////        IsApproved = user.IsApproved,
        ////        PhoneVerify = user.PhoneNumberConfirmed,
        ////        UserImgUrl = user.UserImgUrl,
        ////        Description = user.Description,
        ////        Roles = rolesList.ToList(),
        ////        Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken)
        ////    };
        ////    return result;
        ////}

        ////public async Task<AuthModel> UpdateFreeAgentProfileAdmin(string userId, UpdateFreeAgentModel model)
        ////{
        ////    var user = await _userManager.FindByIdAsync(userId);
        ////    if (user is null)
        ////        return new AuthModel { ErrorCode = (int)Errors.TheUserNotExistOrDeleted, Message = "User not found!", ArMessage = "المستخدم غير موجود" };
        ////    if (await Task.Run(() => _userManager.Users.Any(item => (item.PhoneNumber == model.PhoneNumber) && (item.Id != userId))))
        ////        return new AuthModel { Message = "this phone number is already Exist!", ArMessage = "هذا الرقم المحمول مستخدم من قبل" };
        ////    if (await Task.Run(() => _userManager.Users.Any(item => (item.Email == model.Email) && (item.Id != userId))))
        ////        return new AuthModel { Message = "this email is already Exist!", ArMessage = "هذا البريد الالكتروني مستخدم من قبل" };

        ////    string imgUrl = null;
        ////    try
        ////    {
        ////        if (model.ImgFile != null)
        ////            imgUrl = await _fileHandling.UploadFile(model.ImgFile, "AdminImg", user.UserImgUrl);
        ////    }
        ////    catch
        ////    {
        ////        return new AuthModel { Message = "error in uploading Img!", ArMessage = "حدث خطأ في رفع الصورة", ErrorCode = (int)Errors.ErrorInUploadingImg };
        ////    }
        ////    string formImgUrl = null;
        ////    try
        ////    {
        ////        if (model.FormImgFile != null)
        ////            formImgUrl = await _fileHandling.UploadFile(model.FormImgFile, "AdminImg", user.UserImgUrl);
        ////    }
        ////    catch
        ////    {
        ////        return new AuthModel { Message = "error in uploading Img!", ArMessage = "حدث خطأ في رفع الصورة", ErrorCode = (int)Errors.ErrorInUploadingImg };
        ////    }

        ////    user.FullName = model.FullName;
        ////    user.PhoneNumber = model.PhoneNumber;
        ////    user.UserName = model.PhoneNumber;
        ////    user.NormalizedUserName = model.PhoneNumber;
        ////    user.Email = model.Email;
        ////    user.NormalizedEmail = model.Email;
        ////    user.UserImgUrl = (!string.IsNullOrEmpty(imgUrl)) ? imgUrl : user.UserImgUrl;
        ////    user.FreelanceFormImgUrl = (!string.IsNullOrEmpty(formImgUrl)) ? formImgUrl : user.FreelanceFormImgUrl;
        ////    user.Description = model.Description;
        ////    user.CityId = model.CityId;

        ////    await _userManager.UpdateAsync(user);

        ////    var jwtSecurityToken = await GenerateJwtToken(user);
        ////    var rolesList = await _userManager.GetRolesAsync(user);

        ////    var result = new AuthModel
        ////    {
        ////        Message = "The profile has been updated successfully",
        ////        ArMessage = "تم تحديث الملف الشخصي بنجاح",
        ////        Email = user.Email,
        ////        PhoneNumber = user.PhoneNumber,
        ////        FullName = user.FullName,
        ////        IsAuthenticated = true,
        ////        UserType = user.UserType,
        ////        IsAdmin = user.IsAdmin,
        ////        Roles = rolesList.ToList(),
        ////        IsApproved = user.IsApproved,
        ////        PhoneVerify = user.PhoneNumberConfirmed,
        ////        UserImgUrl = user.UserImgUrl,
        ////        Description = user.Description,
        ////        Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken)
        ////    };
        ////    return result;
        ////}

        ////public async Task<AuthModel> UpdateUserProfileAdmin(string userId, UpdateUserModel model)
        ////{
        ////    var user = await _userManager.FindByIdAsync(userId);
        ////    if (user is null)
        ////        return new AuthModel { ErrorCode = (int)Errors.TheUserNotExistOrDeleted, Message = "User not found!", ArMessage = "المستخدم غير موجود" };
        ////    if (await Task.Run(() => _userManager.Users.Any(item => (item.PhoneNumber == model.PhoneNumber) && (item.Id != userId))))
        ////        return new AuthModel { Message = "this phone number is already Exist!", ArMessage = "هذا الرقم المحمول مستخدم من قبل" };
        ////    if (await Task.Run(() => _userManager.Users.Any(item => (item.Email == model.Email) && (item.Id != userId))))
        ////        return new AuthModel { Message = "this email is already Exist!", ArMessage = "هذا البريد الالكتروني مستخدم من قبل" };

        ////    string imgUrl = null;
        ////    try
        ////    {
        ////        if (model.ImgFile != null)
        ////            imgUrl = await _fileHandling.UploadFile(model.ImgFile, "AdminImg", user.UserImgUrl);
        ////    }
        ////    catch
        ////    {
        ////        return new AuthModel { Message = "error in uploading Img!", ArMessage = "حدث خطأ في رفع الصورة", ErrorCode = (int)Errors.ErrorInUploadingImg };
        ////    }
        ////    string formImgUrl = null;
        ////    user.FullName = model.FullName;
        ////    user.PhoneNumber = model.PhoneNumber;
        ////    user.UserName = model.PhoneNumber;
        ////    user.NormalizedUserName = model.PhoneNumber;
        ////    user.Email = model.Email;
        ////    user.NormalizedEmail = model.Email;
        ////    user.UserImgUrl = (!string.IsNullOrEmpty(imgUrl)) ? imgUrl : user.UserImgUrl;
        ////    user.FreelanceFormImgUrl = (!string.IsNullOrEmpty(formImgUrl)) ? formImgUrl : user.FreelanceFormImgUrl;
        ////    user.Description = model.Description;
        ////    user.CityId = model.CityId;

        ////    await _userManager.UpdateAsync(user);

        ////    var jwtSecurityToken = await GenerateJwtToken(user);
        ////    var rolesList = await _userManager.GetRolesAsync(user);

        ////    var result = new AuthModel
        ////    {
        ////        Message = "The profile has been updated successfully",
        ////        ArMessage = "تم تحديث الملف الشخصي بنجاح",
        ////        Email = user.Email,
        ////        PhoneNumber = user.PhoneNumber,
        ////        FullName = user.FullName,
        ////        IsAuthenticated = true,
        ////        UserType = user.UserType,
        ////        IsAdmin = user.IsAdmin,
        ////        Roles = rolesList.ToList(),
        ////        IsApproved = user.IsApproved,
        ////        PhoneVerify = user.PhoneNumberConfirmed,
        ////        UserImgUrl = user.UserImgUrl,
        ////        Description = user.Description,
        ////        Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken)
        ////    };
        ////    return result;
        ////}

        ////------------------------------------------------------------------------------------------------------------
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

        ////------------------------------------------------------------------------------------------------------------

        ////------------------------------------------------------------------------------------------------------------

        //#region create and validate JWT token

        //private async Task<JwtSecurityToken> GenerateJwtToken(ApplicationUser user, int? time = null)
        //{
        //    var userClaims = await _userManager.GetClaimsAsync(user);
        //    var roles = await _userManager.GetRolesAsync(user);
        //    var roleClaims = roles.Select(role => new Claim("roles", role)).ToList();
        //    var userType = user.UserType.ToString();

        //    var claims = new[]
        //        {
        //        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        //        new Claim("uid", user.Id),
        //        new Claim("Name", user.FullName),
        //        new Claim("userType",userType),
        //        (user.IsAdmin) ? new Claim("isAdmin", "true") : new Claim("isAdmin", "false"),
        //    }
        //        .Union(userClaims)
        //        .Union(roleClaims);

        //    var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
        //    var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

        //    var jwtSecurityToken = new JwtSecurityToken(
        //        issuer: _jwt.Issuer,
        //        audience: _jwt.Audience,
        //        claims: claims,
        //        expires: (time != null) ? DateTime.Now.AddHours((double)time) : DateTime.Now.AddDays(_jwt.DurationInDays),
        //        signingCredentials: signingCredentials);

        //    return jwtSecurityToken;
        //}


        //public string ValidateJwtToken(string token)
        //{
        //    var tokenHandler = new JwtSecurityTokenHandler();
        //    try
        //    {
        //        if (token == null)
        //            return null;
        //        if (token.StartsWith("Bearer "))
        //            token = token.Replace("Bearer ", "");

        //        tokenHandler.ValidateToken(token, new TokenValidationParameters
        //        {
        //            ValidateIssuerSigningKey = true,
        //            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key)),
        //            ValidateIssuer = false,
        //            ValidateAudience = false,
        //            ClockSkew = TimeSpan.Zero
        //        }, out var validatedToken);

        //        var jwtToken = (JwtSecurityToken)validatedToken;
        //        var accountId = jwtToken.Claims.First(x => x.Type == "uid").Value;

        //        return accountId;
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}

        //#endregion create and validate JWT token

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

        ////------------------------------------------------------------------------------------------------------------

        //public async Task<string> StopAsync()
        //{
        //    var request = new HttpRequestMessage(HttpMethod.Get, "http://stopapplication.visooft-code.com/api/StopApplication");
        //    var client = _clientFactory.CreateClient();
        //    var response = await client.SendAsync(request);
        //    if (response.IsSuccessStatusCode)
        //    {
        //        var result = await response.Content.ReadAsStringAsync();
        //        return result; // result will be either "true" or "false"
        //    }
        //    else
        //    {
        //        return "false";
        //    }
        //}

        //public Task Delete(string userId)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
