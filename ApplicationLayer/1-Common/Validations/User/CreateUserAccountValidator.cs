using ApplicationLayer.Requests.Users.Command;
using ApplicationLayer.Extensions.ServiceMessages;
using FluentValidation;

namespace ApplicationLayer.Common.Validations.User
{
    public class CreateUserAccountValidator : AbstractValidator<SignUpCommand>
    {
        public CreateUserAccountValidator()
        {
            RuleFor(row => row.UserName).NotNull()
                .WithErrorCode(ValidationErrorCodes.NotNull).WithMessage(CommonValidateMessages.NotNull("نام کاربری"))
                .Length(0, 200).WithErrorCode(ValidationErrorCodes.LengthExceed).WithMessage(CommonValidateMessages.LengthExceed("نام کاربری", 100));

            RuleFor(row => row.Password).NotNull()
                .WithErrorCode(ValidationErrorCodes.NotNull).WithMessage(CommonValidateMessages.NotNull("رمز عبور"))
                .Length(0, 128).WithErrorCode(ValidationErrorCodes.LengthExceed).WithMessage(CommonValidateMessages.LengthExceed("رمز عبور", 128));
        }
    }
}