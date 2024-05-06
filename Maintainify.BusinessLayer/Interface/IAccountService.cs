using Maintainify.Core.Entity.ApplicationData;
using Maintainify.Core.ModelView.AuthViewModel.ChangePasswordData;
using Maintainify.Core.ModelView.AuthViewModel.LoginData;
using Maintainify.Core.ModelView.AuthViewModel.RegisterData;
using Maintainify.Core.ModelView.AuthViewModel.RoleData;
using Maintainify.Core.ModelView.AuthViewModels;
using Maintainify.Core.ModelView.AuthViewModels.RegisterData;
using Maintainify.Core.ModelView.AuthViewModels.UpdateData;

namespace Maintainify.BusinessLayer.Interface
{
    public interface IAccountService
    {
        Task<List<ApplicationUser>> GetAllUsers();
        Task<ApplicationUser> GetUserById(string userId);
        Task<ApplicationUser> GetUserByPhoneNumber(string phoneNumber);
        Task<ApplicationUser> UpdateUser(ApplicationUser user);
        ////Task<AuthModel> UpdateLocation(string userId, UpdateUserLocation model);
        Task<AuthModel> RegisterProviderAsync(RegisterProvider model);
        Task<AuthModel> RegisterSeekerAsync(RegisterSeeker model);
        Task<AuthModel> LoginAsync(LoginModel model);
        Task<bool> Logout(string userName);
        //Task<AuthModel> ReSendSms(string phoneNumber);
        ////Task<AuthModel> ForgetPassword(ForgotPasswordMv forgotPasswordModelView);
        ////Task<AuthModel> ConfirmSmsAsync(ConfirmSms confirmSms);
        //Task<AuthModel> ChangePassword(string userId, string password);
        Task<AuthModel> ChangeOldPassword(string userId, ChangeOldPassword changePassword);
        Task<AuthModel> UpdateUserProfileProvider(string userId, UpdateProfileProvider updateUser);
        Task<AuthModel> UpdateUserProfileSeeker(string userId, UpdateProfileSeeker updateUser);
        Task<AuthModel> GetUserInfo(string userId);
        Task<string> AddRoleAsync(AddRoleModel model);
        IList<string> RoleUser(ApplicationUser userc);
        Task<List<string>> GetRoles();
        string ValidateJwtToken(string token);
        int GenerateRandomNo();
        ////------------------------------------------------------
        ////Task Activate(string userId);
        ////Task Suspend(string userId);
        ////Task ShowServices(string userId);
        ////Task HideServices(string userId);
        //string RandomString(int length);
        //Task<string> StopAsync();
        //Task Delete(string userId);
    }
}
