using AutoMapper;
using Maintainify.BusinessLayer.Interface;
using Maintainify.Core.DTO;
using Maintainify.Core.Entity.ApplicationData;
using Maintainify.Core.Entity.OrderData;
using Maintainify.Core.Helpers;
using Maintainify.Core.ModelView.OrderViewModels;
using Maintainify.RepositoryLayer.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;

namespace Maintainify.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class OrderProviderController : BaseController, IActionFilter
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileHandling _fileHandling;
        private readonly BaseResponse _baseResponse;
        private ApplicationUser _user;
        private readonly IMapper _mapper;
        private IList<string> roleUser;
        private readonly IAccountService _accountService;

        public OrderProviderController(IUnitOfWork unitOfWork, IFileHandling fileHandling, IAccountService accountService, IMapper mapper)
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
            if (!roleUser.Contains("Provider"))
            {
                _baseResponse.Message = (lang == "ar") ? "المستخدم لا يملك صلاحيه" : "User is not Seeker";
                _baseResponse.Code = 401;
                _baseResponse.Status = false;
                _baseResponse.Data = new { };
                return Unauthorized(_baseResponse);
            }
            var Orders = await _unitOfWork.Orders.FindByQuery(s => s.ProviderId == _user.Id && !s.IsDeleted)
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
            if (!roleUser.Contains("Provider"))
            {
                _baseResponse.Message = (lang == "ar") ? "المستخدم لا يملك صلاحيه" : "User is not Seeker";
                _baseResponse.Code = 401;
                _baseResponse.Status = false;
                _baseResponse.Data = new { };
                return Unauthorized(_baseResponse);
            }
            try
            {
                var Orders = await _unitOfWork.Orders.FindByQuery(s => s.ProviderId == _user.Id && !s.IsDeleted && s.OrderStatus == OrderStatus.Preparing)
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
            if (!roleUser.Contains("Provider"))
            {
                _baseResponse.Message = (lang == "ar") ? "المستخدم لا يملك صلاحيه" : "User is not Seeker";
                _baseResponse.Code = 401;
                _baseResponse.Status = false;
                _baseResponse.Data = new { };
                return Unauthorized(_baseResponse);
            }
            try
            {
                var Orders = await _unitOfWork.Orders.FindByQuery(s => s.ProviderId == _user.Id && !s.IsDeleted && s.OrderStatus == OrderStatus.Confirmed)
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
            if (!roleUser.Contains("Provider"))
            {
                _baseResponse.Message = (lang == "ar") ? "المستخدم لا يملك صلاحيه" : "User is not Seeker";
                _baseResponse.Code = 401;
                _baseResponse.Status = false;
                _baseResponse.Data = new { };
                return Unauthorized(_baseResponse);
            }
            try
            {
                var Orders = await _unitOfWork.Orders.FindByQuery(s => s.ProviderId == _user.Id && !s.IsDeleted && s.OrderStatus == OrderStatus.WithDriver)
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
            if (!roleUser.Contains("Provider"))
            {
                _baseResponse.Message = (lang == "ar") ? "المستخدم لا يملك صلاحيه" : "User is not Seeker";
                _baseResponse.Code = 401;
                _baseResponse.Status = false;
                _baseResponse.Data = new { };
                return Unauthorized(_baseResponse);
            }
            try
            {
                var Orders = await _unitOfWork.Orders.FindByQuery(s => s.ProviderId == _user.Id && !s.IsDeleted && s.OrderStatus == OrderStatus.Finished)
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
            if (!roleUser.Contains("Provider"))
            {
                _baseResponse.Message = (lang == "ar") ? "المستخدم لا يملك صلاحيه" : "User is not Seeker";
                _baseResponse.Code = 401;
                _baseResponse.Status = false;
                _baseResponse.Data = new { };
                return Unauthorized(_baseResponse);
            }
            try
            {
                var Orders = await _unitOfWork.Orders.FindByQuery(s => s.ProviderId == _user.Id && !s.IsDeleted && s.OrderStatus == OrderStatus.Cancelled)
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
            if (!roleUser.Contains("Provider"))
            {
                _baseResponse.Message = (lang == "ar") ? "المستخدم لا يملك صلاحيه" : "User is not Seeker";
                _baseResponse.Code = 401;
                _baseResponse.Status = false;
                _baseResponse.Data = new { orderId };
                return Unauthorized(_baseResponse);
            }
            try
            {
                Order order = await _unitOfWork.Orders.FindByQuery(s => s.Id == orderId && !s.IsDeleted && s.OrderStatus != OrderStatus.Finished).FirstAsync();
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

        [HttpPut("ChangeOrderStatus")]
        public async Task<ActionResult<BaseResponse>> ChangeOrderStatus([FromForm] string orderId, [FromHeader] string lang = "en")
        {
            if (!ModelState.IsValid)
            {
                _baseResponse.Status = false;
                _baseResponse.Message = (lang == "ar") ? "خطأ في البيانات" : "Error in data";
                _baseResponse.Code = 400;
                _baseResponse.Data = new { message = string.Join("; ", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage)) };
                return BadRequest(_baseResponse);
            }
            if (!roleUser.Contains("Provider"))
            {
                _baseResponse.Message = (lang == "ar") ? "المستخدم لا يملك صلاحيه" : "User is not Seeker";
                _baseResponse.Code = 401;
                _baseResponse.Status = false;
                _baseResponse.Data = new { orderId };
                return Unauthorized(_baseResponse);
            }
            try
            {
                Order order = await _unitOfWork.Orders.FindByQuery(s => s.Id == orderId && !s.IsDeleted && s.OrderStatus != OrderStatus.Finished).FirstAsync();
                try
                {
                    switch(order.OrderStatus)
                    {
                        case OrderStatus.Preparing:
                            {
                                order.OrderStatus = OrderStatus.Confirmed;
                                _unitOfWork.Orders.Update(order);
                                await _unitOfWork.SaveChangesAsync();
                                _baseResponse.Message = (lang == "ar") ? "تم تاكيد الخدمه" : "Confirmed Order succsefully";
                                _baseResponse.Status = true;
                                _baseResponse.Code = 200;
                                var Order = _mapper.Map<OrderModel>(order);
                                _baseResponse.Data = new
                                {
                                    Order
                                };
                                break;
                            }
                        case OrderStatus.Confirmed:
                            {
                                order.OrderStatus = OrderStatus.WithDriver;
                                _unitOfWork.Orders.Update(order);
                                await _unitOfWork.SaveChangesAsync();
                                _baseResponse.Message = (lang == "ar") ? " مقدم الخدمه في الطريق" : "Provider come now";
                                _baseResponse.Status = true;
                                _baseResponse.Code = 200;
                                var Order = _mapper.Map<OrderModel>(order);
                                _baseResponse.Data = new
                                {
                                    Order
                                };
                                break;
                            }
                        case OrderStatus.WithDriver:
                            {
                                order.OrderStatus = OrderStatus.Finished;
                                _unitOfWork.Orders.Update(order);
                                await _unitOfWork.SaveChangesAsync();
                                _baseResponse.Message = (lang == "ar") ? "تم انتهاء الخدمه" : "Finished Order succsefully";
                                _baseResponse.Status = true;
                                _baseResponse.Code = 200;
                                var Order = _mapper.Map<OrderModel>(order);
                                _baseResponse.Data = new
                                {
                                    Order
                                };

                                break;
                            }

                    }
                    return StatusCode(_baseResponse.Code, _baseResponse);

                }
                catch (Exception ex)
                {
                    _baseResponse.Message = "Not Can Change Status Order";
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
