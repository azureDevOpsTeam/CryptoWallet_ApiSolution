using DomainLayer.Common.BaseEntities;

namespace DomainLayer.Entities
{
    public class UserAccount : BaseEntityModel, IAuditableEntity
    {
        public string UserName { get; set; }

        public string Password { get; set; }

        public string Email { get; set; }

        public bool ConfirmEmail { get; set; }

        public string PhoneNumber { get; set; }

        public bool ConfirmPhoneNumber { get; set; }

        public bool TwoFactorEnabled { get; set; }

        public string SecurityStamp { get; set; }

        public ICollection<UserProfile> UserProfiles { get; set; } = [];

        public ICollection<UserRole> UserRoles { get; set; } = [];

        public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
    }
}