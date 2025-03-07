﻿using DomainLayer.Common.BaseEntities;

namespace DomainLayer.Entities
{
    public class UserProfile : BaseEntityModel, IAuditableEntity
    {
        public int UserAccountId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string NationalCode { get; set; }

        public string LocalPhoneNumber { get; set; }

        public string Mobile { get; set; }

        public string Address { get; set; }

        public string Company { get; set; }

        public UserAccount UserAccount { get; set; }
    }
}