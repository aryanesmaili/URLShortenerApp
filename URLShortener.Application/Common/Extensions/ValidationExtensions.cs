using FluentValidation;
using System.Text.RegularExpressions;

namespace URLShortener.Application.Utility.ValidatorConfigs;

public static partial class ValidationExtensions
{
    #region IsRequired Validations
    // isRequired for string? properties
    public static IRuleBuilderOptions<T, string?> IsRequired<T>(
        this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode("REQUIRED")
            .WithMessage("{PropertyName} is required.");
    }

    // isRequired for numeric types (int, long, etc.)
    public static IRuleBuilderOptions<T, long> IsRequired<T>(
        this IRuleBuilder<T, long> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode("REQUIRED")
            .WithMessage("{PropertyName} is required.");
    }

    // isRequired for generic collections
    public static IRuleBuilderOptions<T, IEnumerable<TElement>> IsRequired<T, TElement>(
        this IRuleBuilder<T, IEnumerable<TElement>> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode("REQUIRED")
            .WithMessage("{PropertyName} is required.");
    }

    // isRequired for Enums
    public static IRuleBuilderOptions<T, TEnum> IsRequired<T, TEnum>(
        this IRuleBuilder<T, TEnum> ruleBuilder) where TEnum : struct, Enum
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode("REQUIRED")
            .WithMessage("{PropertyName} is required.");
    }

    #endregion

    #region URL Validations
    public static IRuleBuilderOptions<T, string?> ValidURL<T>(this IRuleBuilder<T, string?> ruleBuilder, bool isRequired)
    {
        if (isRequired)
            ruleBuilder.IsRequired();

        return ruleBuilder
            .Must(IsValidURL).WithMessage("Given string is not a URL");
    }

    private static bool IsValidURL(string? url)
        => string.IsNullOrEmpty(url) || (Uri.TryCreate(url, UriKind.Absolute, out var uri)
               && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps));
    #endregion

    #region Credentials Validations

    /// <summary>
    /// Validates that the name is between 4 and 32 characters long.
    /// </summary>
    public static IRuleBuilderOptions<T, string?> ValidName<T>(this IRuleBuilder<T, string?> ruleBuilder, bool isRequired)
    {
        if (isRequired)
            ruleBuilder.IsRequired();

        return ruleBuilder
            .MinimumLength(4)
            .WithMessage("{PropertyName} must be at least 4 characters long.")
            .MaximumLength(32)
            .WithMessage("{PropertyName} must be at most 32 characters long.");
    }

    public static IRuleBuilderOptions<T, string?> ValidEmail<T>(this IRuleBuilder<T, string?> ruleBuilder, bool isRequired)
    {
        if (isRequired)
            ruleBuilder.IsRequired();

        return ruleBuilder
            .EmailAddress()
            .WithMessage("Invalid email address format.");
    }

    /// <summary>
    /// Validates that the username is between 4 and 32 characters long and only contains letters, numbers, underscores, and dashes.
    /// </summary>
    public static IRuleBuilderOptions<T, string?> ValidUsername<T>(this IRuleBuilder<T, string?> ruleBuilder, bool isRequired)
    {
        if (isRequired)
            ruleBuilder.IsRequired();
        return ruleBuilder
            .MinimumLength(4)
            .WithMessage("{PropertyName} must be at least 4 characters long.")
            .MaximumLength(32)
            .WithMessage("{PropertyName} must be at most 32 characters long.")
            .Matches(UsernameRegex())
            .WithMessage("{PropertyName} can only contain letters, numbers, underscores, and dashes.");
    }

    [GeneratedRegex("^[a-zA-Z0-9_-]+$", RegexOptions.Compiled)]
    private static partial Regex UsernameRegex();

    /// <summary>
    /// Validates that the password is at least 5 characters long and contains at least one uppercase letter, one lowercase letter, one digit, and one special character.
    /// </summary>
    public static IRuleBuilderOptions<T, string?> ValidPassword<T>(this IRuleBuilder<T, string?> ruleBuilder, bool isRequired)
    {
        if (isRequired)
            ruleBuilder.IsRequired();
        return ruleBuilder
            .MinimumLength(5)
            .WithMessage("{PropertyName} must be at least 5 characters long.")
            .Matches(PasswordRegex())
            .WithMessage("{PropertyName} must contain at least one uppercase letter, one lowercase letter, one digit, and one special character.");
    }

    // Regex says at least 5 characters long, and should contain at least one uppercase letter, one lowercase letter, one digit, and one special character
    [GeneratedRegex("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[@$!%*?&])[A-Za-z\\d@$!%*?&]{5,}$", RegexOptions.Compiled)]
    private static partial Regex PasswordRegex();

    // Validation for Mobile Number (Iranian Format)
    public static IRuleBuilderOptions<T, string?> ValidMobileNumber<T>(
        this IRuleBuilder<T, string?> ruleBuilder, bool isRequired)
    {
        if (isRequired)
            ruleBuilder.IsRequired();

        return ruleBuilder
            .Must(value => string.IsNullOrEmpty(value) || MobileNumberRegex().IsMatch(value))
            .WithMessage("{PropertyName} must be a valid Iranian mobile number.");
    }

    [GeneratedRegex("^\\+98\\d{10}$", RegexOptions.Compiled)]
    private static partial Regex MobileNumberRegex();

    #endregion
}
