using AutoMapper;
using Maintainify.BusinessLayer.Interface;
using Maintainify.BusinessLayer.Services;
using Maintainify.Core.DTO;
using Maintainify.Core.Entity.ApplicationData;
using Maintainify.Core.Entity.OrderData;
using Maintainify.Core.Entity.ProfessionData;
using Maintainify.Core.Helpers;
using Maintainify.Core.ModelView.AuthViewModels;
using Maintainify.Core.ModelView.OrderViewModels;
using Maintainify.RepositoryLayer.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Net.Http.Headers;
using System.Data;

namespace Maintainify.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class OrderSeekerController : BaseController, IActionFilter
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileHandling _fileHandling;
        private readonly BaseResponse _baseResponse;
        private ApplicationUser _user;
        private readonly IMapper _mapper;
        private IList<string> roleUser;
        private readonly IAccountService _accountService;

        public OrderSeekerController(IUnitOfWork unitOfWork, IFileHandling fileHandling, IAccountService accountService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _fileHandling = fileHandling;
            _baseResponse = new BaseResponse();
            _accountService = accountService;
            _mapper = mapper;
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
            {
                var _order = _unitOfWork.Orders.GetAllAsync().Result.ToList();
                var thresholdDate = DateTime.Now.AddDays(-2);
                _order.RemoveAll(order => order.OrderDate > thresholdDate);
                foreach (var o in _order)
                {
                    o.DeletedAt = DateTime.Now;
                    o.IsDeleted = true;
                    _unitOfWork.Orders.Update(o);
                    _unitOfWork.SaveChangesAsync();
                }
            }

            if (user != null)
            {
                roleUser = _accountService.RoleUser(user);
            }

        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
        //---------------------------------------------------------------------------------------------------
        [HttpGet("AllProvider")]
        public async Task<ActionResult<BaseResponse>> GetProvider([FromHeader] string lang = "en")
        {
            if (!roleUser.Contains("Seeker"))
            {
                _baseResponse.Message = (lang == "ar") ? "المستخدم لا يملك صلاحيه" : "User is not Seeker";
                _baseResponse.Code = 401;
                _baseResponse.Status = false;
                _baseResponse.Data = new { };
                return Unauthorized(_baseResponse);
            }
            try
            {
                var Providers = _accountService.GetUsersRoles("Provider");
                List<dynamic> result = new List<dynamic> { };
                foreach (var provider in Providers)
                {
                    List<ImageModel> images = new List<ImageModel>();
                    PathFiles pathFiles = await _unitOfWork.pathFiles.FindByQuery(x => x.type == "PreviousWork".ToLower()).FirstAsync();
                    foreach (var img in _unitOfWork.images.FindByQuery(s => s.UserId == provider.Id && s.PathId == pathFiles.Id))
                    {
                        images.Add(new ImageModel
                        {
                            IdImage = img.Id,
                            PathImage = await _fileHandling.PathFile(img)
                        });
                    }

                    var user = await _accountService.GetUserInfo(provider.Id);
                    result.Add(new
                    {
                        UserId = user.UserId,
                        Description = user.Description,
                        FullName = user.FullName,
                        UserImgUrl = user.UserImgUrl,
                        PhoneNumber = user.PhoneNumber,
                        Profession = user.Profession,
                        PreviousWork = images,
                    });
                }
                _baseResponse.Message = (lang == "ar") ? "مقدمي الخدمات" : "All Providers";
                _baseResponse.Status = true;
                _baseResponse.Code = 200;
                _baseResponse.Data = result;
                return StatusCode(_baseResponse.Code, _baseResponse);
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

        //---------------------------------------------------------------------------------------------------

        [HttpPost("AddOrder")]
        public async Task<ActionResult<BaseResponse>> AddOrder([FromForm] OrderModel orderModel, [FromHeader] string lang = "en")
        {
            if (!ModelState.IsValid)
            {
                _baseResponse.Status = false;
                _baseResponse.Message = (lang == "ar") ? "خطأ في البيانات" : "Error in data";
                _baseResponse.Code = 400;
                _baseResponse.Data = new { message = string.Join("; ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage)) };
                return BadRequest(_baseResponse);
            }
            if (!roleUser.Contains("Seeker"))
            {
                _baseResponse.Message = (lang == "ar") ? "المستخدم لا يملك صلاحيه" : "User is not Seeker";
                _baseResponse.Code = 401;
                _baseResponse.Status = false;
                _baseResponse.Data = new { orderModel };
                return Unauthorized(_baseResponse);
            }
            try
            {
                orderModel.SeekerId = _user.Id;
                var order = _mapper.Map<Order>(orderModel);
                await _unitOfWork.Orders.AddAsync(order);
                await _unitOfWork.SaveChangesAsync();
                _baseResponse.Message = (lang == "ar") ? "تم طلب الخدمه" : "Create Order succsefully";
                _baseResponse.Status = true;
                _baseResponse.Code = 200;
                _baseResponse.Data = new
                {
                    order.Id,
                    orderModel.Status,
                    order.OrderDate,
                    order.CreatedAt,
                    order.UpdatedAt,
                    order.DeletedAt,
                    order.IsDeleted,
                    order.IsUpdated,
                    order.ProviderId,
                };
                return StatusCode(_baseResponse.Code, _baseResponse);

            }
            catch (Exception ex)
            {
                _baseResponse.Message = "Not Can Save Order";
                _baseResponse.Code = 500;
                _baseResponse.Status = false;
                _baseResponse.Data = new { message = string.Join("; ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage)) };
                return StatusCode(500, _baseResponse);
            }
        }

        //---------------------------------------------------------------------------------------------------

        [HttpGet("AllOrder")]
        public async Task<ActionResult<BaseResponse>> AllOrder([FromHeader] string lang = "en")
        {
            if (!ModelState.IsValid)
            {
                _baseResponse.Status = false;
                _baseResponse.Message = (lang == "ar") ? "خطأ في البيانات" : "Error in data";
                _baseResponse.Code = 400;
                _baseResponse.Data = new { message = string.Join("; ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage)) };
                return BadRequest(_baseResponse);
            }
            if (!roleUser.Contains("Seeker"))
            {
                _baseResponse.Message = (lang == "ar") ? "المستخدم لا يملك صلاحيه" : "User is not Seeker";
                _baseResponse.Code = 401;
                _baseResponse.Status = false;
                _baseResponse.Data = new { };
                return Unauthorized(_baseResponse);
            }
            var Orders = await _unitOfWork.Orders.FindByQuery(s => s.SeekerId == _user.Id && !s.IsDeleted)
                .ToListAsync();
            ICollection<OrderModel> orderModels = new List<OrderModel>();
            foreach (var o in Orders)
            {
                orderModels.Add(_mapper.Map<OrderModel>(o));
            }
            if (!Orders.Any())
            {
                _baseResponse.Message = (lang == "ar") ? "فشل جلب البيانات" : "Get Orders Failed";
                _baseResponse.Code = 500;
                _baseResponse.Status = false;
                _baseResponse.Data = new { message = string.Join("; ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage)) };
                return StatusCode(500, _baseResponse);
            }
            _baseResponse.Message = (lang == "ar") ? "تم جلب الخدمه" : "Get Orders succsefully";
            _baseResponse.Status = true;
            _baseResponse.Code = 200;
            _baseResponse.Data = new
            {
                orderModels
            };
            return StatusCode(_baseResponse.Code, _baseResponse);

        }

        [HttpGet("AllOrderPreparing")]
        public async Task<ActionResult<BaseResponse>> AllOrderPreparing([FromHeader] string lang = "en")
        {
            if (!ModelState.IsValid)
            {
                _baseResponse.Status = false;
                _baseResponse.Message = (lang == "ar") ? "خطأ في البيانات" : "Error in data";
                _baseResponse.Code = 400;
                _baseResponse.Data = new { message = string.Join("; ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage)) };
                return BadRequest(_baseResponse);
            }
            if (!roleUser.Contains("Seeker"))
            {
                _baseResponse.Message = (lang == "ar") ? "المستخدم لا يملك صلاحيه" : "User is not Seeker";
                _baseResponse.Code = 401;
                _baseResponse.Status = false;
                _baseResponse.Data = new { };
                return Unauthorized(_baseResponse);
            }
            try
            {
                var Orders = await _unitOfWork.Orders.FindByQuery(s => s.SeekerId == _user.Id && !s.IsDeleted && s.OrderStatus == OrderStatus.Preparing)
                    .Select(s => new
                    {
                        s.Id,
                        s.OrderStatus,
                        s.OrderDate,
                        s.ProviderId,
                        s.IsDeleted,
                        s.CreatedAt,
                        s.UpdatedAt,
                        s.DeletedAt,
                    })
                    .ToListAsync();
                _baseResponse.Message = (lang == "ar") ? "تم جلب الخدمه" : "Get Orders succsefully";
                _baseResponse.Status = true;
                _baseResponse.Code = 200;
                _baseResponse.Data = new
                {
                    Orders
                };
                return StatusCode(_baseResponse.Code, _baseResponse);
            }
            catch (Exception ex)
            {
                _baseResponse.Message = (lang == "ar") ? "فشل جلب البيانات" : "Get Orders Failed";
                _baseResponse.Code = 500;
                _baseResponse.Status = false;
                _baseResponse.Data = new { message = string.Join("; ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage)) };
                return StatusCode(500, _baseResponse);
            }

        }

        [HttpGet("AllOrderConfirmed")]
        public async Task<ActionResult<BaseResponse>> AllOrderConfirmed([FromHeader] string lang = "en")
        {
            if (!ModelState.IsValid)
            {
                _baseResponse.Status = false;
                _baseResponse.Message = (lang == "ar") ? "خطأ في البيانات" : "Error in data";
                _baseResponse.Code = 400;
                _baseResponse.Data = new { message = string.Join("; ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage)) };
                return BadRequest(_baseResponse);
            }
            if (!roleUser.Contains("Seeker"))
            {
                _baseResponse.Message = (lang == "ar") ? "المستخدم لا يملك صلاحيه" : "User is not Seeker";
                _baseResponse.Code = 401;
                _baseResponse.Status = false;
                _baseResponse.Data = new { };
                return Unauthorized(_baseResponse);
            }
            try
            {
                var Orders = await _unitOfWork.Orders.FindByQuery(s => s.SeekerId == _user.Id && !s.IsDeleted && s.OrderStatus == OrderStatus.Confirmed)
                    .Select(s => new
                    {
                        s.Id,
                        s.OrderStatus,
                        s.OrderDate,
                        s.ProviderId,
                        s.IsDeleted,
                        s.CreatedAt,
                        s.UpdatedAt,
                        s.DeletedAt,
                    })
                    .ToListAsync();
                _baseResponse.Message = (lang == "ar") ? "تم جلب الخدمه" : "Get Orders succsefully";
                _baseResponse.Status = true;
                _baseResponse.Code = 200;
                _baseResponse.Data = new
                {
                    Orders
                };
                return StatusCode(_baseResponse.Code, _baseResponse);
            }
            catch (Exception ex)
            {
                _baseResponse.Message = (lang == "ar") ? "فشل جلب البيانات" : "Get Orders Failed";
                _baseResponse.Code = 500;
                _baseResponse.Status = false;
                _baseResponse.Data = new { message = string.Join("; ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage)) };
                return StatusCode(500, _baseResponse);
            }

        }

        [HttpGet("AllOrderWithDriver")]
        public async Task<ActionResult<BaseResponse>> AllOrderWithDriver([FromHeader] string lang = "en")
        {
            if (!ModelState.IsValid)
            {
                _baseResponse.Status = false;
                _baseResponse.Message = (lang == "ar") ? "خطأ في البيانات" : "Error in data";
                _baseResponse.Code = 400;
                _baseResponse.Data = new { message = string.Join("; ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage)) };
                return BadRequest(_baseResponse);
            }
            if (!roleUser.Contains("Seeker"))
            {
                _baseResponse.Message = (lang == "ar") ? "المستخدم لا يملك صلاحيه" : "User is not Seeker";
                _baseResponse.Code = 401;
                _baseResponse.Status = false;
                _baseResponse.Data = new { };
                return Unauthorized(_baseResponse);
            }
            try
            {
                var Orders = await _unitOfWork.Orders.FindByQuery(s => s.SeekerId == _user.Id && !s.IsDeleted && s.OrderStatus == OrderStatus.WithDriver)
                    .Select(s => new
                    {
                        s.Id,
                        s.OrderStatus,
                        s.OrderDate,
                        s.ProviderId,
                        s.IsDeleted,
                        s.CreatedAt,
                        s.UpdatedAt,
                        s.DeletedAt,
                    })
                    .ToListAsync();
                _baseResponse.Message = (lang == "ar") ? "تم جلب الخدمه" : "Get Orders succsefully";
                _baseResponse.Status = true;
                _baseResponse.Code = 200;
                _baseResponse.Data = new
                {
                    Orders
                };
                return StatusCode(_baseResponse.Code, _baseResponse);
            }
            catch (Exception ex)
            {
                _baseResponse.Message = (lang == "ar") ? "فشل جلب البيانات" : "Get Orders Failed";
                _baseResponse.Code = 500;
                _baseResponse.Status = false;
                _baseResponse.Data = new { message = string.Join("; ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage)) };
                return StatusCode(500, _baseResponse);
            }


        }

        [HttpGet("AllOrderFinished")]
        public async Task<ActionResult<BaseResponse>> AllOrderFinished([FromHeader] string lang = "en")
        {
            if (!ModelState.IsValid)
            {
                _baseResponse.Status = false;
                _baseResponse.Message = (lang == "ar") ? "خطأ في البيانات" : "Error in data";
                _baseResponse.Code = 400;
                _baseResponse.Data = new { message = string.Join("; ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage)) };
                return BadRequest(_baseResponse);
            }
            if (!roleUser.Contains("Seeker"))
            {
                _baseResponse.Message = (lang == "ar") ? "المستخدم لا يملك صلاحيه" : "User is not Seeker";
                _baseResponse.Code = 401;
                _baseResponse.Status = false;
                _baseResponse.Data = new { };
                return Unauthorized(_baseResponse);
            }
            try
            {
                var Orders = await _unitOfWork.Orders.FindByQuery(s => s.SeekerId == _user.Id && !s.IsDeleted && s.OrderStatus == OrderStatus.Finished)
                    .Select(s => new
                    {
                        s.Id,
                        s.OrderStatus,
                        s.OrderDate,
                        s.ProviderId,
                        s.IsDeleted,
                        s.CreatedAt,
                        s.UpdatedAt,
                        s.DeletedAt,
                    })
                    .ToListAsync();
                _baseResponse.Message = (lang == "ar") ? "تم جلب الخدمه" : "Get Orders succsefully";
                _baseResponse.Status = true;
                _baseResponse.Code = 200;
                _baseResponse.Data = new
                {
                    Orders
                };
                return StatusCode(_baseResponse.Code, _baseResponse);
            }
            catch (Exception ex)
            {
                _baseResponse.Message = (lang == "ar") ? "فشل جلب البيانات" : "Get Orders Failed";
                _baseResponse.Code = 500;
                _baseResponse.Status = false;
                _baseResponse.Data = new { message = string.Join("; ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage)) };
                return StatusCode(500, _baseResponse);
            }

        }

        [HttpGet("AllOrderCancelled")]
        public async Task<ActionResult<BaseResponse>> AllOrderCancelled([FromHeader] string lang = "en")
        {
            if (!ModelState.IsValid)
            {
                _baseResponse.Status = false;
                _baseResponse.Message = (lang == "ar") ? "خطأ في البيانات" : "Error in data";
                _baseResponse.Code = 400;
                _baseResponse.Data = new { message = string.Join("; ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage)) };
                return BadRequest(_baseResponse);
            }
            if (!roleUser.Contains("Seeker"))
            {
                _baseResponse.Message = (lang == "ar") ? "المستخدم لا يملك صلاحيه" : "User is not Seeker";
                _baseResponse.Code = 401;
                _baseResponse.Status = false;
                _baseResponse.Data = new { };
                return Unauthorized(_baseResponse);
            }
            try
            {
                var Orders = await _unitOfWork.Orders.FindByQuery(s => s.SeekerId == _user.Id && !s.IsDeleted && s.OrderStatus == OrderStatus.Cancelled)
                    .Select(s => new
                    {
                        s.Id,
                        s.OrderStatus,
                        s.OrderDate,
                        s.ProviderId,
                        s.IsDeleted,
                        s.CreatedAt,
                        s.UpdatedAt,
                        s.DeletedAt,
                    })
                    .ToListAsync();
                _baseResponse.Message = (lang == "ar") ? "تم جلب الخدمه" : "Get Orders succsefully";
                _baseResponse.Status = true;
                _baseResponse.Code = 200;
                _baseResponse.Data = new
                {
                    Orders
                };
                return StatusCode(_baseResponse.Code, _baseResponse);
            }
            catch (Exception ex)
            {
                _baseResponse.Message = (lang == "ar") ? "فشل جلب البيانات" : "Get Orders Failed";
                _baseResponse.Code = 500;
                _baseResponse.Status = false;
                _baseResponse.Data = new { message = string.Join("; ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage)) };
                return StatusCode(500, _baseResponse);
            }


        }

        //---------------------------------------------------------------------------------------------------

        [HttpPut("CancelledOrder")]
        public async Task<ActionResult<BaseResponse>> CancelledOrder([FromForm] string orderId, [FromHeader] string lang = "en")
        {
            if (!ModelState.IsValid)
            {
                _baseResponse.Status = false;
                _baseResponse.Message = (lang == "ar") ? "خطأ في البيانات" : "Error in data";
                _baseResponse.Code = 400;
                _baseResponse.Data = new { message = string.Join("; ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage)) };
                return BadRequest(_baseResponse);
            }
            if (!roleUser.Contains("Seeker"))
            {
                _baseResponse.Message = (lang == "ar") ? "المستخدم لا يملك صلاحيه" : "User is not Seeker";
                _baseResponse.Code = 401;
                _baseResponse.Status = false;
                _baseResponse.Data = new { orderId };
                return Unauthorized(_baseResponse);
            }
            try
            {
                Order order = await _unitOfWork.Orders.FindByQuery(s => s.Id == orderId && !s.IsDeleted &&s.OrderStatus != OrderStatus.Finished).FirstAsync();
                try
                {
                    order.OrderStatus = OrderStatus.Cancelled;
                    _unitOfWork.Orders.Update(order);
                    await _unitOfWork.SaveChangesAsync();
                    _baseResponse.Message = (lang == "ar") ? "تم الغاء الخدمه" : "Cancelled Order succsefully";
                    _baseResponse.Status = true;
                    _baseResponse.Code = 200;
                    var Order = _mapper.Map<OrderModel>(order);
                    _baseResponse.Data = new
                    {
                        Order
                    };
                    return StatusCode(_baseResponse.Code, _baseResponse);

                }
                catch (Exception ex)
                {
                    _baseResponse.Message = "Not Can Cancelled Order";
                    _baseResponse.Code = 500;
                    _baseResponse.Status = false;
                    _baseResponse.Data = new { message = string.Join("; ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage)) };
                    return StatusCode(500, _baseResponse);
                }

            }
            catch (Exception ex)
            {
                _baseResponse.Message = "Not Found Order";
                _baseResponse.Code = 400;
                _baseResponse.Status = false;
                _baseResponse.Data = new { message = string.Join("; ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage)) };
                return StatusCode(400, _baseResponse);
            }
        }

    }
}
