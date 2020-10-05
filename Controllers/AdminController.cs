using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OctoCodes.Data;

namespace OctoCodes.Controllers
{
    public class AdminController : Controller
    {
        private readonly OctoCodesContext ctx;

        public AdminController(OctoCodesContext ctx)
        {
            this.ctx = ctx;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public string Login(IFormCollection collection)
        {
            return "Successfully logged in";
            //try
            //{
            //    ctx.Users.FirstOrDefault(user => user.Username.Equals())
            //    return RedirectToAction(nameof(Index));
            //}
            //catch
            //{
            //    return View();
            //}
        }

        public static string EncryptPassword(string password)
        {
            using var sha = new SHA512Managed();
            return sha.ComputeHash(Encoding.UTF8.GetBytes(password)).ToString();
        }
    }
}