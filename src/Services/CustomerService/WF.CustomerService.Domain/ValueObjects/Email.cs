using System.Net.Mail;
using System.Text.RegularExpressions;
using WF.Shared.Contracts.Result;

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

    public static Result<Email> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<Email>.Failure(Error.Validation("Email.Required", "Email cannot be null or empty."));

        var trimmedValue = value.Trim();

        if (trimmedValue.Length > MaxEmailLength)
            return Result<Email>.Failure(Error.Validation("Email.MaxLength", $"Email must not exceed {MaxEmailLength} characters."));

        if (!EmailRegex.IsMatch(trimmedValue))
            return Result<Email>.Failure(Error.Validation("Email.InvalidFormat", "Email must be a valid email address."));

        try
        {
            var mailAddress = new MailAddress(trimmedValue);
            if (mailAddress.Address != trimmedValue)
                return Result<Email>.Failure(Error.Validation("Email.InvalidCharacters", "Email contains invalid characters."));
        }
        catch (Exception ex) when (ex is ArgumentException || ex is FormatException)
        {
            return Result<Email>.Failure(Error.Validation("Email.InvalidFormat", "Email must be a valid email address."));
        }

        return Result<Email>.Success(new Email(trimmedValue));
    }

    // for efcore
    public static Email FromDatabaseValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException("Email cannot be null or empty when reading from database.");

        return new Email(value.Trim());
    }

    public static implicit operator string(Email email) => email.Value;

    public override string ToString() => Value;
}

