﻿using ApplicationLayer.Common;
using MediatR;

namespace ApplicationLayer.Requests.Users.Command
{
    public class SignUpCommand : IRequest<HandlerResult>
    {
        public string UserName { get; set; }

        public string Password { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string NationalCode { get; set; }

        public string LocalPhoneNumber { get; set; }

        public string Mobile { get; set; }

        public string Address { get; set; }

        public string Company { get; set; }
    }
}