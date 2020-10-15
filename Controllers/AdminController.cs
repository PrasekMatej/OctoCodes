using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OctoCodes.Data;
using OctoCodes.Models;

namespace OctoCodes.Controllers
{
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

        public IActionResult EditArticle(int? id)
        {
            if (id == null)
                return NotFound();
            var article = ctx.Articles.FirstOrDefault(a => a.Id.Equals(id));
            if (article == null)
                return NotFound();
            ViewData["Image"] = article.Image.Replace('\\', '/');
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
            imageName = imageName.Replace("+", "-");
            imageName = imageName.Replace(" ", "-");
            imageName = Path.Combine("img", imageName);
            var path = Path.Combine(webHostEnvironment.WebRootPath, "img");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            using var fs = new FileStream(Path.Combine(webHostEnvironment.WebRootPath, imageName), FileMode.Create);
            image.CopyTo(fs);
            return imageName;
        }

        public IActionResult Login()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login([Bind("Username, Password")] User user)
        {
            try
            {
                CheckPassword(user.Username, user.Password);
                return new JsonResult("logged in!");
                //return RedirectToAction(nameof(Index));
            }
            catch
            {
                TempData["LoginFailed"] = true;
                return View();
            }
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