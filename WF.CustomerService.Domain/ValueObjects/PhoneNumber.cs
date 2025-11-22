using System.Text.RegularExpressions;

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

    public PhoneNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Phone number cannot be null or empty.", nameof(value));

        var trimmedValue = value.Trim();

        if (trimmedValue.Length < MinPhoneNumberLength)
            throw new ArgumentException($"Phone number must be at least {MinPhoneNumberLength} characters.", nameof(value));

        if (trimmedValue.Length > MaxPhoneNumberLength)
            throw new ArgumentException($"Phone number must not exceed {MaxPhoneNumberLength} characters.", nameof(value));

        if (!PhoneNumberRegex.IsMatch(trimmedValue))
            throw new ArgumentException("Phone number can only contain digits, spaces, hyphens, plus signs, and parentheses.", nameof(value));

 
        var digitCount = trimmedValue.Count(char.IsDigit);
        if (digitCount < MinPhoneNumberLength - 2)
            throw new ArgumentException("Phone number must contain sufficient digits.", nameof(value));

        Value = trimmedValue;
    }

    public static implicit operator string(PhoneNumber phoneNumber) => phoneNumber.Value;

    public static implicit operator PhoneNumber(string value) => new(value);

    public override string ToString() => Value;
}

