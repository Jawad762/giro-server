using Giro.Api.Data;
using Giro.Api.Dtos.Users;
using Giro.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Giro.Api.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        public UsersController(AppDbContext context) {
            _context = context;
        }

        [HttpPut("edit")]
        public async Task<IActionResult> Edit([FromBody] UpdateUserDto req)
        {
            if (HttpContext.Items.TryGetValue("User", out var userObj))
            {
                var user = userObj as UserModel;

                if (user.Role == "driver" && req.Vehicle == null)
                {
                    return BadRequest(new ApiErrorResponse<string>
                    {
                        Status = "error",
                        ErrorDetails = "Vehicle information is required for drivers."
                    });
                }

                if (req.FirstName.Length > 30 || req.LastName.Length > 30 || req.FirstName.Length < 2 || req.LastName.Length < 2)
                {
                    return BadRequest(new ApiErrorResponse<string>
                    {
                        Status = "error",
                        ErrorDetails = "Invalid Input"
                    });
                }

                var selectedUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
                if (selectedUser == null)
                {
                    return NotFound(new ApiErrorResponse<string>
                    {
                        Status = "error",
                        ErrorDetails = "User not found."
                    });
                }

                selectedUser.FirstName = req.FirstName;
                selectedUser.LastName = req.LastName;
                selectedUser.ProfilePicture = req.ProfilePicture;

                if (selectedUser.Role == "driver")
                {
                    var userVehicle = await _context.Vehicles.FirstOrDefaultAsync(vehicle => vehicle.DriverId == selectedUser.Id);
                    if (userVehicle == null) throw new Exception();

                    userVehicle.Type = req.Vehicle.Type;
                    userVehicle.Color = req.Vehicle.Color;
                    userVehicle.LicenseNumber = req.Vehicle.LicenseNumber;
                }

                await _context.SaveChangesAsync();

                return Ok(new ApiSuccessResponse<string>
                {
                    Status = "success",
                    Message = "Successfully updated info."
                });
            }
            else
            {
                return BadRequest(new ApiErrorResponse<string>
                {
                    Status = "error",
                    ErrorDetails = "An unexpected error occurred."
                });
            }
        }

    }
}
