using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using MosquitoApi.Application.DTOs;
using MosquitoApi.Application.Interfaces;

namespace MosquitoApi.Api.Endpoints;

/// <summary>
/// Extension methods for mapping mosquito-related endpoints
/// </summary>
public static class MosquitoEndpoints
{
    /// <summary>
    /// Maps mosquito endpoints using minimal APIs
    /// </summary>
    /// <param name="app">Web application</param>
    /// <returns>Web application for chaining</returns>
    public static WebApplication MapMosquitoEndpoints(this WebApplication app)
    {
        var mosquitoGroup = app.MapGroup("/api/mosquitoes")
            .WithTags("Mosquitoes")
            .RequireRateLimiting("ApiLimiter");

        // GET /api/mosquitoes - Get all mosquitoes with pagination
        mosquitoGroup.MapGet("/", GetMosquitoesAsync)
            .WithName("GetMosquitoes")
            .WithSummary("Get all mosquitoes")
            .WithDescription("Retrieves a paginated list of all mosquito records")
            .WithOpenApi();

        // GET /api/mosquitoes/{id} - Get mosquito by ID
        mosquitoGroup.MapGet("/{id:guid}", GetMosquitoByIdAsync)
            .WithName("GetMosquitoById")
            .WithSummary("Get mosquito by ID")
            .WithDescription("Retrieves a specific mosquito record by its unique identifier")
            .WithOpenApi();

        // POST /api/mosquitoes - Create new mosquito
        mosquitoGroup.MapPost("/", CreateMosquitoAsync)
            .WithName("CreateMosquito")
            .WithSummary("Create new mosquito")
            .WithDescription("Creates a new mosquito record")
            .WithOpenApi();

        // PUT /api/mosquitoes/{id} - Update existing mosquito
        mosquitoGroup.MapPut("/{id:guid}", UpdateMosquitoAsync)
            .WithName("UpdateMosquito")
            .WithSummary("Update mosquito")
            .WithDescription("Updates an existing mosquito record")
            .WithOpenApi();

        // DELETE /api/mosquitoes/{id} - Delete mosquito
        mosquitoGroup.MapDelete("/{id:guid}", DeleteMosquitoAsync)
            .WithName("DeleteMosquito")
            .WithSummary("Delete mosquito")
            .WithDescription("Deletes a mosquito record (soft delete)")
            .WithOpenApi();

        // POST /api/mosquitoes/search - Search mosquitoes
        mosquitoGroup.MapPost("/search", SearchMosquitoesAsync)
            .WithName("SearchMosquitoes")
            .WithSummary("Search mosquitoes")
            .WithDescription("Searches mosquito records based on specified criteria")
            .RequireRateLimiting("SearchLimiter")
            .WithOpenApi();

        // GET /api/mosquitoes/statistics - Get mosquito statistics
        mosquitoGroup.MapGet("/statistics", GetMosquitoStatisticsAsync)
            .WithName("GetMosquitoStatistics")
            .WithSummary("Get mosquito statistics")
            .WithDescription("Retrieves comprehensive statistics about mosquito data")
            .WithOpenApi();

        // GET /api/mosquitoes/location/{latitude}/{longitude} - Get mosquitoes by location
        mosquitoGroup.MapGet("/location/{latitude:double}/{longitude:double}", GetMosquitoesByLocationAsync)
            .WithName("GetMosquitoesByLocation")
            .WithSummary("Get mosquitoes by location")
            .WithDescription("Retrieves mosquito records within a specified geographical area")
            .WithOpenApi();

        // POST /api/mosquitoes/validate - Validate mosquito data
        mosquitoGroup.MapPost("/validate", ValidateMosquitoAsync)
            .WithName("ValidateMosquito")
            .WithSummary("Validate mosquito data")
            .WithDescription("Validates mosquito data without creating a record")
            .WithOpenApi();

        return app;
    }

    /// <summary>
    /// Gets all mosquitoes with pagination
    /// </summary>
    private static async Task<IResult> GetMosquitoesAsync(
        IMosquitoService mosquitoService,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await mosquitoService.GetAllAsync(pageNumber, pageSize);
        
        return result.IsSuccess 
            ? Results.Ok(result.Value) 
            : Results.BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Gets a mosquito by ID
    /// </summary>
    private static async Task<IResult> GetMosquitoByIdAsync(
        Guid id,
        IMosquitoService mosquitoService)
    {
        var result = await mosquitoService.GetByIdAsync(id);
        
        return result.IsSuccess 
            ? Results.Ok(result.Value) 
            : Results.NotFound(new { error = result.Error });
    }

    /// <summary>
    /// Creates a new mosquito
    /// </summary>
    private static async Task<IResult> CreateMosquitoAsync(
        CreateMosquitoRequest request,
        IMosquitoService mosquitoService,
        IValidator<CreateMosquitoRequest> validator,
        HttpContext context)
    {
        // Validate request
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(new 
            { 
                error = "Validation failed", 
                errors = validationResult.Errors.Select(e => e.ErrorMessage) 
            });
        }

        // Get user context (placeholder - implement authentication)
        var userId = GetUserId(context);

        var result = await mosquitoService.CreateAsync(request, userId);
        
        return result.IsSuccess 
            ? Results.Created($"/api/mosquitoes/{result.Value!.Id}", result.Value)
            : Results.BadRequest(new { error = result.Error, details = result.Details });
    }

    /// <summary>
    /// Updates an existing mosquito
    /// </summary>
    private static async Task<IResult> UpdateMosquitoAsync(
        Guid id,
        UpdateMosquitoRequest request,
        IMosquitoService mosquitoService,
        IValidator<UpdateMosquitoRequest> validator,
        HttpContext context)
    {
        // Validate request
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(new 
            { 
                error = "Validation failed", 
                errors = validationResult.Errors.Select(e => e.ErrorMessage) 
            });
        }

        // Get user context (placeholder - implement authentication)
        var userId = GetUserId(context);

        var result = await mosquitoService.UpdateAsync(id, request, userId);
        
        return result.IsSuccess 
            ? Results.Ok(result.Value)
            : result.Error.Contains("not found") 
                ? Results.NotFound(new { error = result.Error })
                : Results.BadRequest(new { error = result.Error, details = result.Details });
    }

    /// <summary>
    /// Deletes a mosquito
    /// </summary>
    private static async Task<IResult> DeleteMosquitoAsync(
        Guid id,
        IMosquitoService mosquitoService,
        HttpContext context)
    {
        // Get user context (placeholder - implement authentication)
        var userId = GetUserId(context);

        var result = await mosquitoService.DeleteAsync(id, userId);
        
        return result.IsSuccess 
            ? Results.NoContent()
            : result.Error.Contains("not found")
                ? Results.NotFound(new { error = result.Error })
                : Results.BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Searches mosquitoes based on criteria
    /// </summary>
    private static async Task<IResult> SearchMosquitoesAsync(
        MosquitoSearchRequest request,
        IMosquitoService mosquitoService,
        IValidator<MosquitoSearchRequest> validator)
    {
        // Validate request
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(new 
            { 
                error = "Validation failed", 
                errors = validationResult.Errors.Select(e => e.ErrorMessage) 
            });
        }

        var result = await mosquitoService.SearchAsync(request);
        
        return result.IsSuccess 
            ? Results.Ok(result.Value)
            : Results.BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Gets mosquito statistics
    /// </summary>
    private static async Task<IResult> GetMosquitoStatisticsAsync(
        IMosquitoService mosquitoService)
    {
        var result = await mosquitoService.GetStatisticsAsync();
        
        return result.IsSuccess 
            ? Results.Ok(result.Value)
            : Results.BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Gets mosquitoes by location
    /// </summary>
    private static async Task<IResult> GetMosquitoesByLocationAsync(
        double latitude,
        double longitude,
        IMosquitoService mosquitoService,
        [FromQuery] double radiusKm = 10.0,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await mosquitoService.GetByLocationAsync(latitude, longitude, radiusKm, pageNumber, pageSize);
        
        return result.IsSuccess 
            ? Results.Ok(result.Value)
            : Results.BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Validates mosquito data without creating a record
    /// </summary>
    private static async Task<IResult> ValidateMosquitoAsync(
        CreateMosquitoRequest request,
        IMosquitoService mosquitoService,
        IValidator<CreateMosquitoRequest> validator)
    {
        // Validate request
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(new 
            { 
                valid = false,
                errors = validationResult.Errors.Select(e => e.ErrorMessage) 
            });
        }

        var result = await mosquitoService.ValidateAsync(request);
        
        return result.IsSuccess 
            ? Results.Ok(new { valid = true, message = "Mosquito data is valid" })
            : Results.BadRequest(new { valid = false, error = result.Error, details = result.Details });
    }

    /// <summary>
    /// Gets the user ID from the HTTP context (placeholder for authentication)
    /// </summary>
    private static string? GetUserId(HttpContext context)
    {
        // TODO: Implement proper authentication and extract user ID
        // For now, return correlation ID or a placeholder
        return context.Items["CorrelationId"]?.ToString() ?? "anonymous";
    }
}