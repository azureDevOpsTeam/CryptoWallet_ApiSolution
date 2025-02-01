using ApplicationLayer.Requests.Users.Command;
using ApplicationLayer.Common;
using ApplicationLayer.Extensions;
using ApplicationLayer.Extensions.ServiceMessages;
using ApplicationLayer.Extensions.SmartEnums;
using ApplicationLayer.BusinessLogic.UserAccounts;
using DomainLayer.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ApplicationLayer.Requests.Users.Handler
{
    public class SignUpCommandHandler(IUnitOfWork unitOfWork, IHttpContextAccessor accessor, IUserAccountServices userAccountServices, IRepository<Role> roleRepository, IRepository<UserRole> userRoleRepository, ILogger<SignUpCommandHandler> logger) : IRequestHandler<SignUpCommand, HandlerResult>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IHttpContextAccessor _accessor = accessor;
        private readonly IUserAccountServices _userAccountServices = userAccountServices;
        private readonly IRepository<Role> _roleRepository = roleRepository;
        private readonly IRepository<UserRole> _userRoleRepository = userRoleRepository;
        private readonly ILogger<SignUpCommandHandler> _logger = logger;

        public async Task<HandlerResult> Handle(SignUpCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                var userAccount = await _userAccountServices.SignUpAccountAsync(request);
                if (userAccount.RequestStatus != RequestStatus.Successful)
                    return new HandlerResult { RequestStatus = userAccount.RequestStatus, Message = userAccount.Message };
                await _unitOfWork.SaveChangesAsync();

                var userProfile = await _userAccountServices.CreateUserProfile(request, userAccount.Data.Id);
                if (userProfile.RequestStatus != RequestStatus.Successful)
                    return new HandlerResult { RequestStatus = userProfile.RequestStatus, Message = userProfile.Message };
                await _unitOfWork.SaveChangesAsync();

                var roleString = string.Empty;
                if (_accessor.HttpContext.User.IsInRole("Admin"))
                    roleString = "courier";
                else if (_accessor.HttpContext.User.IsInRole("Courier"))
                    roleString = "fleet";

                var getUserRole = await _roleRepository.GetDbSet().FirstOrDefaultAsync(current => current.RoleName.ToLower() == roleString);
                UserRole userRole = new()
                {
                    UserAccountId = userAccount.Data.Id,
                    RoleId = getUserRole.Id
                };
                await _userRoleRepository.AddAsync(userRole);
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
    }
}