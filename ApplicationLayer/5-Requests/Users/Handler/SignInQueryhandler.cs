using ApplicationLayer.Requests.Users.Query;
using ApplicationLayer.Common;
using ApplicationLayer.Extensions;
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

namespace ApplicationLayer.Requests.Users.Handler
{
    public class SignInQueryHandler(IUnitOfWork unitOfWork, IRepository<UserAccount> userAccountRepository, IRefreshTokenService refreshTokenService, IHttpContextAccessor accessor, ILogger<SignInQueryHandler> logger, IConfiguration configuration) : IRequestHandler<SignInQuery, HandlerResult>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IConfiguration _configuration = configuration;
        private readonly IRefreshTokenService _refreshTokenService = refreshTokenService;
        private readonly IHttpContextAccessor _accessor = accessor;
        private readonly IRepository<UserAccount> _userAccountRepository = userAccountRepository;
        private readonly ILogger<SignInQueryHandler> _logger = logger;
        public async Task<HandlerResult> Handle(SignInQuery request, CancellationToken cancellationToken)
        {
            try
            {
                AuthorizeTokenResultViewModel tokenResult = new();
                var result = await LoginAsync(request);
                if (string.IsNullOrEmpty(result.AccessTokens))
                    return new HandlerResult { RequestStatus = RequestStatus.AuthenticationFailed, Message = CommonMessages.IncorrectUser };

                if (result.RequestStatus != RequestStatus.Successful)
                    return new HandlerResult { RequestStatus = result.RequestStatus, Message = result.Message };

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                tokenResult.AccessTokens = result.AccessTokens;
                tokenResult.RefreshToken = result.RefreshToken;

                return new HandlerResult { RequestStatus = RequestStatus.Successful, ObjectResult = tokenResult, Message = CommonMessages.Successful };
            }
            catch (Exception exception)
            {
                _logger.Log(LogLevel.Error, exception);
                return new HandlerResult { RequestStatus = RequestStatus.Failed, Message = CommonMessages.Failed };
            }
        }

        public async Task<AuthorizeResultViewModel> LoginAsync(SignInQuery request)
        {
            AuthorizeResultViewModel result = new();

            var response = await _userAccountRepository.Query()
                .Include(current => current.UserRoles)
                .ThenInclude(current => current.Role)
                .FirstOrDefaultAsync(current => current.UserName == request.UserName);

            if (response is null)
            {
                result.AccessTokens = string.Empty;
                return result;
            }

            if (response.UserRoles.FirstOrDefault().Role.RoleName.ToLower() == "fleet" && _accessor.HttpContext.Request.Headers["X-UserAgent"].ToString().Contains("WebApp"))
            {
                result.AccessTokens = string.Empty;
                return result;
            }

            var identityUser = HashGenerator.VerifyPassword(request.Password, response.Password, response.SecurityStamp);

            result.RequestStatus = identityUser ? RequestStatus.Successful : RequestStatus.IncorrectUser;
            result.Message = identityUser ? CommonMessages.Successful : CommonMessages.IncorrectUser;

            var token = TokenGenerator(request.UserName, response.Id, response.UserRoles.FirstOrDefault().Role.RoleName);
            var refreshToken = _refreshTokenService.RefreshTokenGenerator(response.Id, token.tokenId);
            var refreshTokenResult = _refreshTokenService.AddRefreshToken(refreshToken);

            result.AccessTokens = token.jwtToken;
            result.RefreshToken = refreshToken.Token;

            return result;
        }

        public (string jwtToken, string tokenId) TokenGenerator(string userName, int userId, string role)
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
                    new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new(JwtRegisteredClaimNames.Iat, DateTime.Now.ToUniversalTime().ToString())
                ]),
                Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration.GetSection("JWT:JwtTokenExpirationTimeInMinutes").Value)),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            var jwtToken = tokenHandler.WriteToken(token);

            return (jwtToken, token.Id);
        }
    }
}