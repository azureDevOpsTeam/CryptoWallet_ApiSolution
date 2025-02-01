using System.Text.Json.Serialization;
using ApplicationLayer.Common;
using ApplicationLayer.Common.ViewModels;
using MediatR;

namespace ApplicationLayer.Requests.RefreshTokens.Query
{
    public class TokenRequestQuery : IRequest<HandlerResult>
    {
        public TokenRequestViewModel InputData { get; }

        [JsonConstructor]
        public TokenRequestQuery(TokenRequestViewModel inputData)
        {
            InputData = inputData;
        }
    }
}