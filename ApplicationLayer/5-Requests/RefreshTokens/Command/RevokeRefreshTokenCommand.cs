using ApplicationLayer.Common;
using MediatR;

namespace ApplicationLayer.Requests.RefreshTokens.Command
{
    public class RevokeRefreshTokenCommand : IRequest<HandlerResult>
    {
        public int UserId { get; set; }
    }
}