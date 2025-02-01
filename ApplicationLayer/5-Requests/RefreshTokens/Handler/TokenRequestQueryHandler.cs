using ApplicationLayer.Requests.RefreshTokens.Query;
using ApplicationLayer.Common;
using ApplicationLayer.Common.ViewModels;
using ApplicationLayer.Extensions.ServiceMessages;
using ApplicationLayer.Extensions.SmartEnums;
using ApplicationLayer.Extensions.Utilities;
using DomainLayer.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ApplicationLayer.Requests.RefreshTokens.Handler
{
    public class TokenRequestQueryHandler(IRepository<UserAccount> userAccountRepository, IHttpContextAccessor accessor, IConfiguration configuration,
                                          IRefreshTokenService refreshTokenService, IUnitOfWork unitOfWork, ILogger<TokenRequestViewModel> logger) : IRequestHandler<TokenRequestQuery, HandlerResult>
    {
        private readonly IConfiguration _configuration = configuration;
        private readonly IHttpContextAccessor _accessor = accessor;
        private readonly IRefreshTokenService _refreshTokenService = refreshTokenService;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IRepository<UserAccount> _userAccountRepository = userAccountRepository;
        private readonly ILogger<TokenRequestViewModel> _logger = logger;

        public async Task<HandlerResult> Handle(TokenRequestQuery request, CancellationToken cancellationToken)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var result = await _refreshTokenService.VerifyToken(request.InputData);

                if (result.RequestStatus != RequestStatus.Failed && result.RequestStatus != RequestStatus.Successful)
                    return new HandlerResult { RequestStatus = RequestStatus.Successful, ObjectResult = request.InputData, Message = result.Message };

                if (result.RequestStatus != RequestStatus.Successful)
                {
                    await _unitOfWork.RollbackAsync();
                    return new HandlerResult
                    {
                        RequestStatus = result.RequestStatus,
                        ObjectResult = request.InputData,
                        Message = result.Message
                    };
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var storedToken = (RefreshToken)result.Data;

                var userModel = await _userAccountRepository.Query().FirstOrDefaultAsync(current => current.Id.Equals(storedToken.UserAccountId));

                var roleString = string.Empty;
                if (_accessor.HttpContext.User.IsInRole("Admin"))
                    roleString = "courier";
                else if (_accessor.HttpContext.User.IsInRole("Courier"))
                    roleString = "fleet";

                var token = TokenGenerator(userModel.UserName, userModel.Id, roleString, storedToken.JwtId);

                var refreshToken = _refreshTokenService.RefreshTokenGenerator(userModel.Id, storedToken.JwtId);
                refreshToken.UserFullName = storedToken.UserFullName;
                var refreshTokenResult = _refreshTokenService.AddRefreshToken(refreshToken);

                if (refreshTokenResult.RequestStatus != RequestStatus.Successful)
                {
                    await _unitOfWork.RollbackAsync();
                    return new HandlerResult
                    {
                        RequestStatus = refreshTokenResult.RequestStatus,
                        ObjectResult = null,
                        Message = refreshTokenResult.Message
                    };
                }
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitAsync();

                var authorizeResultViewModel = new AuthorizeResultViewModel()
                {
                    AccessTokens = token.jwtToken,
                    RefreshToken = refreshToken.Token,
                    UserFullName = storedToken.UserFullName
                };

                return new HandlerResult
                {
                    RequestStatus = RequestStatus.Successful,
                    ObjectResult = authorizeResultViewModel,
                    Message = CommonMessages.Successful
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(message: ex.Message, CommonMessages.Failed);
                return new HandlerResult
                {
                    RequestStatus = RequestStatus.Failed,
                    ObjectResult = request.InputData,
                    Message = CommonMessages.Failed
                };
            }
        }

        public (string jwtToken, string tokenId) TokenGenerator(string userName, int userId, string role, string Jti)
        {
            if (_configuration == null) throw new ArgumentNullException(nameof(_configuration));
            if (_accessor == null) throw new ArgumentNullException(nameof(_accessor));

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenKey = Encoding.UTF8.GetBytes(_configuration["JWT:Key"]);

            if (tokenKey.Length < 32)
            {
                throw new ArgumentException("JWT key must be at least 256 bits (32 bytes) long.");
            }

            string clientAppId = null;

            if (_accessor.HttpContext != null && _accessor.HttpContext.Request.Headers.TryGetValue("ClientAppId", out var clientAppIdValues))
            {
                clientAppId = clientAppIdValues.FirstOrDefault();
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                [
                    new(ClaimTypes.Name, userName),
                    new(ClaimTypes.NameIdentifier, userId.ToString()),
                    new(ClaimTypes.Role, role),
                    new("ClientAppId", clientAppId ?? ""),
                    new(JwtRegisteredClaimNames.Jti, Jti),
                    new(JwtRegisteredClaimNames.Iat, DateTime.Now.ToUniversalTime().ToString())
                ]),
                Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration.GetSection("JWT:JwtTokenExpirationTimeInMinutes").Value)),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            var jwtToken = tokenHandler.WriteToken(token);

            return (jwtToken, token.Id);
        }

        //public string TokenGenerator(string userName, int userId, string role)
        //{
        //    var tokenHandler = new JwtSecurityTokenHandler();
        //    var tokenKey = Encoding.UTF8.GetBytes(_configuration["JWT:Key"]);
        //    var tokenDescriptor = new SecurityTokenDescriptor
        //    {
        //        Subject = new ClaimsIdentity(new Claim[]
        //        {
        //            new(ClaimTypes.Name, userName),
        //            new(ClaimTypes.NameIdentifier, userId.ToString()),
        //            new(ClaimTypes.Role, role)
        //        }),
        //        Expires = DateTime.UtcNow.AddMinutes(15),
        //        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256Signature)
        //    };
        //    var token = tokenHandler.CreateToken(tokenDescriptor);
        //    return tokenHandler.WriteToken(token);
        //}
    }
}