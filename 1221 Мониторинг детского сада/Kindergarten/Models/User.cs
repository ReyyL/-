using Kindergarten.Helpers;
using Microsoft.AspNetCore.Http;
using System.Data;
using System.Linq;

namespace Kindergarten.Models
{
    public class User: Model
    {
        public string Login { get; set; } = "";
        public string Password { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string SecondName { get; set; } = "";
        public string Patronymic { get; set; } = "";
        public Role Role { get; set; }
        public User()
        {

        }

        public User(DataRow row)
        {
            var items = row.ItemArray;
            Id = int.Parse($"{items[0]}");
            Login = $"{items[1]}";
            Password = $"{items[2]}";
            FirstName = $"{items[3]}";
            SecondName = $"{items[4]}";
            Patronymic = $"{items[5]}";
            int roleId = int.Parse($"{items[6]}");
            Role = Database.Roles.Where(x => x.Id == roleId).FirstOrDefault();
        }

        #region Переопределенные свойства

        public override string UpdateValues => $"[Login] = N'{Login}', [Password] = N'{Password}', [FirstName] = N'{FirstName}', " +
            $"[SecondName] = N'{SecondName}', [Patronymic] = N'{Patronymic}', [RoleId] ={Role.Id}";

        public override string InsertValues => $"N'{Login}', N'{Password}', '{FirstName}', '{SecondName}', '{Patronymic}', {Role.Id}";

        #endregion

        #region Вспомогательные методы

        public void Save(HttpResponse response)
        {
            response.Cookies.Append("UserLogin", Login);
            response.Cookies.Append("UserToken", Password);
        }

        public static bool Exists(HttpRequest request, string password, string login)
        {
            try
            {
                var realLogin = request.Cookies["UserLogin"];
                var realPassword = request.Cookies["UserToken"];
                if (realPassword == password && realLogin == login)
                {
                    return true;
                }
                else
                {
                    throw new System.Exception();
                }    
            }
            catch
            {
                return false;
            }
            
        }

        public static User Current(HttpRequest request)
        {
            return Database.Users.Where(x => x.Password == request.Cookies["UserToken"] && x.Login == request.Cookies["UserLogin"]).FirstOrDefault();
        }

        public static void Clear(HttpResponse response)
        {
            response.Cookies.Append("UserToken", "NULL");
        }

        #endregion
    }
}
