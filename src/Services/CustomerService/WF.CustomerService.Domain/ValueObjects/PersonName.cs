using System.Text.RegularExpressions;
using WF.Shared.Contracts.Result;

namespace WF.CustomerService.Domain.ValueObjects;

public readonly record struct PersonName
{
    private const int MaxNameLength = 100;
    private static readonly Regex NameRegex = new(
        @"^[a-zA-Z\s\-'\.]+$",
        RegexOptions.Compiled,
        TimeSpan.FromMilliseconds(250));

    public string FirstName { get; }
    public string LastName { get; }

    private PersonName(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    public static Result<PersonName> Create(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            return Result<PersonName>.Failure(Error.Validation("PersonName.FirstName.Required", "First name cannot be null or empty."));

        if (string.IsNullOrWhiteSpace(lastName))
            return Result<PersonName>.Failure(Error.Validation("PersonName.LastName.Required", "Last name cannot be null or empty."));

        var trimmedFirstName = firstName.Trim();
        var trimmedLastName = lastName.Trim();

        if (trimmedFirstName.Length > MaxNameLength)
            return Result<PersonName>.Failure(Error.Validation("PersonName.FirstName.MaxLength", $"First name must not exceed {MaxNameLength} characters."));

        if (trimmedLastName.Length > MaxNameLength)
            return Result<PersonName>.Failure(Error.Validation("PersonName.LastName.MaxLength", $"Last name must not exceed {MaxNameLength} characters."));

        if (!NameRegex.IsMatch(trimmedFirstName))
            return Result<PersonName>.Failure(Error.Validation("PersonName.FirstName.InvalidFormat", "First name can only contain letters, spaces, hyphens, apostrophes, and periods."));

        if (!NameRegex.IsMatch(trimmedLastName))
            return Result<PersonName>.Failure(Error.Validation("PersonName.LastName.InvalidFormat", "Last name can only contain letters, spaces, hyphens, apostrophes, and periods."));

        return Result<PersonName>.Success(new PersonName(trimmedFirstName, trimmedLastName));
    }

    public string FullName => $"{FirstName} {LastName}";

    public override string ToString() => FullName;
}

