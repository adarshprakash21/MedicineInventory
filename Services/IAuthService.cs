using MedicineInventory.Models;

namespace MedicineInventory.Services
{
    public interface IAuthService
    {
        Task<User> AuthenticateUser(UserLoginRequest login);
        Task<User> RegisterUser(User login);
    }
}
