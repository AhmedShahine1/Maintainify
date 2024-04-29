using Maintainify.BusinessLayer.Interface;
using Maintainify.Core.DTO;
using Maintainify.Core.Entity.ApplicationData;
using Maintainify.Core.Entity.ProfessionData;
using Maintainify.Core.ModelView.AuthViewModel.RegisterData;
using Maintainify.Core.ModelView.AuthViewModel.RoleData;
using Maintainify.Core.ModelView.AuthViewModels.RegisterData;
using Maintainify.RepositoryLayer.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Maintainify.Controllers
{
    public class UserController : BaseController
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IAccountService _accountService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileHandling _fileHandling;
        private readonly BaseResponse _baseResponse = new();
        private ApplicationUser _user;

        public UserController(IUnitOfWork unitOfWork, IAccountService accountService, IFileHandling fileHandling, RoleManager<ApplicationRole> roleManager)
        {
            _fileHandling = fileHandling;
            _accountService = accountService;
            _unitOfWork = unitOfWork;
            _roleManager = roleManager;
        }
        [ApiExplorerSettings(IgnoreApi = true)]
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var accessToken = Request.Headers[HeaderNames.Authorization];
            if (string.IsNullOrEmpty(accessToken))
                return;

            var userId = User.Claims.First(i => i.Type == "uid").Value; // will give the user's userId
            var user = _unitOfWork.Users.FindByQuery(s => s.Id == userId && s.Status == true)
                .FirstOrDefault();
            _user = user;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public void OnActionExecuted(ActionExecutedContext context)
        {

        }
        //----------------------------------------------------------------------------------------------
        [HttpPost("AddRole")]
        public async Task<ActionResult<BaseResponse>> AddRoles(RoleDto roleDto, [FromHeader] string lang = "en")
        {
            if (!ModelState.IsValid)
            {
                _baseResponse.Status = false;
                _baseResponse.Message = (lang == "ar") ? "خطأ في البيانات" : "Error in data";
                _baseResponse.Code = 400;
                _baseResponse.Data = new { message = string.Join("; ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage)) };
                return BadRequest(_baseResponse);
            }
            var oldRole = await _roleManager.FindByNameAsync(roleDto.RoleName);
            if (oldRole != null)
            {
                _baseResponse.Status = false;
                _baseResponse.Message = (lang == "ar") ? "هناك دور بنفس القيمة موجود بالفعل." : "A role with the same value already exists.";
                _baseResponse.Code = 409;
                return Ok(_baseResponse);
            }

            var role = await _roleManager.CreateAsync(new ApplicationRole()
            {
                Name = roleDto.RoleName,
                NameAr = roleDto.RoleNameAr,
                Description = roleDto.Description,
            });
            if (role.Succeeded)
            {
                _baseResponse.Status = true;
                _baseResponse.Message = (lang == "ar") ? "تم اضافة الصلاحية بنجاح" : "Role added successfully";
                _baseResponse.Code = 200;
                _baseResponse.Data = new { roleDto };
                return Ok(_baseResponse);
            }
            else
            {
                _baseResponse.Status = false;
                _baseResponse.Message = (lang == "ar") ? "حدث خطأ غير متوقع أثناء حفظ البيانات. يرجى إعادة المحاولة لاحقا." : "An unexpected error occurred while saving the data. Please try again later.";
                _baseResponse.Code = 500;
                _baseResponse.Data = new { message = role.Errors.ToString() };
                return Ok(_baseResponse);
            }
        }

        [HttpPost("AddPath")]
        public async Task<ActionResult<BaseResponse>> AddPath(PathFiles pathFiles, [FromHeader] string lang = "en")
        {
            if(!ModelState.IsValid)
            {
                _baseResponse.Status = false;
                _baseResponse.Message = (lang == "ar") ? "خطأ في البيانات" : "Error in data";
                _baseResponse.Code = 400;
                _baseResponse.Data = new { message = string.Join("; ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage)) };
                return BadRequest(_baseResponse);
            }
            var oldPath= await _unitOfWork.pathFiles.FindByQuery(path=>path.Name==pathFiles.Name).FirstOrDefaultAsync();
            if (oldPath != null)
            {
                _baseResponse.Status = false;
                _baseResponse.Message = (lang == "ar") ? "هذا المسار موجود من قبل." : "Path is already in use.";
                _baseResponse.Code = 409;
                _baseResponse.Data = new { message = string.Join("; ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage)) };
                return Conflict(_baseResponse);
            }
            bool result=await _fileHandling.PathFiles(pathFiles);
            if(!result)
            {
                _baseResponse.Status = false;
                _baseResponse.Message = (lang == "ar") ? "خطأ في حفظ البيانات" : "An unexpected error occurred while saving the data. Please try again later.";
                _baseResponse.Code = 500;
                _baseResponse.Data = new { message = string.Join("; ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage)) };
                return StatusCode(500, _baseResponse);
            }
            _baseResponse.Status = true;
            _baseResponse.Message = (lang == "ar") ? "تم اضافة المسار بنجاح" : "Path added successfully";
            _baseResponse.Code = 200;
            _baseResponse.Data = new { pathFiles };
            return Ok(_baseResponse);
        }

        [HttpPost("AddProfession")]
        public async Task<ActionResult<BaseResponse>> AddProfession(Profession profession, [FromHeader] string lang = "en")
        {
            if (!ModelState.IsValid)
            {
                _baseResponse.Status = false;
                _baseResponse.Message = (lang == "ar") ? "خطأ في البيانات" : "Error in data";
                _baseResponse.Code = 400;
                _baseResponse.Data = new { message = string.Join("; ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage)) };
                return BadRequest(_baseResponse);
            }
            var oldprofession = await _unitOfWork.Profession.FindByQuery(prof => prof.Name == profession.Name).FirstOrDefaultAsync();
            if (oldprofession != null)
            {
                _baseResponse.Status = false;
                _baseResponse.Message = (lang == "ar") ? "هذا المسار موجود من قبل." : "Path is already in use.";
                _baseResponse.Code = 409;
                _baseResponse.Data = new { message = string.Join("; ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage)) };
                return Conflict(_baseResponse);
            }
            try
            {
                profession.Id = Guid.NewGuid().ToString();
                await _unitOfWork.Profession.AddAsync(profession);
                await _unitOfWork.SaveChangesAsync();
                _baseResponse.Status = true;
                _baseResponse.Message = (lang == "ar") ? "تم اضافة المسار بنجاح" : "Path added successfully";
                _baseResponse.Code = 200;
                _baseResponse.Data = new { profession };
                return Ok(_baseResponse);

            }
            catch (Exception ex)
            {
                _baseResponse.Status = false;
                _baseResponse.Message = (lang == "ar") ? "خطأ في حفظ البيانات" : "An unexpected error occurred while saving the data. Please try again later.";
                _baseResponse.Code = 500;
                _baseResponse.Data = new { message = string.Join("; ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage)) };
                return StatusCode(500, _baseResponse);
            }
        }


        [HttpPost("ProviderRegister")]
        public async Task<ActionResult<BaseResponse>> ProviderRegister([FromForm] RegisterProvider model, [FromHeader] string lang = "en")
        {
            if (!ModelState.IsValid)
            {
                _baseResponse.Status = false;
                _baseResponse.Message = (lang == "ar") ? "خطأ في البيانات" : "Error in data";
                _baseResponse.Code = 400;
                _baseResponse.Data = new { message = string.Join("; ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage)) };
                return BadRequest(_baseResponse);
            }
            var result = await _accountService.RegisterProviderAsync(model);
            if (!result.IsAuthenticated)
            {
                _baseResponse.Status = false;
                _baseResponse.Message = (lang == "ar") ? result.ArMessage : result.Message;
                _baseResponse.Code = result.ErrorCode;
                _baseResponse.Data = new { message = string.Join("; ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage)) };
                return BadRequest(_baseResponse);
            }
            else
            {
                _baseResponse.Status = true;
                _baseResponse.Message = (lang == "ar") ? result.ArMessage : result.Message;
                _baseResponse.Code = result.ErrorCode;
                _baseResponse.Data = new { result.FullName, result.PhoneNumber, result.Description, result.UserImgUrl, result.Roles, result.bankAccountNumber };
            }
            return Ok(_baseResponse);
        }

        [HttpPost("SeekerRegister")]
        public async Task<ActionResult<BaseResponse>> SeekerRegister([FromForm] RegisterSeeker model, [FromHeader] string lang = "en")
        {
            if (!ModelState.IsValid)
            {
                _baseResponse.Status = false;
                _baseResponse.Message = (lang == "ar") ? "خطأ في البيانات" : "Error in data";
                _baseResponse.Code = 400;
                _baseResponse.Data = new { message = string.Join("; ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage)) };
                return BadRequest(_baseResponse);
            }
            var result = await _accountService.RegisterSeekerAsync(model);
            if (!result.IsAuthenticated)
            {
                _baseResponse.Status = false;
                _baseResponse.Message = (lang == "ar") ? result.ArMessage : result.Message;
                _baseResponse.Code = result.ErrorCode;
                _baseResponse.Data = new { message = string.Join("; ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage)) };
                return BadRequest(_baseResponse);
            }
            else
            {
                _baseResponse.Status = true;
                _baseResponse.Message = (lang == "ar") ? result.ArMessage : result.Message;
                _baseResponse.Code = result.ErrorCode;
                _baseResponse.Data = new { result.FullName, result.PhoneNumber, result.UserImgUrl, result.Roles };
            }
            return Ok(_baseResponse);
        }

    }
}
