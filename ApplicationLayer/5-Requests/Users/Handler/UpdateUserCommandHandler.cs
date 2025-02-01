using ApplicationLayer.Requests.Users.Command;
using ApplicationLayer.Common;
using ApplicationLayer.Extensions;
using ApplicationLayer.Extensions.ServiceMessages;
using ApplicationLayer.Extensions.SmartEnums;
using DomainLayer.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ApplicationLayer.Requests.Users.Handler
{
    public class UpdateUserCommandHandler(IUnitOfWork unitOfWork, IRepository<UserAccount> userAccountRepository, IRepository<UserProfile> userProfileRepository, ILogger<UpdateUserCommandHandler> logger) : IRequestHandler<UpdateUserCommand, HandlerResult>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IRepository<UserAccount> _userAccountRepository = userAccountRepository;
        private readonly IRepository<UserProfile> _userProfileRepository = userProfileRepository;
        private readonly ILogger<UpdateUserCommandHandler> _logger = logger;

        public async Task<HandlerResult> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                var userAccount = await UpdateUserAccount(request);
                if (userAccount.RequestStatus != RequestStatus.Successful)
                    return new HandlerResult { RequestStatus = userAccount.RequestStatus, Message = userAccount.Message };
                await _unitOfWork.SaveChangesAsync();

                var userProfile = await UpdateUserProfile(request);
                if (userProfile.RequestStatus != RequestStatus.Successful)
                    return new HandlerResult { RequestStatus = userProfile.RequestStatus, Message = userProfile.Message };
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitAsync();
                return new HandlerResult { RequestStatus = RequestStatus.Successful, Message = CommonMessages.Successful };
            }
            catch (Exception exception)
            {
                await _unitOfWork.RollbackAsync();
                _logger.Log(LogLevel.Error, exception);
                return new HandlerResult { RequestStatus = RequestStatus.Failed, Message = CommonMessages.Failed };
            }
        }

        private async Task<ServiceResult<UserAccount>> UpdateUserAccount(UpdateUserCommand request)
        {
            try
            {
                var userAccount = await _userAccountRepository.Query().FirstOrDefaultAsync(current => current.Id.Equals(request.Id));

                userAccount.UserName = request.UserName;
                userAccount.Email = request.Email;

                _userAccountRepository.Update(userAccount);
                return new ServiceResult<UserAccount> { RequestStatus = RequestStatus.Successful, Data = userAccount, Message = CommonMessages.Successful };
            }
            catch (Exception exception)
            {
                _logger.Log(LogLevel.Error, exception);
                return new ServiceResult<UserAccount> { RequestStatus = RequestStatus.Failed, Message = CommonMessages.Failed };
            }
        }

        private async Task<ServiceResult<UserProfile>> UpdateUserProfile(UpdateUserCommand request)
        {
            try
            {
                var userProfile = await _userProfileRepository.Query().FirstOrDefaultAsync(current => current.UserAccountId.Equals(request.Id));

                userProfile.FirstName = request.FirstName;
                userProfile.LastName = request.LastName;
                userProfile.NationalCode = request.NationalCode;
                userProfile.Address = request.Address;
                userProfile.Company = request.Company;
                userProfile.LocalPhoneNumber = request.LocalPhoneNumber;
                userProfile.Mobile = request.Mobile;

                _userProfileRepository.Update(userProfile);
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