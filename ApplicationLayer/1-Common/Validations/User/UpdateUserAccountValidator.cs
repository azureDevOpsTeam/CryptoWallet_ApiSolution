using ApplicationLayer.Requests.Users.Command;
using ApplicationLayer.Extensions.ServiceMessages;
using FluentValidation;

namespace ApplicationLayer.Common.Validations.User
{
    public class UpdateUserAccountValidator : AbstractValidator<UpdateUserCommand>
    {
        public UpdateUserAccountValidator()
        {
            RuleFor(row => row.UserName).NotNull()
                .WithErrorCode(ValidationErrorCodes.NotNull).WithMessage(CommonValidateMessages.NotNull("UserName"))
                .Length(0, 200).WithErrorCode(ValidationErrorCodes.LengthExceed).WithMessage(CommonValidateMessages.LengthExceed("UserName", 100));
        }
    }
}