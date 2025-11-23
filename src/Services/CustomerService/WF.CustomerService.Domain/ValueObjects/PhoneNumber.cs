using System.Text.RegularExpressions;
using WF.Shared.Contracts.Result;

namespace WF.CustomerService.Domain.ValueObjects;

public readonly record struct PhoneNumber
{
    private const int MinPhoneNumberLength = 10;
    private const int MaxPhoneNumberLength = 20;
    private static readonly Regex PhoneNumberRegex = new(
        @"^[\d\s\-\+\(\)]+$",
        RegexOptions.Compiled,
        TimeSpan.FromMilliseconds(250));

    public string Value { get; }

    private PhoneNumber(string value)
    {
        Value = value;
    }

    public static Result<PhoneNumber> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<PhoneNumber>.Failure(Error.Validation("PhoneNumber.Required", "Phone number cannot be null or empty."));

        var trimmedValue = value.Trim();

        if (trimmedValue.Length < MinPhoneNumberLength)
            return Result<PhoneNumber>.Failure(Error.Validation("PhoneNumber.MinLength", $"Phone number must be at least {MinPhoneNumberLength} characters."));

        if (trimmedValue.Length > MaxPhoneNumberLength)
            return Result<PhoneNumber>.Failure(Error.Validation("PhoneNumber.MaxLength", $"Phone number must not exceed {MaxPhoneNumberLength} characters."));

        if (!PhoneNumberRegex.IsMatch(trimmedValue))
            return Result<PhoneNumber>.Failure(Error.Validation("PhoneNumber.InvalidFormat", "Phone number can only contain digits, spaces, hyphens, plus signs, and parentheses."));

        var digitCount = trimmedValue.Count(char.IsDigit);
        if (digitCount < MinPhoneNumberLength - 2)
            return Result<PhoneNumber>.Failure(Error.Validation("PhoneNumber.InsufficientDigits", "Phone number must contain sufficient digits."));

        return Result<PhoneNumber>.Success(new PhoneNumber(trimmedValue));
    }

    public static PhoneNumber FromDatabaseValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException("Phone number cannot be null or empty when reading from database.");

        return new PhoneNumber(value.Trim());
    }

    public static implicit operator string(PhoneNumber phoneNumber) => phoneNumber.Value;

    public override string ToString() => Value;
}

