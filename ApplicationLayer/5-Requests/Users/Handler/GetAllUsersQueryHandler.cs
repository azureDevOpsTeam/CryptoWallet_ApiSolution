using ApplicationLayer.Requests.Users.Query;
using ApplicationLayer.Common;
using ApplicationLayer.Common.ViewModels;
using ApplicationLayer.Extensions;
using ApplicationLayer.Extensions.ServiceMessages;
using ApplicationLayer.Extensions.SmartEnums;
using DomainLayer.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ApplicationLayer.Requests.Users.Handler
{
    public class GetAllUsersQueryHandler(IRepository<UserAccount> userAccountRepository, ILogger<UserDropdownQueryHandler> logger) : IRequestHandler<GetAllUsersQuery, HandlerResult>
    {
        private readonly IRepository<UserAccount> _userAccountRepository = userAccountRepository;
        private readonly ILogger<UserDropdownQueryHandler> _logger = logger;

        public async Task<HandlerResult> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _userAccountRepository.Query()
                    .Include(current => current.UserProfiles)
                    .Select(current => new UserAccountViewModel
                    {
                        Id = current.Id,
                        UserName = current.UserName,
                        Email = current.Email,
                        Mobile = current.UserProfiles.FirstOrDefault().Mobile,
                        Address = current.UserProfiles.FirstOrDefault().Address,
                        Company = current.UserProfiles.FirstOrDefault().Company,
                        FirstName = current.UserProfiles.FirstOrDefault().FirstName,
                        LastName = current.UserProfiles.FirstOrDefault().LastName,
                        NationalCode = current.UserProfiles.FirstOrDefault().NationalCode,
                        LocalPhoneNumber = current.UserProfiles.FirstOrDefault().LocalPhoneNumber,
                        PhoneNumber = current.PhoneNumber
                    }).ToListAsync(cancellationToken);

                return new HandlerResult { RequestStatus = RequestStatus.Successful, ObjectResult = response, Message = CommonMessages.Successful };
            }
            catch (Exception exception)
            {
                _logger.Log(LogLevel.Error, exception);
                return new HandlerResult { RequestStatus = RequestStatus.Failed, Message = CommonMessages.Failed };
            }
        }
    }
}