using FluentValidation;
using MosquitoApi.Application.DTOs.User;
using MosquitoApi.Domain.Enums;

namespace MosquitoApi.Application.Validators;

/// <summary>
/// Validator for CreateUserRequest
/// </summary>
public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .Length(1, 250).WithMessage("First name must be between 1 and 250 characters")
            .Matches(@"^[a-zA-Z\s'-]+$").WithMessage("First name can only contain letters, spaces, apostrophes, and hyphens");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .Length(1, 250).WithMessage("Last name must be between 1 and 250 characters")
            .Matches(@"^[a-zA-Z\s'-]+$").WithMessage("Last name can only contain letters, spaces, apostrophes, and hyphens");

        RuleFor(x => x.FatherName)
            .Length(1, 250).WithMessage("Father name must be between 1 and 250 characters")
            .Matches(@"^[a-zA-Z\s'-]+$").WithMessage("Father name can only contain letters, spaces, apostrophes, and hyphens")
            .When(x => !string.IsNullOrEmpty(x.FatherName));

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required")
            .Length(3, 250).WithMessage("Username must be between 3 and 250 characters")
            .Matches(@"^[a-zA-Z0-9_.-]+$").WithMessage("Username can only contain letters, numbers, underscores, dots, and hyphens")
            .Must(username => !username.StartsWith(".") && !username.EndsWith("."))
            .WithMessage("Username cannot start or end with a dot")
            .Must(username => !username.Contains(".."))
            .WithMessage("Username cannot contain consecutive dots");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email address is required")
            .EmailAddress().WithMessage("Invalid email address format")
            .Length(5, 250).WithMessage("Email address must be between 5 and 250 characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .Length(6, 100).WithMessage("Password must be between 6 and 100 characters")
            .Must(ContainUppercase).WithMessage("Password must contain at least one uppercase letter")
            .Must(ContainLowercase).WithMessage("Password must contain at least one lowercase letter")
            .Must(ContainDigit).WithMessage("Password must contain at least one number")
            .Must(ContainSpecialCharacter).WithMessage("Password must contain at least one special character (!@#$%^&*()_+-=[]{}|;:,.<>?)");

        RuleFor(x => x.Mobile)
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Invalid mobile number format")
            .When(x => !string.IsNullOrEmpty(x.Mobile));

        RuleFor(x => x.DateOfBirth)
            .Must(BeAValidAge).WithMessage("Date of birth must be between 13 and 120 years ago")
            .When(x => x.DateOfBirth.HasValue);

        RuleFor(x => x.GenderType)
            .IsInEnum().WithMessage("Invalid gender type");

        RuleFor(x => x.RoleType)
            .IsInEnum().WithMessage("Invalid role type")
            .Must(role => role != RoleType.SuperAdmin).WithMessage("SuperAdmin role cannot be assigned through user creation");

        RuleFor(x => x.PhotoUrl)
            .Must(BeAValidUrl).WithMessage("Invalid photo URL format")
            .When(x => !string.IsNullOrEmpty(x.PhotoUrl));
    }

    private static bool ContainUppercase(string password)
    {
        return password.Any(char.IsUpper);
    }

    private static bool ContainLowercase(string password)
    {
        return password.Any(char.IsLower);
    }

    private static bool ContainDigit(string password)
    {
        return password.Any(char.IsDigit);
    }

    private static bool ContainSpecialCharacter(string password)
    {
        var specialChars = "!@#$%^&*()_+-=[]{}|;:,.<>?";
        return password.Any(specialChars.Contains);
    }

    private static bool BeAValidAge(DateTime? dateOfBirth)
    {
        if (!dateOfBirth.HasValue) return true;

        var age = DateTime.Today.Year - dateOfBirth.Value.Year;
        if (dateOfBirth.Value.Date > DateTime.Today.AddYears(-age)) age--;

        return age >= 13 && age <= 120;
    }

    private static bool BeAValidUrl(string? url)
    {
        if (string.IsNullOrEmpty(url)) return true;
        return Uri.TryCreate(url, UriKind.Absolute, out var result) && 
               (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
}