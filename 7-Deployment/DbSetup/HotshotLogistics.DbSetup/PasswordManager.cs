using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;

namespace HotshotLogistics.DbSetup
{
    public class PasswordManager
    {
        private const int s_defaultLength = 24;
        private const int s_minimumLength = 16;
        private readonly ILogger<PasswordManager> _logger;

        public PasswordManager(ILogger<PasswordManager> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public string GeneratePassword(int length = s_defaultLength)
        {
            if (length < s_minimumLength)
            {
                _logger.LogWarning("Password length {Length} is below minimum {MinimumLength}, using minimum", length,
                    s_minimumLength);
                length = s_minimumLength;
            }

            StringBuilder password = new(length);
            RandomNumberGenerator random = RandomNumberGenerator.Create();

            // Define character sets for each class
            const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lowercase = "abcdefghijklmnopqrstuvwxyz";
            const string digits = "0123456789";
            const string symbols = "!@#$%^&*()_+-=[]{}|;:,.<>?";

            string allCharacters = uppercase + lowercase + digits + symbols;

            // Ensure at least one character from each class
            password.Append(uppercase[RandomInt(random, uppercase.Length)]);
            password.Append(lowercase[RandomInt(random, lowercase.Length)]);
            password.Append(digits[RandomInt(random, digits.Length)]);
            password.Append(symbols[RandomInt(random, symbols.Length)]);

            // Fill the rest randomly from all characters
            for (int i = 4; i < length; i++)
            {
                password.Append(allCharacters[RandomInt(random, allCharacters.Length)]);
            }

            // Shuffle the password to avoid predictable patterns
            string shuffledPassword = ShuffleString(password.ToString(), random);

            _logger.LogInformation("Generated secure password of length {Length}", length);
            return shuffledPassword;
        }

        public bool ValidatePassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                _logger.LogWarning("Password validation failed: password is null or empty");
                return false;
            }

            if (password.Length < s_minimumLength)
            {
                _logger.LogWarning("Password validation failed: length {Length} is below minimum {MinimumLength}",
                    password.Length, s_minimumLength);
                return false;
            }

            bool hasUppercase = password.Any(char.IsUpper);
            bool hasLowercase = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);
            bool hasSymbol = password.Any(c => !char.IsLetterOrDigit(c));

            if (!hasUppercase)
            {
                _logger.LogWarning("Password validation failed: missing uppercase letters");
                return false;
            }

            if (!hasLowercase)
            {
                _logger.LogWarning("Password validation failed: missing lowercase letters");
                return false;
            }

            if (!hasDigit)
            {
                _logger.LogWarning("Password validation failed: missing digits");
                return false;
            }

            if (!hasSymbol)
            {
                _logger.LogWarning("Password validation failed: missing symbols");
                return false;
            }

            _logger.LogInformation("Password validation passed for password of length {Length}", password.Length);
            return true;
        }

        public string GetPasswordPolicyDescription()
        {
            return
                $"Password must be at least {s_minimumLength} characters long and contain at least one uppercase letter, one lowercase letter, one digit, and one symbol.";
        }

        private static int RandomInt(RandomNumberGenerator random, int maxValue)
        {
            byte[] bytes = new byte[4];
            random.GetBytes(bytes);
            return Math.Abs(BitConverter.ToInt32(bytes, 0)) % maxValue;
        }

        private static string ShuffleString(string input, RandomNumberGenerator random)
        {
            char[] characters = input.ToCharArray();
            for (int i = characters.Length - 1; i > 0; i--)
            {
                int j = RandomInt(random, i + 1);
                (characters[i], characters[j]) = (characters[j], characters[i]);
            }

            return new string(characters);
        }
    }
}
