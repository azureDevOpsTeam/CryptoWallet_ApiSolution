using ApplicationLayer.Common;
using MediatR;

namespace ApplicationLayer.Requests.Users.Query
{
    public class GetUserByIdQuery : IRequest<HandlerResult>
    {
    }
}