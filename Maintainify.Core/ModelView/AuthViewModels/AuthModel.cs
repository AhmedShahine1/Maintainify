using Maintainify.Core.Entity.ProfessionData;

namespace Maintainify.Core.ModelView.AuthViewModels
{
    public class AuthModel
    {
        public bool IsAuthenticated { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public List<string> Roles { get; set; }
        public string Token { get; set; }
        public string FullName { get; set; }
        public Profession Profession { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
        public string ArMessage { get; set; }
        public int ErrorCode { get; set; }
        public string PhoneNumber { get; set; }
        public float? Lat { get; set; }
        public float? Lng { get; set; }
        public List<string> UserImgUrl { get; set; }
        public string Description { get; set; }
        public string bankAccountNumber { get; set; }
    }
}
