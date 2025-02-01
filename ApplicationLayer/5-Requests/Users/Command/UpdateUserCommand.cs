using ApplicationLayer.Common;
using MediatR;

namespace ApplicationLayer.Requests.Users.Command
{
    public class UpdateUserCommand : IRequest<HandlerResult>
    {
        public int Id { get; set; }

        public string UserName { get; set; }

        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string NationalCode { get; set; }

        public string LocalPhoneNumber { get; set; }

        public string Mobile { get; set; }

        public string Address { get; set; }

        public string Company { get; set; }
    }
}