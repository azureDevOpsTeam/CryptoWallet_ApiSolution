using ApplicationLayer.Requests.Users.Command;
using ApplicationLayer.Common;
using ApplicationLayer.Extensions;
using ApplicationLayer.Extensions.ServiceMessages;
using ApplicationLayer.Extensions.SmartEnums;
using DomainLayer.Common.Attributes;
using DomainLayer.Entities;
using Microsoft.Extensions.Logging;

namespace ApplicationLayer.BusinessLogic.UserAccounts
{
    public interface IUserAccountServices
    {
        Task<ServiceResult<UserAccount>> SignUpAccountAsync(SignUpCommand request);

        Task<ServiceResult<UserProfile>> CreateUserProfile(SignUpCommand request, int userAccountId);

        Task<ServiceResult<UserAccount>> CreateAccount(UserAccount userAccount);

        Task<ServiceResult<UserProfile>> CreateUserProfile(UserProfile userProfile);
    }

    [InjectAsScoped]
    public class UserAccountServices(IRepository<UserAccount> userAccountRepository, IRepository<UserProfile> userProfileRepository, ILogger<UserAccountServices> logger) : IUserAccountServices
    {
        private readonly IRepository<UserAccount> _userAccountRepository = userAccountRepository;
        private readonly IRepository<UserProfile> _userProfileRepository = userProfileRepository;
        private readonly ILogger<UserAccountServices> _logger = logger;

        public async Task<ServiceResult<UserAccount>> SignUpAccountAsync(SignUpCommand request)
        {
            try
            {
                var userAccount = new UserAccount
                {
                    UserName = request.UserName,
                    Password = HashGenerator.GenerateSHA256HashWithSalt(request.Password, out string securityStamp),
                    SecurityStamp = securityStamp,
                    Email = request.Email,
                    ConfirmEmail = true,
                    PhoneNumber = request.PhoneNumber,
                    ConfirmPhoneNumber = true,
                    TwoFactorEnabled = false
                };
                await _userAccountRepository.AddAsync(userAccount);
                return new ServiceResult<UserAccount> { RequestStatus = RequestStatus.Successful, Data = userAccount, Message = CommonMessages.Successful };
            }
            catch (Exception exception)
            {
                _logger.Log(LogLevel.Error, exception);
                return new ServiceResult<UserAccount> { RequestStatus = RequestStatus.Failed, Message = CommonMessages.Failed };
            }
        }

        public async Task<ServiceResult<UserAccount>> CreateAccount(UserAccount userAccount)
        {
            try
            {
                await _userAccountRepository.AddAsync(userAccount);
                return new ServiceResult<UserAccount> { RequestStatus = RequestStatus.Successful, Data = userAccount, Message = CommonMessages.Successful };
            }
            catch (Exception exception)
            {
                _logger.Log(LogLevel.Error, exception);
                return new ServiceResult<UserAccount> { RequestStatus = RequestStatus.Failed, Message = CommonMessages.Failed };
            }
        }

        public async Task<ServiceResult<UserProfile>> CreateUserProfile(SignUpCommand request, int userAccountId)
        {
            try
            {
                UserProfile userProfile = new()
                {
                    UserAccountId = userAccountId,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    NationalCode = request.NationalCode,
                    Address = request.Address,
                    Company = request.Company,
                    LocalPhoneNumber = request.LocalPhoneNumber,
                    Mobile = request.Mobile,
                };

                await _userProfileRepository.AddAsync(userProfile);
                return new ServiceResult<UserProfile> { RequestStatus = RequestStatus.Successful, Data = userProfile, Message = CommonMessages.Successful };
            }
            catch (Exception exception)
            {
                _logger.Log(LogLevel.Error, exception);
                return new ServiceResult<UserProfile> { RequestStatus = RequestStatus.Failed, Message = CommonMessages.Failed };
            }
        }

        public async Task<ServiceResult<UserProfile>> CreateUserProfile(UserProfile userProfile)
        {
            try
            {
                await _userProfileRepository.AddAsync(userProfile);
                return new ServiceResult<UserProfile> { RequestStatus = RequestStatus.Successful, Data = userProfile, Message = CommonMessages.Successful };
            }
            catch (Exception exception)
            {
                _logger.Log(LogLevel.Error, exception);
                return new ServiceResult<UserProfile> { RequestStatus = RequestStatus.Failed, Message = CommonMessages.Failed };
            }
        }
    }
}