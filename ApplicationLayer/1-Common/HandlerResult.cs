#region Usings

using ApplicationLayer.Common.Validations;
using ApplicationLayer.Extensions.SmartEnums;

#endregion

namespace ApplicationLayer.Common
{
    public class HandlerResult
    {
        public ValidationResult ValidationResult { get; set; }

        public RequestStatus RequestStatus { get; set; }

        public string Message { get; set; }

        public object ObjectResult { get; set; }

        private static NotificationType GetNotificationType(RequestStatus requestStatus)
        {
            return (int)requestStatus switch
            {
                var SuccessfulRow when SuccessfulRow.Equals(RequestStatus.Successful) => NotificationType.Success,
                var failedRow when failedRow.Equals(RequestStatus.Failed) => NotificationType.Error,
                _ => NotificationType.Warning,
            };
        }
    }
}