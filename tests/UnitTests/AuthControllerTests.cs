using Api.Controllers;
using Application.Features.Auth;
using Application.Features.Auth.LoginUser;
using Application.Features.Auth.RegisterUser;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests.Api;

public class AuthControllerTests
{
    [Fact]
    public async Task Register_Returns_Ok_With_NewUserId()
    {
        var mediator = new Mock<IMediator>();
        var newId = Guid.NewGuid();

        mediator.Setup(m => m.Send(It.IsAny<RegisterUserCommand>(), default))
                .ReturnsAsync(newId);

        var c = new AuthController(mediator.Object);
        var res = await c.Register(new AuthController.RegisterRequest("a@mail.com", "Passw0rd!", null, false));

        var ok = res as OkObjectResult;
        ok!.Value.Should().Be(newId);
    }

    [Fact]
    public async Task Login_Returns_Ok_With_Token()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<LoginUserCommand>(), default))
                .ReturnsAsync("JWT_TOKEN");

        var c = new AuthController(mediator.Object);
        var res = await c.Login(new AuthController.LoginRequest("a@mail.com", "Passw0rd!"));

        var ok = res as OkObjectResult;
        ok!.Value.Should().Be("JWT_TOKEN");
    }
}
