namespace RestauranteUni.Domain.ValuesObjects
{
    public class Email
    {
        public string Value { get;}

        public Email(string? value)
        {
            ArgumentException.ThrowIfNullOrEmpty(value);
            ArgumentException.ThrowIfNullOrWhiteSpace(value);

            if (!IsValid(value))
            {
                throw new ArgumentException("Invalid e-mail");
            }
            Value = value.ToLowerInvariant();
        }

        private static bool IsValid(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
