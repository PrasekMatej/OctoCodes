using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGeneration.Contracts.Messaging;
using OctoCodes.Data;
using OctoCodes.Models;

namespace OctoCodes.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly OctoCodesContext ctx;
        private readonly IWebHostEnvironment webHostEnvironment;

        public AdminController(OctoCodesContext ctx, IWebHostEnvironment webHostEnvironment)
        {
            this.ctx = ctx;
            this.webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            return View(ctx.Articles.OrderByDescending(article => article.CreatedDate));
        }

        public async Task<IActionResult> EditArticle(int? id)
        {
            if (id == null)
                return NotFound();
            var article = await ctx.Articles.FirstOrDefaultAsync(a => a.Id.Equals(id));
            if (article == null)
                return NotFound();
            return View(article);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditArticle(int id, [Bind("Author, Title, Text, ImageFile, Category")]
            Article article)
        {
            if (id != article.Id)
                return NotFound();

            var savedArticle = ctx.Articles.FirstOrDefault(a => a.Id.Equals(id));

            if (savedArticle == null)
                return NotFound();

            if (!ModelState.IsValid)
                return View(article);

            article.Image = 
                article.ImageFile == null ? 
                savedArticle.Image : 
                SaveImage(article.ImageFile);
            article.CreatedDate = savedArticle.CreatedDate;
            article.Views = savedArticle.Views;

            ctx.Articles.Update(article);
            ctx.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult NewArticle()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult NewArticle([Bind("Author, Title, Text, ImageFile, Category")]
            Article article)
        {
            if (!ModelState.IsValid)
                return View(article);

            article.Image = SaveImage(article.ImageFile);
            article.CreatedDate = DateTime.Now;
            article.Views = 0;

            ctx.Articles.Add(article);
            ctx.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public ActionResult UploadImage(IFormFile upload)
        {
            if (upload.Length <= 0) return null;
            var filename = SaveImage(upload);
            var url = $"{Url.Content("~/")}{filename}";
            var success = new {url};
            return Json(success);
        }

        private string SaveImage(IFormFile image)
        {
            var imageName = GetImageName(image);
            imageName = imageName
                .Replace("+", "-")
                .Replace(" ", "-");
            imageName = "img/" + imageName;
            var path = Path.Combine(webHostEnvironment.WebRootPath, "img");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            using var fs = new FileStream(Path.Combine(webHostEnvironment.WebRootPath, imageName), FileMode.Create);
            image.CopyTo(fs);
            return imageName;
        }

        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Login([Bind("Username, Password")] User user)
        {
            if (string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Password))
                return View(user);
            try
            {
                CheckPassword(user.Username, user.Password);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username)
                };
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                TempData["LoginFailed"] = true;
                return View(user);
            }
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        public async Task<IActionResult> UserManagement()
        {
            return View(await ctx.Users.ToListAsync());
        }

        public IActionResult CreateUser()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser([Bind("Username,Password,ConfirmPassword")] User user)
        {
            if (!ModelState.IsValid) return View(user);
            try
            {
                user.Password = EncryptPassword(user.Password);
                ctx.Add(user);
                await ctx.SaveChangesAsync();
                return RedirectToAction(nameof(UserManagement));
            }
            catch (DbUpdateException e)
            {
                TempData["ErrorMessage"] = e.InnerException.Message.Contains("Cannot insert duplicate key")
                    ? $"Uživatel se jménem {user.Username} již existuje"
                    : "Chyba pøi uložení dat do databáze";
                return View(user);
            }
            catch (Exception e)
            {
                TempData["ErrorMessage"] = "Chyba pøi vytváøení uživatele";
                return View(user);
            }
        }

        public async Task<IActionResult> ChangePassword(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await ctx.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string id, [Bind("Username,Password,ConfirmPassword")] User user)
        {
            if (id != user.Username)
            {
                return NotFound();
            }

            if (!ModelState.IsValid) 
                return View(user);

            try
            {
                user.Password = EncryptPassword(user.Password);
                ctx.Update(user);
                await ctx.SaveChangesAsync();
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Zmìna hesla se nezdaøila";
            }
            return RedirectToAction(nameof(UserManagement));
        }

        public async Task<IActionResult> DeleteUser(string username)
        {
            try
            {
                var user = await ctx.Users.FindAsync(username);
                ctx.Users.Remove(user);
                await ctx.SaveChangesAsync();
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Odstranìní se nezdaøilo";
            }
            return RedirectToAction(nameof(UserManagement));
        }

        public static string EncryptPassword(string password)
        {
            byte[] salt;
                new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);

                var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000);
                byte[] hash = pbkdf2.GetBytes(20);

                byte[] hashBytes = new byte[36];
                Array.Copy(salt, 0, hashBytes, 0, 16);
                Array.Copy(hash, 0, hashBytes, 16, 20);

            return Convert.ToBase64String(hashBytes);
        }

        private void CheckPassword(string username, string password)
        {
            var user = ctx.Users.Single(u => u.Username == username);
            if (user == null)
                    throw new UnauthorizedAccessException();

            /* Fetch the stored value */
            string savedPasswordHash = user.Password;
            /* Extract the bytes */
            byte[] hashBytes = Convert.FromBase64String(savedPasswordHash);
            /* Get the salt */
            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);
            /* Compute the hash on the password the user entered */
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000);
            byte[] hash = pbkdf2.GetBytes(20);
            /* Compare the results */
            for (int i = 0; i < 20; i++)
                if (hashBytes[i + 16] != hash[i])
                    throw new UnauthorizedAccessException();
        }

        private string GetImageName(IFormFile image)
        {
            var filename = Path.GetFileNameWithoutExtension(image.FileName);
            var extension = Path.GetExtension(image.FileName);
            filename = filename + DateTime.Now.ToString("yyyyMMddHHmmss") + extension;
            return filename;
        }
    }
}