using System.Text.RegularExpressions;

namespace WF.WalletService.Domain.ValueObjects;

public readonly record struct Iban
{
    private const int MinIbanLength = 15;
    private const int MaxIbanLength = 34;
    private static readonly Regex IbanRegex = new(
        @"^[A-Z]{2}\d{2}[A-Z0-9]+$",
        RegexOptions.Compiled,
        TimeSpan.FromMilliseconds(250));

    public string Value { get; }

    public Iban(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("IBAN cannot be null or empty.", nameof(value));

        var normalizedValue = value.Trim().Replace(" ", "").ToUpperInvariant();

        if (normalizedValue.Length < MinIbanLength || normalizedValue.Length > MaxIbanLength)
            throw new ArgumentException($"IBAN must be between {MinIbanLength} and {MaxIbanLength} characters.", nameof(value));

        if (!IbanRegex.IsMatch(normalizedValue))
            throw new ArgumentException("IBAN format is invalid. It must start with 2 letters (country code) followed by 2 digits (check digits) and alphanumeric characters.", nameof(value));

        if (!ValidateCheckDigits(normalizedValue))
            throw new ArgumentException("IBAN check digits are invalid.", nameof(value));

        Value = normalizedValue;
    }

    private static bool ValidateCheckDigits(string iban)
    {
        // Move first 4 characters to end
        var rearranged = iban.Substring(4) + iban.Substring(0, 4);

        // Replace letters with numbers (A=10, B=11, ..., Z=35)
        var numericString = new System.Text.StringBuilder(rearranged.Length);
        foreach (var c in rearranged)
        {
            if (char.IsLetter(c))
            {
                numericString.Append((c - 'A' + 10).ToString());
            }
            else
            {
                numericString.Append(c);
            }
        }

        // Calculate mod 97 using big integer approach (to handle large numbers)
        var remainder = Mod97(numericString.ToString());
        return remainder == 1;
    }

    private static int Mod97(string number)
    {
        // Process in chunks to avoid overflow
        int remainder = 0;
        for (int i = 0; i < number.Length; i++)
        {
            remainder = (remainder * 10 + (number[i] - '0')) % 97;
        }
        return remainder;
    }

    public static implicit operator string(Iban iban) => iban.Value;

    public static implicit operator Iban(string value) => new(value);

    public override string ToString() => Value;

    public string ToFormattedString()
    {
        // Format IBAN with spaces every 4 characters
        var formatted = new System.Text.StringBuilder(Value.Length + (Value.Length / 4));
        for (int i = 0; i < Value.Length; i++)
        {
            if (i > 0 && i % 4 == 0)
                formatted.Append(' ');
            formatted.Append(Value[i]);
        }
        return formatted.ToString();
    }
}

