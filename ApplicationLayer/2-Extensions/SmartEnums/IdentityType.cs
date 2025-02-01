using Ardalis.SmartEnum;

namespace ApplicationLayer.Extensions.SmartEnums
{
    public class IdentityType : SmartEnum<IdentityType>
    {
        public static readonly IdentityType POD = new(0, "کد یکبار مصرف", "OneTimePassword");
        public static readonly IdentityType NationalCard = new(1, "کارت ملی", "NationalCard");
        public static readonly IdentityType IdentityDocument = new(2, "شناسنامه", "IdentityDocument");

        public string PersianName { get; }

        public string EnglishName { get; }

        private IdentityType(int id, string persianName, string englishName) : base(englishName, id)
        {
            PersianName = persianName;
            EnglishName = englishName;
        }
    }
}