using System.Text.RegularExpressions;

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

    public PersonName(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be null or empty.", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be null or empty.", nameof(lastName));

        var trimmedFirstName = firstName.Trim();
        var trimmedLastName = lastName.Trim();

        if (trimmedFirstName.Length > MaxNameLength)
            throw new ArgumentException($"First name must not exceed {MaxNameLength} characters.", nameof(firstName));

        if (trimmedLastName.Length > MaxNameLength)
            throw new ArgumentException($"Last name must not exceed {MaxNameLength} characters.", nameof(lastName));

        if (!NameRegex.IsMatch(trimmedFirstName))
            throw new ArgumentException("First name can only contain letters, spaces, hyphens, apostrophes, and periods.", nameof(firstName));

        if (!NameRegex.IsMatch(trimmedLastName))
            throw new ArgumentException("Last name can only contain letters, spaces, hyphens, apostrophes, and periods.", nameof(lastName));

        FirstName = trimmedFirstName;
        LastName = trimmedLastName;
    }

    public string FullName => $"{FirstName} {LastName}";

    public override string ToString() => FullName;
}

