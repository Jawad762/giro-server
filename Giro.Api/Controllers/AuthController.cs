using Microsoft.AspNetCore.Mvc;
using Jose;
using Giro.Api.Data;
using Giro.Api.Models;
using Giro.Api.Dtos.Auth;
using Giro.Api.Interfaces;

namespace Giro.Api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly string _secretKey;

        public AuthController(AppDbContext context, IEmailService emailService, IConfiguration config)
        {
            _context = context;
            _emailService = emailService;
            _secretKey = config["JwtSecret"];
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequestDto req)
        {
            try
            {
                var existingUser = _context.Users.FirstOrDefault(user => user.Email == req.Email);

                if (existingUser == null || !BCrypt.Net.BCrypt.Verify(req.Password, existingUser.Password))
                {
                    return Unauthorized(new ApiErrorResponse<string>
                    {
                        Status = "error",
                        ErrorMessage = "Incorrect Email or Password"
                    });
                }

                var secretKey = Convert.FromBase64String(_secretKey);

                var accessTokenPayload = new Dictionary<string, object>
                {
                    { "id", existingUser.Id },
                    { "email", existingUser.Email },
                    { "firstName", existingUser.FirstName },
                    { "lastName", existingUser.LastName },
                    { "role", existingUser.Role }
                };
                var accessToken = GenerateToken(accessTokenPayload, secretKey);

                var refreshTokenPayload = new Dictionary<string, object>
                {
                    { "id", existingUser.Id },
                    { "email", existingUser.Email },
                    { "role", existingUser.Role }
                };
                var refreshToken = GenerateToken(refreshTokenPayload, secretKey);

                VehicleModel vehicle = null;

                if (existingUser.Role == "driver")
                {
                    vehicle = _context.Vehicles.FirstOrDefault(v => v.DriverId == existingUser.Id);
                }

                return Ok(new LoginResponseDto
                {
                    Jwt = new JwtModel
                    {
                        AccessToken = accessToken,
                        RefreshToken = refreshToken,
                    },
                    User = new UserDto
                    { 
                        Id = existingUser.Id,
                        Email = existingUser.Email,
                        FirstName = existingUser.FirstName,
                        LastName = existingUser.LastName,
                        ProfilePicture = existingUser.ProfilePicture,
                        Role = existingUser.Role,
                        IsConfirmed = existingUser.IsConfirmed
                    },
                    Vehicle = vehicle
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse<string>
                {
                    Status = "error",
                    ErrorMessage = "An unexpected error occurred"
                });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto req)
        {
            try
            {
                if ((req.Role != "rider" && req.Role != "driver") || (req.Role == "driver" && req.Vehicle == null))
                {
                    return StatusCode(400, new ApiErrorResponse<string>
                    {
                        Status = "error",
                        ErrorMessage = "Invalid Input"
                    });
                }

                var existingUser = _context.Users.FirstOrDefault(user => user.Email == req.Email);

                if (existingUser != null)
                {
                    return BadRequest(new ApiErrorResponse<string>
                    {
                        Status = "error",
                        ErrorMessage = "An account with this email already exists"
                    });
                }

                if (string.IsNullOrEmpty(req.Password) || req.Password.Length < 6 || req.FirstName.Length > 30 || req.LastName.Length > 30 || req.FirstName.Length < 2 || req.LastName.Length < 2)
                {
                    return BadRequest(new ApiErrorResponse<string>
                    {
                        Status = "error",
                        ErrorMessage = "Invalid Input"
                    });
                }

                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(req.Password);

                var newUser = new UserModel
                {
                    Email = req.Email,
                    FirstName = req.FirstName,
                    LastName = req.LastName,
                    Password = hashedPassword,
                    Role = req.Role,
                    ProfilePicture = null,
                    IsConfirmed = false,
                };

                _context.Users.Add(newUser);
                _context.SaveChanges();

                VehicleModel newVehicle = null;

                if (req.Role == "driver")
                {
                    newVehicle = new VehicleModel
                    {
                        DriverId = newUser.Id,
                        Type = req.Vehicle.Type,
                        Color = req.Vehicle.Color,
                        LicenseNumber = req.Vehicle.LicenseNumber
                    };
                    try
                    {
                        _context.Vehicles.Add(newVehicle);
                    }
                    catch (Exception ex)
                    {
                        _context.Users.Remove(newUser);
                    }
                }
                _context.SaveChanges();

                var secretKey = Convert.FromBase64String(_secretKey);

                var accessTokenPayload = new Dictionary<string, object>
                {
                    { "id", newUser.Id },
                    { "email", newUser.Email },
                    { "firstName", newUser.FirstName },
                    { "lastName", newUser.LastName },
                    { "role", newUser.Role }
                };
                var accessToken = GenerateToken(accessTokenPayload, secretKey);

                var refreshTokenPayload = new Dictionary<string, object>
                {
                    { "id", newUser.Id },
                    { "email", newUser.Email },
                    { "role", newUser.Role }
                };
                var refreshToken = GenerateToken(refreshTokenPayload, secretKey);

                Random generator = new Random();
                int code = generator.Next(10000, 99999);

                var verificationCode = new VerificationCodeModel
                {
                    Code = code,
                    UserId = newUser.Id
                };

                _context.VerificationCodes.Add(verificationCode);
                _context.SaveChanges();

                await _emailService.SendEmailConfirmationAsync(newUser.Email, code);

                return Ok(new LoginResponseDto
                {
                    Jwt = new JwtModel
                    {
                        AccessToken = accessToken,
                        RefreshToken = refreshToken,
                    },
                    User = new UserDto
                    {
                        Id = newUser.Id,
                        Email = newUser.Email,
                        FirstName = newUser.FirstName,
                        LastName = newUser.LastName,
                        ProfilePicture = newUser.ProfilePicture,
                        Role = newUser.Role,
                        IsConfirmed = newUser.IsConfirmed
                    },
                    Vehicle = newVehicle
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse<string>
                {
                    Status = "error",
                    ErrorMessage = "An unexpected error occurred"
                });
            }
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequestDto req)
        {
            try
            {
                var code = _context.VerificationCodes.FirstOrDefault(e => e.UserId == req.UserId);

                if (code == null || code.Code != req.Code)
                {
                    return BadRequest(new ApiErrorResponse<string>
                    {
                        Status = "error",
                        ErrorMessage = "Invalid Code"
                    });
                }

                // User code is valid --> update user status and delete code record from the db
                var user = await _context.Users.FindAsync(req.UserId);

                if (user == null)
                {
                    return NotFound(new ApiErrorResponse<string>
                    {
                        Status = "error",
                        ErrorMessage = "User not found"
                    });
                }

                user.IsConfirmed = true;

                _context.VerificationCodes.Remove(code);
                await _context.SaveChangesAsync();

                return Ok(new ApiSuccessResponse<string>
                {
                    Status = "success",
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse<string>
                {
                    Status = "error",
                    ErrorMessage = "An unexpected error occurred"
                });
            }
        }

        [HttpPost("refresh")]
        public IActionResult RefreshToken()
        {
            try
            {
                var refreshToken = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ")[1];

                bool isRevoked = _context.RevokedTokens.Any(t => t.Token == refreshToken);

                if (isRevoked)
                {
                    throw new Exception();
                }

                var secretKey = Convert.FromBase64String(_secretKey);

                var payload = JWT.Decode<Dictionary<string, object>>(refreshToken, secretKey);

                var accessToken = GenerateToken(payload, secretKey);

                return Ok(
                    new
                    { 
                        AccessToken = accessToken
                    }
                );
            } catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse<string>
                {
                    Status = "error",
                    ErrorMessage = "An unexpected error occurred"
                });
            }
        }

        [HttpPost("revoke")]
        public IActionResult RevokeToken()
        {
            try
            {
                var refreshToken = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ")[1];

                if (String.IsNullOrEmpty(refreshToken))
                {
                    throw new Exception();
                };

                var secretKey = Convert.FromBase64String(_secretKey);

                var payload = JWT.Decode<Dictionary<string, object>>(refreshToken, secretKey);

                var newToken = new RevokedTokensModel {
                    Token = refreshToken,
                    UserId = Convert.ToInt32(payload["id"])
                };

                _context.RevokedTokens.Add(newToken);
                _context.SaveChanges();

                return Ok(
                    new
                    {
                        status = "success"
                    }
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse<string>
                {
                    Status = "error",
                    ErrorMessage = "An unexpected error occurred"
                });
            }
        }
        private string GenerateToken(Dictionary<string, object> payload, byte[] secretKey)
        {
            var issuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            payload["issuedAt"] = issuedAt;

            return JWT.Encode(payload, secretKey, JwsAlgorithm.HS256);
        }
    }
}
