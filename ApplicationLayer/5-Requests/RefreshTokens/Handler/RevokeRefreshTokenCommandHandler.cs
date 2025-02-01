using ApplicationLayer.Requests.RefreshTokens.Command;
using ApplicationLayer.Common;
using ApplicationLayer.Extensions.ServiceMessages;
using ApplicationLayer.Extensions.SmartEnums;
using ApplicationLayer.Extensions.Utilities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ApplicationLayer.Requests.RefreshTokens.Handler
{
    public class RevokeRefreshTokenCommandHandler(IRefreshTokenService refreshTokenService, IUnitOfWork unitOfWork, ILogger<RevokeRefreshTokenCommandHandler> logger) : IRequestHandler<RevokeRefreshTokenCommand, HandlerResult>
    {
        private readonly IRefreshTokenService _refreshTokenService = refreshTokenService;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ILogger<RevokeRefreshTokenCommandHandler> _logger = logger;

        public async Task<HandlerResult> Handle(RevokeRefreshTokenCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _refreshTokenService.RevokeRefreshTokenByUserId(request.UserId);

                if (result.RequestStatus != RequestStatus.Successful)
                {
                    return new HandlerResult
                    {
                        RequestStatus = result.RequestStatus,
                        ObjectResult = request,
                        Message = result.Message
                    };
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return new HandlerResult
                {
                    RequestStatus = RequestStatus.Successful,
                    ObjectResult = request,
                    Message = CommonMessages.Successful
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(message: ex.Message, CommonMessages.Failed);
                return new HandlerResult
                {
                    RequestStatus = RequestStatus.Failed,
                    ObjectResult = request,
                    Message = CommonMessages.Failed
                };
            }
        }
    }
}