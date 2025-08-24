using Application.Features.Auth;
using Application.Features.Auth.RegisterUser;
using Application.Features.Auth.LoginUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator) => _mediator = mediator;

    public record RegisterRequest(string Email, string Password, string? UserName, bool AsAdmin = false);
    public record LoginRequest(string Email, string Password);

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        var result = await _mediator.Send(new RegisterUserCommand(req.Email, req.Password, req.UserName, req.AsAdmin));
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var result = await _mediator.Send(new LoginUserCommand(req.Email, req.Password));
        return Ok(result);
    }
}
