using AutoMapper;
using BusTrackingAPI.Mappings;
using BusTrackingAPI.Models;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace BusTrackingAPI.Tests;

internal static class TestHelpers
{
    public static IMapper CreateMapper()
    {
        return new MapperConfiguration(config =>
            config.AddProfile<MappingProfile>()).CreateMapper();
    }

    public static IHttpContextAccessor CreateHttpContext(
        int userId = 1,
        UserRole role = UserRole.Passenger)
    {
        var identity = new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, role.ToString())
            },
            "TestAuthentication");

        return new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        };
    }
}
