using Application.Abstractions.Identity;
using Application.Abstractions.Security;
using Application.Common;
using Application.Features.Auth.LoginUser;
using Application.Features.Auth.RegisterUser;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests.Application;

public class AuthHandlersTests
{
    [Fact]
    public async Task RegisterUser_Should_Assign_Role()
    {
        var identity = new Mock<IIdentityService>();
        var userId = Guid.NewGuid();

        identity.Setup(i => i.CreateUserAsync("u@mail.com", "u@mail.com", "P!"))
                .ReturnsAsync(Result<Guid>.Success(userId));
        identity.Setup(i => i.AssignRoleAsync(userId, "User"))
                .Returns(Task.CompletedTask)
                .Verifiable();

        var h = new RegisterUserCommandHandler(identity.Object);
        var id = await h.Handle(new RegisterUserCommand("u@mail.com", "P!", null, false), default);

        id.Should().Be(userId);
        identity.Verify();
    }

    [Fact]
    public async Task LoginUser_Should_Return_Token()
    {
        var identity = new Mock<IIdentityService>();
        var jwt = new Mock<IJwtTokenGenerator>();
        var uid = Guid.NewGuid();

        identity.Setup(i => i.GetUserIdByEmailAsync("u@mail.com")).ReturnsAsync(uid);
        identity.Setup(i => i.CheckPasswordAsync(uid, "P!")).ReturnsAsync(true);
        identity.Setup(i => i.GetUserRolesAsync(uid)).ReturnsAsync(new List<string> { "User" });
        identity.Setup(i => i.GetUserProfileAsync(uid)).ReturnsAsync((uid, "u", "u@mail.com"));

        jwt.Setup(j => j.CreateToken(uid, "u", "u@mail.com", It.IsAny<IEnumerable<string>>()))
           .Returns("TOKEN");

        var h = new LoginUserCommandHandler(identity.Object, jwt.Object);
        var token = await h.Handle(new LoginUserCommand("u@mail.com", "P!"), default);

        token.Should().Be("TOKEN");
    }
}
