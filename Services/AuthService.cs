using BCrypt.Net;
using MedicineInventory.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;

namespace MedicineInventory.Services
{
    public class AuthService:IAuthService
    {
        private readonly IInventoryStore _store;
        private readonly IConfiguration _configuration;

        public AuthService(IWebHostEnvironment environment, IInventoryStore store, IConfiguration configuration)
        {
            _store = store;
            _configuration = configuration;
        }

        public async Task<User> AuthenticateUser(UserLoginRequest login)
        {
            var data = await _store.ReadDataAsync();
            var user= data.Users.FirstOrDefault(x => x.Name == login.Name);
            bool isPasswordCorrect = BCrypt.Net.BCrypt.Verify(login.Password, user.Password);
            if (user == null || !isPasswordCorrect)
            {
                return null;
            }
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["ApplicationSettings:JWT_Secret"]);

            var tokenDescriptor= new SecurityTokenDescriptor
            {
                Subject = new System.Security.Claims.ClaimsIdentity(new[]
                {
                    new System.Security.Claims.Claim("id", user.Id.ToString()),
                    new System.Security.Claims.Claim("name", user.Name)
                }),
                IssuedAt = DateTime.UtcNow,
                Issuer = _configuration["ApplicationSettings:JWT_Issuer"],
                Audience= _configuration["ApplicationSettings:JWT_Audience"],
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            user.Token= tokenHandler.WriteToken(token);
            return user;
        }

        public async Task<User> RegisterUser(User login)
        { 
           var data = await _store.ReadDataAsync();
            if (data.Users.Any(x => x.Name == login.Name && x.Id== login.Id))
            {
                return null;
            }
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(login.Password);
            login.Password = hashedPassword;
            login.DOJ= DateTime.UtcNow.ToString("yyyy-MM-dd");
            data.Users.Add(login);
            await _store.WriteDataInternalAsync(data);
            return login;
        }
    }
}
