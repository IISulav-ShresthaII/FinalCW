using System.Security.Cryptography;
using System.Text;
using FinalCoffee1.Common.model;

namespace FinalCoffee1.common.helperServices;
public class AuthService
{
    public string GenerateHash(string data)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
        }
    }

    public bool Authenticate(UserModel existingData, UserModel comingData)
    {
        var hashedPassword = this.GenerateHash(comingData.Password);
        if (existingData.Username == comingData.Username && existingData.Password == hashedPassword)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
