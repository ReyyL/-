using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Kindergarten.Enums;
using Kindergarten.Helpers;
using Kindergarten.Models;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using System;

namespace Kindergarten.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login()
        {
            //GenerateData(15, 9);

            return View("Login", new User());
        }

        [Obsolete]
        private void GenerateData(int childCount, int groupCount)
        {
            Random random = new Random();

            //генерация групп
            List<Group> groups = new List<Group>();
            for (int i = 0; i < groupCount; i++)
            {
                var type = Database.GroupTypes[random.Next(4)];
                var count = groups.Where(x => x.Type.Id == type.Id).Count();
                var group = new Group() 
                { 
                    Name = $"Группа {count + 1}", 
                    Type = type,
                    Mentor = Database.Users[random.Next(Database.Users.Count)]
                };
                groups.Add(group);
                Database.Insert(groups.Last(), "[Group]");
            }

            //генерация детей
            List<string> names = new List<string>() { "Артём", "Александр", "Михаил", "Максим", "Иван", "София", "Виктория", "Анна", "Мария", "Алиса" };
            List<string> secondNames = new List<string>() { "Ли", "Чжан", "Ван", "Нгуен", "Гарсия", "Гонсалес", "Эрнандес", "Смиты", "Смирновы" };

            groups = Database.Groups;

            for (int i = 0; i < childCount * groupCount; i++)
            {
                Child temp = new Child()
                {
                    FirstName = names[random.Next(names.Count)],
                    SecondName = secondNames[random.Next(secondNames.Count)],
                    Group = groups[i % groupCount],
                    Age = random.Next(4,8)
                };
                Database.Insert(temp, "[Child]");
            }


        }


        [HttpPost]
        public IActionResult Login(string password, string login)
        {
            User current = new User()
            {
                Password = GetHash(password),
                Login = login
            };

            var resultByLogin = Database.Users.Where(x => x.Login == current.Login);
            var resultByPassword = resultByLogin.Where(x => x.Password == current.Password).FirstOrDefault();

            if (resultByPassword != null)
            {
                resultByPassword.Save(Response);
                return RedirectToAction("Index", "Account");
            }
            else if (resultByLogin.Count() > 0)
            {
                ModelState.AddModelError("login", "Введен неправильный пароль");
            }
            else
            {
                ModelState.AddModelError("login", "Пользователь с таким именем не найден");
            }
            return View("Login", current);
        }

        private static string GetHash(string password)
        {
            if (!string.IsNullOrEmpty(password))
            {
                var crypt = new SHA256Managed();
                string hash = "";
                byte[] crypto = crypt.ComputeHash(Encoding.ASCII.GetBytes(password));
                foreach (byte theByte in crypto)
                {
                    hash += theByte.ToString("x2");
                }
                return hash;
            }

            return "";
        }
    }
}
