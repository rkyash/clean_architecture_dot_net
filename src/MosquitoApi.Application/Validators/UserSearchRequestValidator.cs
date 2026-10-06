using FluentValidation;
using MosquitoApi.Application.DTOs.User;

namespace MosquitoApi.Application.Validators;

/// <summary>
/// Validator for UserSearchRequest
/// </summary>
public class UserSearchRequestValidator : AbstractValidator<UserSearchRequest>
{
    private readonly string[] _allowedSortFields = {
        "username", "email", "firstname", "lastname", "createdat", "lastlogin", "role"
    };

    private readonly string[] _allowedSortDirections = { "asc", "desc" };

    public UserSearchRequestValidator()
    {
        RuleFor(x => x.SearchTerm)
            .Length(2, 100).WithMessage("Search term must be between 2 and 100 characters")
            .When(x => !string.IsNullOrEmpty(x.SearchTerm));

        RuleFor(x => x.RoleType)
            .IsInEnum().WithMessage("Invalid role type")
            .When(x => x.RoleType.HasValue);

        RuleFor(x => x.GenderType)
            .IsInEnum().WithMessage("Invalid gender type")
            .When(x => x.GenderType.HasValue);

        RuleFor(x => x.CreatedAfter)
            .LessThan(DateTime.UtcNow).WithMessage("Created after date cannot be in the future")
            .When(x => x.CreatedAfter.HasValue);

        RuleFor(x => x.CreatedBefore)
            .LessThan(DateTime.UtcNow).WithMessage("Created before date cannot be in the future")
            .When(x => x.CreatedBefore.HasValue);

        RuleFor(x => x.LastLoginAfter)
            .LessThan(DateTime.UtcNow).WithMessage("Last login after date cannot be in the future")
            .When(x => x.LastLoginAfter.HasValue);

        RuleFor(x => x.LastLoginBefore)
            .LessThan(DateTime.UtcNow).WithMessage("Last login before date cannot be in the future")
            .When(x => x.LastLoginBefore.HasValue);

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page number must be greater than 0");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100");

        RuleFor(x => x.SortBy)
            .Must(BeAValidSortField).WithMessage($"Sort field must be one of: {string.Join(", ", _allowedSortFields)}")
            .When(x => !string.IsNullOrEmpty(x.SortBy));

        RuleFor(x => x.SortDirection)
            .Must(BeAValidSortDirection).WithMessage($"Sort direction must be one of: {string.Join(", ", _allowedSortDirections)}")
            .When(x => !string.IsNullOrEmpty(x.SortDirection));

        // Date range validation
        RuleFor(x => x)
            .Must(HaveValidDateRange).WithMessage("Created before date must be after created after date")
            .When(x => x.CreatedAfter.HasValue && x.CreatedBefore.HasValue);

        RuleFor(x => x)
            .Must(HaveValidLastLoginRange).WithMessage("Last login before date must be after last login after date")
            .When(x => x.LastLoginAfter.HasValue && x.LastLoginBefore.HasValue);
    }

    private bool BeAValidSortField(string sortField)
    {
        return _allowedSortFields.Contains(sortField.ToLowerInvariant());
    }

    private bool BeAValidSortDirection(string sortDirection)
    {
        return _allowedSortDirections.Contains(sortDirection.ToLowerInvariant());
    }

    private static bool HaveValidDateRange(UserSearchRequest request)
    {
        return request.CreatedBefore > request.CreatedAfter;
    }

    private static bool HaveValidLastLoginRange(UserSearchRequest request)
    {
        return request.LastLoginBefore > request.LastLoginAfter;
    }
}