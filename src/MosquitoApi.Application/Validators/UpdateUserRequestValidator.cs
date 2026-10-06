using FluentValidation;
using MosquitoApi.Application.DTOs.User;
using MosquitoApi.Domain.Enums;

namespace MosquitoApi.Application.Validators;

/// <summary>
/// Validator for UpdateUserRequest
/// </summary>
public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .Length(1, 250).WithMessage("First name must be between 1 and 250 characters")
            .Matches(@"^[a-zA-Z\s'-]+$").WithMessage("First name can only contain letters, spaces, apostrophes, and hyphens")
            .When(x => !string.IsNullOrEmpty(x.FirstName));

        RuleFor(x => x.LastName)
            .Length(1, 250).WithMessage("Last name must be between 1 and 250 characters")
            .Matches(@"^[a-zA-Z\s'-]+$").WithMessage("Last name can only contain letters, spaces, apostrophes, and hyphens")
            .When(x => !string.IsNullOrEmpty(x.LastName));

        RuleFor(x => x.FatherName)
            .Length(1, 250).WithMessage("Father name must be between 1 and 250 characters")
            .Matches(@"^[a-zA-Z\s'-]+$").WithMessage("Father name can only contain letters, spaces, apostrophes, and hyphens")
            .When(x => !string.IsNullOrEmpty(x.FatherName));

        RuleFor(x => x.Username)
            .Length(3, 250).WithMessage("Username must be between 3 and 250 characters")
            .Matches(@"^[a-zA-Z0-9_.-]+$").WithMessage("Username can only contain letters, numbers, underscores, dots, and hyphens")
            .Must(username => !username!.StartsWith(".") && !username.EndsWith("."))
            .WithMessage("Username cannot start or end with a dot")
            .Must(username => !username!.Contains(".."))
            .WithMessage("Username cannot contain consecutive dots")
            .When(x => !string.IsNullOrEmpty(x.Username));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Invalid email address format")
            .Length(5, 250).WithMessage("Email address must be between 5 and 250 characters")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Mobile)
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Invalid mobile number format")
            .When(x => !string.IsNullOrEmpty(x.Mobile));

        RuleFor(x => x.DateOfBirth)
            .Must(BeAValidAge).WithMessage("Date of birth must be between 13 and 120 years ago")
            .When(x => x.DateOfBirth.HasValue);

        RuleFor(x => x.GenderType)
            .IsInEnum().WithMessage("Invalid gender type")
            .When(x => x.GenderType.HasValue);

        RuleFor(x => x.RoleType)
            .IsInEnum().WithMessage("Invalid role type")
            .Must(role => role != RoleType.SuperAdmin).WithMessage("SuperAdmin role cannot be assigned through user update")
            .When(x => x.RoleType.HasValue);

        RuleFor(x => x.PhotoUrl)
            .Must(BeAValidUrl).WithMessage("Invalid photo URL format")
            .When(x => !string.IsNullOrEmpty(x.PhotoUrl));

        // At least one field must be provided for update
        RuleFor(x => x)
            .Must(HaveAtLeastOneField).WithMessage("At least one field must be provided for update");
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

    private static bool HaveAtLeastOneField(UpdateUserRequest request)
    {
        return !string.IsNullOrEmpty(request.FirstName) ||
               !string.IsNullOrEmpty(request.LastName) ||
               !string.IsNullOrEmpty(request.FatherName) ||
               !string.IsNullOrEmpty(request.Username) ||
               !string.IsNullOrEmpty(request.Email) ||
               !string.IsNullOrEmpty(request.Mobile) ||
               request.DateOfBirth.HasValue ||
               request.GenderType.HasValue ||
               request.RoleType.HasValue ||
               !string.IsNullOrEmpty(request.PhotoUrl) ||
               request.IsActive.HasValue;
    }
}