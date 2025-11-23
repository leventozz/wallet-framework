using System.Text.RegularExpressions;
using WF.Shared.Contracts.Result;

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

    private Iban(string value)
    {
        Value = value;
    }

    public static Result<Iban> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<Iban>.Failure(Error.Validation("Iban.Required", "IBAN cannot be null or empty."));

        var normalizedValue = value.Trim().Replace(" ", "").ToUpperInvariant();

        if (normalizedValue.Length < MinIbanLength || normalizedValue.Length > MaxIbanLength)
            return Result<Iban>.Failure(Error.Validation("Iban.InvalidLength", $"IBAN must be between {MinIbanLength} and {MaxIbanLength} characters."));

        if (!IbanRegex.IsMatch(normalizedValue))
            return Result<Iban>.Failure(Error.Validation("Iban.InvalidFormat", "IBAN format is invalid. It must start with 2 letters (country code) followed by 2 digits (check digits) and alphanumeric characters."));

        if (!ValidateCheckDigits(normalizedValue))
            return Result<Iban>.Failure(Error.Validation("Iban.InvalidCheckDigits", "IBAN check digits are invalid."));

        return Result<Iban>.Success(new Iban(normalizedValue));
    }

    public static Iban FromDatabaseValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException("IBAN cannot be null or empty when reading from database.");

        var normalizedValue = value.Trim().Replace(" ", "").ToUpperInvariant();

        if (normalizedValue.Length < MinIbanLength || normalizedValue.Length > MaxIbanLength)
            throw new InvalidOperationException($"IBAN must be between {MinIbanLength} and {MaxIbanLength} characters when reading from database.");

        if (!IbanRegex.IsMatch(normalizedValue))
            throw new InvalidOperationException("IBAN format is invalid when reading from database.");

        if (!ValidateCheckDigits(normalizedValue))
            throw new InvalidOperationException("IBAN check digits are invalid when reading from database.");

        return new Iban(normalizedValue);
    }

    private static bool ValidateCheckDigits(string iban)
    {
        var rearranged = iban.Substring(4) + iban.Substring(0, 4);

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

        var remainder = Mod97(numericString.ToString());
        return remainder == 1;
    }

    private static int Mod97(string number)
    {
        int remainder = 0;
        for (int i = 0; i < number.Length; i++)
        {
            remainder = (remainder * 10 + (number[i] - '0')) % 97;
        }
        return remainder;
    }

    public static implicit operator string(Iban iban) => iban.Value;

    public override string ToString() => Value;

    public string ToFormattedString()
    {
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

