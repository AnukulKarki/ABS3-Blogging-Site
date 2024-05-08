﻿using ABS3.DTO;
using ABS3.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ABS3.Services;

namespace ABS3.Controllers
{

    [Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
    [ApiController]
    public class UserController: ControllerBase
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUser()
        {
            if (_context.Users == null)
            {
                return NotFound();
            }
            return await _context.Users.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUserId(int id)
        {
            if (_context.Users == null)
            {
                return NotFound();
            }
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        [HttpPost]
        public async Task<ActionResult<User>> PostUser(UserDto user)
        {

            var passwordHash = Hash.HashPassword(user.Password);
            

            User userObj = new User()
            {
                Name = user.Name,
                Email = user.Email,
                Password = passwordHash,
                Phone = user.Phone,
                role = user.role
            };


            _context.Users.Add(userObj);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> PutUser(int id, User user)
        {
            if (id != user.Id)
            {
                return BadRequest();
            }

            _context.Entry(user).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserAvailable(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return Ok();
        }

        private bool UserAvailable(int id)
        {
            return (_context.Users?.Any(x => x.Id == id)).GetValueOrDefault();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if(_context.Users == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Ok();
        }
        [Authorize]
        [HttpPut("UserProfilePassword")]
        public async Task<ActionResult> ChangePassword(string currentPassword, string newPassword)
        {
            var currentPasswordHash = Hash.HashPassword(currentPassword);
            var newPasswordHash = Hash.HashPassword(newPassword);
            var userId = User.Claims.FirstOrDefault(claim => claim.Type == "UserId")?.Value;

            var user = _context.Users.FirstOrDefault(u => u.Id == int.Parse(userId));
            if (user == null)
            {
                return NotFound();
            }

            if(user.Password != currentPasswordHash)
            {
                return BadRequest("Password Does not match");
            }
            user.Password = newPasswordHash;
            await _context.SaveChangesAsync();
            return Ok("Password Changed Successfully");

            
        }


    }
}
