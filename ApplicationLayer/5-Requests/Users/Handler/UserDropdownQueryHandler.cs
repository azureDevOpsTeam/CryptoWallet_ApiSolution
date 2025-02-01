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
    public class UserDropdownQueryHandler(IRepository<UserAccount> userAccountRepository, ILogger<UserDropdownQueryHandler> logger) : IRequestHandler<UserDropdownQuery, HandlerResult>
    {
        private readonly IRepository<UserAccount> _userAccountRepository = userAccountRepository;
        private readonly ILogger<UserDropdownQueryHandler> _logger = logger;

        public async Task<HandlerResult> Handle(UserDropdownQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _userAccountRepository.Query()
                    .Include(current => current.UserProfiles)
                    .ToListAsync(cancellationToken);

                DropDownViewModel dropdownViewModel = new()
                {
                    ListItems = response.Select(current =>
                    {
                        var profile = current.UserProfiles.FirstOrDefault();
                        var fullName = profile != null ? $"{profile.FirstName} {profile.LastName}" : current.Email;

                        return new DropDownItemViewModel
                        {
                            Text = fullName,
                            Value = current.Id.ToString(),
                        };
                    }).ToList(),
                };

                return new HandlerResult { RequestStatus = RequestStatus.Successful, ObjectResult = dropdownViewModel, Message = CommonMessages.Successful };
            }
            catch (Exception exception)
            {
                _logger.Log(LogLevel.Error, exception);
                return new HandlerResult { RequestStatus = RequestStatus.Failed, Message = CommonMessages.Failed };
            }
        }
    }
}