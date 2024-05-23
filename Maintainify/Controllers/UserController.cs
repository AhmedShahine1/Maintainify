using Maintainify.BusinessLayer.Interface;
using Maintainify.Core.DTO;
using Maintainify.Core.Entity.ApplicationData;
using Maintainify.Core.Entity.ProfessionData;
using Maintainify.Core.ModelView.AuthViewModel.ChangePasswordData;
using Maintainify.Core.ModelView.AuthViewModel.LoginData;
using Maintainify.Core.ModelView.AuthViewModel.RegisterData;
using Maintainify.Core.ModelView.AuthViewModel.RoleData;
using Maintainify.Core.ModelView.AuthViewModels;
using Maintainify.Core.ModelView.AuthViewModels.RegisterData;
using Maintainify.Core.ModelView.AuthViewModels.UpdateData;
using Maintainify.RepositoryLayer.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using static System.Net.Mime.MediaTypeNames;

namespace Maintainify.Controllers
{
    public class UserController : BaseController
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IAccountService _accountService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileHandling _fileHandling;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly BaseResponse _baseResponse = new();

        public UserController(IUnitOfWork unitOfWork, IAccountService accountService, IFileHandling fileHandling, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
        {
            _userManager = userManager;
            _fileHandling = fileHandling;
            _accountService = accountService;
            _unitOfWork = unitOfWork;
            _roleManager = roleManager;
        }
        //----------------------------------------------------------------------------------------------
        [HttpPost("AddRole")]
        public async Task<ActionResult<BaseResponse>> AddRoles([FromForm] RoleDto roleDto, [FromHeader] string lang = "en")
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
        public async Task<ActionResult<BaseResponse>> AddPath([FromForm] PathFiles pathFiles, [FromHeader] string lang = "en")
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
        public async Task<ActionResult<BaseResponse>> AddProfession([FromForm] Profession profession, [FromHeader] string lang = "en")
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
                _baseResponse.Message = (lang == "ar") ? "هذا المسار موجود من قبل." : "Profession is already in use.";
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
                _baseResponse.Message = (lang == "ar") ? "تم اضافة المسار بنجاح" : "Profession added successfully";
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

        [HttpGet("GetProfession")]
        public async Task<ActionResult<BaseResponse>> GetProfession([FromHeader] string lang = "en")
        {
            try
            {
                var profession = await _unitOfWork.Profession.GetAllAsync();
                _baseResponse.Status = true;
                _baseResponse.Message = (lang == "ar") ? "تم جلب المسار بنجاح" : "Get Professions successfully";
                _baseResponse.Code = 200;
                _baseResponse.Data = new { profession };
                return Ok(_baseResponse);

            }
            catch (Exception ex)
            {
                _baseResponse.Status = false;
                _baseResponse.Message = (lang == "ar") ? "خطأ في جلب البيانات" : "An unexpected error occurred while get the data. Please try again later.";
                _baseResponse.Code = 500;
                _baseResponse.Data = new { message = string.Join("; ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage)) };
                return StatusCode(500, _baseResponse);
            }
        }
        //---------------------------------------------------------------------------------------------- Register api
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
                _baseResponse.Code = 200;
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
                _baseResponse.Code = 200;
                _baseResponse.Data = new { result.FullName, result.PhoneNumber, result.UserImgUrl, result.Roles };
            }
            return Ok(_baseResponse);
        }

        //----------------------------------------------------------------------------------------------------- update user profile
        [HttpPut("UpdateProfileImage")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<BaseResponse>> UpdateProfileImage([FromForm] IFormFile ProfileImage, [FromHeader] string lang = "en")
        {
            if (!ModelState.IsValid)
            {
                _baseResponse.Message = (lang == "ar") ? "خطأ في البيانات" : "Error in data";
                _baseResponse.Code = 400;
                _baseResponse.Status = false;
                _baseResponse.Data = new { message = string.Join("; ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage)) };
                return BadRequest(_baseResponse);
            }
            var userId = this.User.Claims.First(i => i.Type == "uid").Value; // will give the user's userId
            var user = await _unitOfWork.Users.FindByQuery(x => x.Id == userId && x.Status == true).FirstOrDefaultAsync();
            if (user == null)
            {
                _baseResponse.Message = (lang == "ar") ? "لم يتم العثور علي المستخدم" : "User is Exits";
                _baseResponse.Code = 400;
                _baseResponse.Status = false;
                _baseResponse.Data = new { };
                return StatusCode(400, _baseResponse);

            }
            try
            {
                PathFiles pathFiles = await _unitOfWork.pathFiles.FindByQuery(x => x.type == "Profile").FirstOrDefaultAsync();
                Images image = await _unitOfWork.images.FindByQuery(s => s.UserId == userId && s.PathId == pathFiles.Id).FirstOrDefaultAsync();
                if (!await _fileHandling.DeleteFile(image))
                {
                    _baseResponse.Message = (lang == "ar") ? " خطا في تحديث الصورة" : "Error in update Profile";
                    _baseResponse.Code = 500;
                    _baseResponse.Status = false;
                    _baseResponse.Data = new { };
                    return StatusCode(500, _baseResponse);
                }
                Images newImages = await _fileHandling.UploadFile(ProfileImage, "Profile", userId);
                List<string> images = new List<string>();
                foreach (var img in _unitOfWork.images.FindByQuery(s => s.UserId == userId && s.PathId == pathFiles.Id))
                {
                    images.Add(await _fileHandling.PathFile(img));
                }
                _baseResponse.Code = 200;
                _baseResponse.Status = true;
                _baseResponse.Message = (lang == "ar") ? "تم تحديث الصورة" : "Update Profile successfully";
                _baseResponse.Data = new
                {
                    ImageUrl=images
                };
                return Ok(_baseResponse);
            }
            catch (Exception ex)
            {
                _baseResponse.Message = ex.Message;
                _baseResponse.Code = 500;
                _baseResponse.Status = false;
                _baseResponse.Data = new { message = string.Join("; ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage)) };
                return StatusCode(500,_baseResponse);
            }
        }

        [HttpPut("UpdateProfileProvider")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<BaseResponse>> UpdateProfileProvider([FromForm] UpdateProfileProvider updateProfile, [FromHeader] string lang = "en")
        {
            if (!ModelState.IsValid)
            {
                _baseResponse.Message = (lang == "ar") ? "خطأ في البيانات" : "Error in data";
                _baseResponse.Code = 400;
                _baseResponse.Status = false;
                _baseResponse.Data = new { message = string.Join("; ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage)) };
                return BadRequest(_baseResponse);
            }
            var userId = this.User.Claims.First(i => i.Type == "uid").Value; // will give the user's userId
            var user = await _unitOfWork.Users.FindByQuery(x => x.Id == userId && x.Status==true).FirstOrDefaultAsync();
            if(user == null)
            {
                _baseResponse.Message = (lang == "ar") ? "لم يتم العثور علي المستخدم" : "User is Exits";
                _baseResponse.Code = 400;
                _baseResponse.Status = false;
                _baseResponse.Data = new { };
                return StatusCode(400, _baseResponse);

            }
            IList<string> roleUser = _accountService.RoleUser(user);
            if (!roleUser.Contains("Provider"))
            {
                _baseResponse.Message = (lang == "ar") ? "المستخدم لا يملك صلاحيه" : "User is not Provider";
                _baseResponse.Code = 401;
                _baseResponse.Status = false;
                _baseResponse.Data = new { updateProfile };
                return Unauthorized(_baseResponse);
            }

            PathFiles pathFiles = await _unitOfWork.pathFiles.FindByQuery(x => x.type == "PreviousWork").FirstOrDefaultAsync();
            IEnumerable<Images> image = _unitOfWork.images.FindByQuery(s => s.UserId == userId && s.PathId == pathFiles.Id).AsQueryable();
            if(image.Count()!=0)
            {
                foreach (var imageItem in image)
                {
                    var DeleteFile = await _fileHandling.DeleteFile(imageItem);
                    if (!DeleteFile)
                    {
                        _baseResponse.Message = (lang == "ar") ? " خطا في تحديث الصورة" : "Error in update Previous Work";
                        _baseResponse.Code = 500;
                        _baseResponse.Status = false;
                        _baseResponse.Data = new { };
                        return StatusCode(500, _baseResponse);
                    }
                }

            }
            try
            {
                var result = await _accountService.UpdateUserProfileProvider(userId,updateProfile);
                if (!result.IsAuthenticated)
                {
                    _baseResponse.Status = false;
                    _baseResponse.Code = result.ErrorCode;
                    _baseResponse.Message = (lang == "ar") ? result.ArMessage : result.Message;
                    _baseResponse.Data = new { updateProfile };
                    return BadRequest(_baseResponse);
                }

                _baseResponse.Code = 200;
                _baseResponse.Status = true;
                _baseResponse.Message = (lang == "ar") ? "تم تحديث البروفايل" : "Update Profile successfully";
                _baseResponse.Data = new
                {
                    result.UserId,
                    result.FullName,
                    result.Token,
                    Role = result.Roles,
                    result.PhoneNumber,
                    result.Description,
                    result.bankAccountNumber
                };
                return Ok(_baseResponse);
            }
            catch (Exception ex)
            {
                _baseResponse.Message = ex.Message;
                _baseResponse.Code = 500;
                _baseResponse.Status = false;
                _baseResponse.Data = new { message = string.Join("; ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage)) };
                return StatusCode(500, _baseResponse);
            }
        }

        [HttpPut("UpdateProfileSeeker")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<BaseResponse>> UpdateProfileSeeker([FromForm] UpdateProfileSeeker updateProfile, [FromHeader] string lang = "en")
        {
            if (!ModelState.IsValid)
            {
                _baseResponse.Message = (lang == "ar") ? "خطأ في البيانات" : "Error in data";
                _baseResponse.Code = 400;
                _baseResponse.Status = false;
                _baseResponse.Data = new { message = string.Join("; ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage)) };
                return BadRequest(_baseResponse);
            }
            var userId = this.User.Claims.First(i => i.Type == "uid").Value; // will give the user's userId
            var user = await _unitOfWork.Users.FindByQuery(x => x.Id == userId && x.Status == true).FirstOrDefaultAsync();
            if (user == null)
            {
                _baseResponse.Message = (lang == "ar") ? "لم يتم العثور علي المستخدم" : "User is Exits";
                _baseResponse.Code = 400;
                _baseResponse.Status = false;
                _baseResponse.Data = new { };
                return StatusCode(400, _baseResponse);

            }
            IList<string> roleUser = _accountService.RoleUser(user);
            if (!roleUser.Contains("Seeker"))
            {
                _baseResponse.Message = (lang == "ar") ? "المستخدم لا يملك صلاحيه" : "User is not Seeker";
                _baseResponse.Code = 401;
                _baseResponse.Status = false;
                _baseResponse.Data = new { updateProfile };
                return Unauthorized(_baseResponse);
            }
            try
            {
                var result = await _accountService.UpdateUserProfileSeeker(userId, updateProfile);
                if (!result.IsAuthenticated)
                {
                    _baseResponse.Status = false;
                    _baseResponse.Code = result.ErrorCode;
                    _baseResponse.Message = (lang == "ar") ? result.ArMessage : result.Message;
                    _baseResponse.Data = new { updateProfile };
                    return BadRequest(_baseResponse);
                }

                _baseResponse.Code = 200;
                _baseResponse.Status = true;
                _baseResponse.Message = (lang == "ar") ? "تم تحديث البروفايل" : "Update Profile successfully";
                _baseResponse.Data = new
                {
                    result.UserId,
                    result.FullName,
                    Role = result.Roles,
                    result.PhoneNumber,
                };
                return Ok(_baseResponse);
            }
            catch (Exception ex)
            {
                _baseResponse.Message = ex.Message;
                _baseResponse.Code = 500;
                _baseResponse.Status = false;
                _baseResponse.Data = new { message = string.Join("; ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage)) };
                return StatusCode(500, _baseResponse);
            }
        }
        //----------------------------------------------------------------------------------------------------- previous Work Provider
        [HttpDelete("DeletePreviousWork")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<BaseResponse>> DeletePreviousWork([FromForm] string ImageId, [FromHeader] string lang = "en")
        {
            if (!ModelState.IsValid)
            {
                _baseResponse.Message = (lang == "ar") ? "خطأ في البيانات" : "Error in data";
                _baseResponse.Code = 400;
                _baseResponse.Status = false;
                _baseResponse.Data = new { message = string.Join("; ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage)) };
                return BadRequest(_baseResponse);
            }
            var userId = this.User.Claims.First(i => i.Type == "uid").Value; // will give the user's userId
            var user = await _unitOfWork.Users.FindByQuery(x => x.Id == userId && x.Status == true).FirstOrDefaultAsync();
            if (user == null)
            {
                _baseResponse.Message = (lang == "ar") ? "لم يتم العثور علي المستخدم" : "User is Exits";
                _baseResponse.Code = 400;
                _baseResponse.Status = false;
                _baseResponse.Data = new { };
                return StatusCode(400, _baseResponse);

            }
            IList<string> roleUser = _accountService.RoleUser(user);
            if (!roleUser.Contains("Provider"))
            {
                _baseResponse.Message = (lang == "ar") ? "المستخدم لا يملك صلاحيه" : "User is not Seeker";
                _baseResponse.Code = 401;
                _baseResponse.Status = false;
                _baseResponse.Data = new { ImageId };
                return Unauthorized(_baseResponse);
            }
            var Image = _unitOfWork.images.FindByQuery(s => s.Id == ImageId && s.UserId == userId).First();
            if (Image == null)
            {
                _baseResponse.Message = (lang == "ar") ? "لم يتم العثور علي الصورة" : "Image is Exits";
                _baseResponse.Code = 400;
                _baseResponse.Status = false;
                _baseResponse.Data = new { };
                return StatusCode(400, _baseResponse);

            }
            try
            {
                _unitOfWork.images.Delete(Image);
                await _unitOfWork.SaveChangesAsync();
                _baseResponse.Code = 200;
                _baseResponse.Status = true;
                _baseResponse.Message = (lang == "ar") ? "تم حذف الصورة" : "Delete Image successfully";
                _baseResponse.Data = new
                {
                };
                return Ok(_baseResponse);
            }
            catch (Exception ex)
            {
                _baseResponse.Message = ex.Message;
                _baseResponse.Code = 500;
                _baseResponse.Status = false;
                _baseResponse.Data = new { message = string.Join("; ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage)) };
                return StatusCode(500, _baseResponse);
            }
        }

        [HttpPost("AddPreviousWork")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<BaseResponse>> AddPreviousWork([FromForm] IList<IFormFile> files, [FromHeader] string lang = "en")
        {
            if (!ModelState.IsValid)
            {
                _baseResponse.Message = (lang == "ar") ? "خطأ في البيانات" : "Error in data";
                _baseResponse.Code = 400;
                _baseResponse.Status = false;
                _baseResponse.Data = new { message = string.Join("; ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage)) };
                return BadRequest(_baseResponse);
            }
            var userId = this.User.Claims.First(i => i.Type == "uid").Value; // will give the user's userId
            var user = await _unitOfWork.Users.FindByQuery(x => x.Id == userId && x.Status == true).FirstOrDefaultAsync();
            if (user == null)
            {
                _baseResponse.Message = (lang == "ar") ? "لم يتم العثور علي المستخدم" : "User is Exits";
                _baseResponse.Code = 400;
                _baseResponse.Status = false;
                _baseResponse.Data = new { };
                return StatusCode(400, _baseResponse);

            }
            IList<string> roleUser = _accountService.RoleUser(user);
            if (!roleUser.Contains("Provider"))
            {
                _baseResponse.Message = (lang == "ar") ? "المستخدم لا يملك صلاحيه" : "User is not Seeker";
                _baseResponse.Code = 401;
                _baseResponse.Status = false;
                _baseResponse.Data = new { files };
                return Unauthorized(_baseResponse);
            }
            IList<ImageModel> images=new List<ImageModel>();
            foreach(var file in files)
            {
                try
                {
                    Images image = await _fileHandling.UploadFile(file, "PreviousWork", userId);
                    images.Add(new ImageModel
                    {
                        IdImage = image.Id,
                        PathImage = await _fileHandling.PathFile(image)
                    }
                    ); ;
                }
                catch (Exception ex)
                {
                    _baseResponse.Message = ex.Message;
                    _baseResponse.Code = 500;
                    _baseResponse.Status = false;
                    _baseResponse.Data = new { message = string.Join("; ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage)) };
                    return StatusCode(500, _baseResponse);
                }
            }

            _baseResponse.Code = 200;
            _baseResponse.Status = true;
            _baseResponse.Message = (lang == "ar") ? "تم حفظ الصور" : "Add Images successfully";
            _baseResponse.Data = new
            {
                images
            };
            return Ok(_baseResponse);

        }

        [HttpGet("GetPreviousWork")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<BaseResponse>> GetPreviousWork([FromHeader] string lang = "en")
        {
            var userId = this.User.Claims.First(i => i.Type == "uid").Value; // will give the user's userId
            var user = await _unitOfWork.Users.FindByQuery(x => x.Id == userId && x.Status == true).FirstOrDefaultAsync();
            if (user == null)
            {
                _baseResponse.Message = (lang == "ar") ? "لم يتم العثور علي المستخدم" : "User is Exits";
                _baseResponse.Code = 400;
                _baseResponse.Status = false;
                _baseResponse.Data = new { };
                return StatusCode(400, _baseResponse);

            }
            IList<string> roleUser = _accountService.RoleUser(user);
            if (!roleUser.Contains("Provider"))
            {
                _baseResponse.Message = (lang == "ar") ? "المستخدم لا يملك صلاحيه" : "User is not Seeker";
                _baseResponse.Code = 401;
                _baseResponse.Status = false;
                _baseResponse.Data = new { };
                return Unauthorized(_baseResponse);
            }
            IEnumerable<Images> files = await _fileHandling.GetFile("PreviousWork", userId);
            IList<ImageModel> images = new List<ImageModel>();
            foreach (var file in files)
            {
                try
                {
                    images.Add(new ImageModel
                    {
                        IdImage = file.Id,
                        PathImage = await _fileHandling.PathFile(file)
                    });
                }
                catch (Exception ex)
                {
                    _baseResponse.Message = ex.Message;
                    _baseResponse.Code = 500;
                    _baseResponse.Status = false;
                    _baseResponse.Data = new { message = string.Join("; ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage)) };
                    return StatusCode(500, _baseResponse);
                }
            }
            _baseResponse.Status = true;
            _baseResponse.Code = 200;
            _baseResponse.Message = (lang == "ar") ? " معرض الصور الخاص بك" : "Get Images successfully";
            _baseResponse.Data = new
            {
                images
            };
            return Ok(_baseResponse);

        }

        //------------------------------------------------------------------------------------------------ Change Password Api

        [HttpPut("ChangePassword")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<BaseResponse>> ChangePassword([FromForm] ChangeOldPassword changeOldPassword, [FromHeader] string lang = "en")
        {
            if (!ModelState.IsValid)
            {
                _baseResponse.Message = (lang == "ar") ? "خطأ في البيانات" : "Error in data";
                _baseResponse.Code = 400;
                _baseResponse.Status = false;
                _baseResponse.Data = new { message = string.Join("; ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage)) };
                return BadRequest(_baseResponse);
            }
            var userId = this.User.Claims.First(i => i.Type == "uid").Value; // will give the user's userId
            var user = await _unitOfWork.Users.FindByQuery(x => x.Id == userId && x.Status == true).FirstOrDefaultAsync();
            if (user == null)
            {
                _baseResponse.Message = (lang == "ar") ? "لم يتم العثور علي المستخدم" : "User is Exits";
                _baseResponse.Code = 400;
                _baseResponse.Status = false;
                _baseResponse.Data = new { };
                return StatusCode(400, _baseResponse);

            }
            IList<string> roleUser = _accountService.RoleUser(user);
            if (roleUser.Contains("Admin"))
            {
                _baseResponse.Message = (lang == "ar") ? "المستخدم لا يملك صلاحيه" : "User is Unauthorized";
                _baseResponse.Code = 401;
                _baseResponse.Status = false;
                _baseResponse.Data = new { changeOldPassword };
                return Unauthorized(_baseResponse);
            }
            try
            {
                var result = await _accountService.ChangeOldPassword(userId, changeOldPassword);
                if (!result.IsAuthenticated)
                {
                    _baseResponse.Status = false;
                    _baseResponse.Code = result.ErrorCode;
                    _baseResponse.Message = (lang == "ar") ? result.ArMessage : result.Message;
                    _baseResponse.Data = new { changeOldPassword };
                    return BadRequest(_baseResponse);
                }

                _baseResponse.Code = 200;
                _baseResponse.Status = true;
                _baseResponse.Message = (lang == "ar") ? "تم تحديث كلمه السر" : "Update Password successfully";
                _baseResponse.Data = new
                {
                    result.UserId,
                    result.FullName,
                    Role = result.Roles,
                    result.PhoneNumber,
                };
                return Ok(_baseResponse);
            }
            catch (Exception ex)
            {
                _baseResponse.Message = ex.Message;
                _baseResponse.Code = 500;
                _baseResponse.Status = false;
                _baseResponse.Data = new { message = string.Join("; ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage)) };
                return StatusCode(500, _baseResponse);
            }
        }

        //-------------------------------------------------------------------------------------------- login Api 
        [HttpPost("login")]
        public async Task<ActionResult<BaseResponse>> LoginAsync([FromForm] LoginModel model, [FromHeader] string lang = "en")
        {
            if (!ModelState.IsValid)
            {
                _baseResponse.Status = false;
                _baseResponse.Message = (lang == "ar") ? "خطأ في البيانات" : "Error in data";
                _baseResponse.Code = 400;
                _baseResponse.Data = new { message = string.Join("; ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage)) };
                return BadRequest(_baseResponse);
            }
            var result = await _accountService.LoginAsync(model);

            if (!result.IsAuthenticated)
            {
                _baseResponse.Status = false;
                _baseResponse.Code = result.ErrorCode;
                _baseResponse.Message = (lang == "ar") ? result.ArMessage : result.Message;
                _baseResponse.Data = new { model};
                return BadRequest(_baseResponse);
            }
            _baseResponse.Status = true;
            _baseResponse.Code = 200;
            _baseResponse.Message = (lang == "ar") ? "تم تسجيل الدخول" : "Login Successfully";
            _baseResponse.Data = new
            {
                result.UserId,
                result.FullName,
                result.Token,
                Role = result.Roles,
                result.UserImgUrl,
                result.PhoneNumber,
                result.Description,
            };
            return Ok(_baseResponse);
        }

        //-------------------------------------------------------------------------------------------- logout Api 
        [HttpPost("logout")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<BaseResponse>> LogoutAsync([FromHeader] string lang = "en")
        {
            var userId = this.User.Claims.First(i => i.Type == "uid").Value; // will give the user's userId
            if (!string.IsNullOrEmpty(userId))
            {
                var result = await _accountService.Logout(userName: _unitOfWork.Users.FindByQuery(s => s.Id == userId).FirstOrDefaultAsync().Result.UserName);
                if (result)
                {
                    _baseResponse.Status = true;
                    _baseResponse.Code = 200;
                    _baseResponse.Message = (lang == "ar") ? "تم تسجيل الخروج بنجاح " : "Signed out successfully";
                    return Ok(_baseResponse);
                }
            }
            _baseResponse.Code = 404;
            _baseResponse.Status = false;
            _baseResponse.Message = (lang == "ar") ? "هذا الحساب غير موجود " : "The User Not Exist";
            return NotFound(_baseResponse);
        }

        //----------------------------------------------------------------------------------------------------- get profile
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("GetUserInfo")]
        public async Task<ActionResult<BaseResponse>> GetUserInfo([FromHeader] string lang="en")
        {
            var userId = this.User.Claims.First(i => i.Type == "uid").Value; // will give the user's userId
            if (string.IsNullOrEmpty(userId))
            {
                _baseResponse.Code = 400;
                _baseResponse.Status = false;
                _baseResponse.Message = (lang == "ar") ? "المستخدم غير موجود" : "User not exist";
                _baseResponse.Data = null;
                return Ok(_baseResponse);
            }
            var result = await _accountService.GetUserInfo(userId);

            if (!result.IsAuthenticated)
            {
                _baseResponse.Message = (lang == "ar") ? result.ArMessage : result.Message;
                _baseResponse.Code = result.ErrorCode;
                _baseResponse.Data = result;
                return BadRequest(_baseResponse);
            }
            _baseResponse.Status = true;
            _baseResponse.Code = 200;
            _baseResponse.Message = (lang == "ar") ? result.ArMessage : result.Message;
            _baseResponse.Data = new
            {
                result.FullName,
                result.Description,
                result.PhoneNumber,
                Role = result.Roles,
                result.bankAccountNumber,
                result.Profession,
                result.Token,
                result.UserImgUrl,
                result.Status,
            };
            return Ok(_baseResponse);
        }

        //---------------------------------------------------------------------------------------------------- Delete Account
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpDelete("DeleteUser")]
        public async Task<ActionResult<BaseResponse>> DeleteUser([FromHeader] string lang = "en")
        {
            var userId = this.User.Claims.First(i => i.Type == "uid").Value; // will give the user's userId
            if (string.IsNullOrEmpty(userId))
            {
                _baseResponse.Code = 400;
                _baseResponse.Status = false;
                _baseResponse.Message = (lang == "ar") ? "المستخدم غير موجود" : "User not exist";
                _baseResponse.Data = null;
                return Ok(_baseResponse);
            }
            var result = await _accountService.DeleteAccount(userId);

            if (!result.IsAuthenticated)
            {
                _baseResponse.Message = (lang == "ar") ? result.ArMessage : result.Message;
                _baseResponse.Code = result.ErrorCode;
                _baseResponse.Data = result;
                return StatusCode(result.ErrorCode,_baseResponse);
            }
            _baseResponse.Status = true;
            _baseResponse.Code = 200;
            _baseResponse.Message = (lang == "ar") ? result.ArMessage : result.Message;
            _baseResponse.Data = new
            {
                result.FullName,
                result.PhoneNumber,
                result.UserImgUrl,
                result.Status,
            };
            return Ok(_baseResponse);
        }

    }
}
