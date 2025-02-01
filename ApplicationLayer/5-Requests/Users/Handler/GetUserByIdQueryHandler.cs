using ApplicationLayer.Requests.Users.Query;
using ApplicationLayer.Common;
using ApplicationLayer.Common.ViewModels;
using ApplicationLayer.Extensions;
using ApplicationLayer.Extensions.ServiceMessages;
using ApplicationLayer.Extensions.SmartEnums;
using DomainLayer.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ApplicationLayer.Requests.Users.Handler
{
    public class GetUserByIdQueryHandler(IHttpContextAccessor accessor, IRepository<UserAccount> userAccountRepository, ILogger<GetUserByIdQueryHandler> logger) : IRequestHandler<GetUserByIdQuery, HandlerResult>
    {
        private readonly IHttpContextAccessor _accessor = accessor;
        private readonly IRepository<UserAccount> _userAccountRepository = userAccountRepository;
        private readonly ILogger<GetUserByIdQueryHandler> _logger = logger;

        public async Task<HandlerResult> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var currentUserId = _accessor.HttpContext.User.Identity.GetUserId();

                var user = await _userAccountRepository.Query()
                    .Include(current => current.UserProfiles)
                    .Select(current => new UserAccountViewModel
                    {
                        Id = current.Id,
                        FirstName = current.UserProfiles.FirstOrDefault().FirstName,
                        LastName = current.UserProfiles.FirstOrDefault().LastName,
                        UserName = current.UserName,
                        Mobile = current.UserProfiles.FirstOrDefault().Mobile,
                        NationalCode = current.UserProfiles.FirstOrDefault().NationalCode,
                    }).FirstOrDefaultAsync(current => current.Id.Equals(currentUserId));

                if (user is null)
                    return new HandlerResult { RequestStatus = RequestStatus.NotFound, Message = CommonMessages.NotFound };

                return new HandlerResult { RequestStatus = RequestStatus.Successful, ObjectResult = user, Message = CommonMessages.Successful };
            }
            catch (Exception exception)
            {
                _logger.Log(LogLevel.Error, exception);
                return new HandlerResult { RequestStatus = RequestStatus.Failed, Message = CommonMessages.Failed };
            }
        }
    }
}