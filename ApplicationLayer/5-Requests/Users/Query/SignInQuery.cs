using ApplicationLayer.Common;
using MediatR;

namespace ApplicationLayer.Requests.Users.Query
{
    public class SignInQuery : IRequest<HandlerResult>
    {
        public string UserName { get; set; }

        public string Password { get; set; }
    }
}