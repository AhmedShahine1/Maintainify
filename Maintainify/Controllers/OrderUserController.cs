using Maintainify.BusinessLayer.Interface;
using Maintainify.Core.DTO;
using Maintainify.Core.Entity.ApplicationData;
using Maintainify.RepositoryLayer.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;

namespace Maintainify.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class OrderUserController : BaseController, IActionFilter
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileHandling _fileHandling;
        private readonly BaseResponse _baseResponse;
        private ApplicationUser _user;

        public OrderUserController( IUnitOfWork unitOfWork, IFileHandling fileHandling)
        {
            _unitOfWork = unitOfWork;
            _fileHandling = fileHandling;
            _baseResponse = new BaseResponse();
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var accessToken = Request.Headers[HeaderNames.Authorization];
            if (string.IsNullOrEmpty(accessToken))
                return;

            var userId = User.Claims.First(i => i.Type == "uid").Value; // will give the user's userId
            var user = _unitOfWork.Users.FindByQuery(s => s.Id == userId)
                .FirstOrDefault();
            _user = user;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
        //---------------------------------------------------------------------------------------------------
    }
}
