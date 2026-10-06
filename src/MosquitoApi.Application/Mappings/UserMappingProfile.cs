using AutoMapper;
using MosquitoApi.Application.DTOs.User;
using MosquitoApi.Domain.Entities;

namespace MosquitoApi.Application.Mappings;

/// <summary>
/// AutoMapper profile for User entity mappings
/// </summary>
public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        // User entity to UserDto mapping
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.IsLocked, opt => opt.MapFrom(src => src.LockoutEndDateUtc.HasValue && src.LockoutEndDateUtc > DateTime.UtcNow));

        // CreateUserRequest to User entity mapping
        CreateMap<CreateUserRequest, User>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
            .ForMember(dest => dest.Password, opt => opt.Ignore()) // Will be set after hashing
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.FailedPasswordAttemptCount, opt => opt.MapFrom(src => 0))
            .ForMember(dest => dest.IsEmailVerified, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.Tokens, opt => opt.Ignore());

        // UpdateUserRequest to User entity mapping (for partial updates)
        CreateMap<UpdateUserRequest, User>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}