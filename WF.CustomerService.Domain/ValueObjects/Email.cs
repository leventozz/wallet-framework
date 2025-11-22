using System.Net.Mail;
using System.Text.RegularExpressions;

namespace WF.CustomerService.Domain.ValueObjects;

public readonly record struct Email
{
    private const int MaxEmailLength = 320;
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase,
        TimeSpan.FromMilliseconds(250));

    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email cannot be null or empty.", nameof(value));

        var trimmedValue = value.Trim();

        if (trimmedValue.Length > MaxEmailLength)
            throw new ArgumentException($"Email must not exceed {MaxEmailLength} characters.", nameof(value));

        if (!EmailRegex.IsMatch(trimmedValue))
            throw new ArgumentException("Email must be a valid email address.", nameof(value));

        try
        {
            var mailAddress = new MailAddress(trimmedValue);
            if (mailAddress.Address != trimmedValue)
                throw new ArgumentException("Email contains invalid characters.", nameof(value));
        }
        catch (Exception ex) when (ex is ArgumentException || ex is FormatException)
        {
            throw new ArgumentException("Email must be a valid email address.", nameof(value), ex);
        }

        return new Email(trimmedValue);
    }

    public static implicit operator string(Email email) => email.Value;

    public override string ToString() => Value;
}

