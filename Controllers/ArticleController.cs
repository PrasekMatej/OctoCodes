using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OctoCodes.Data;
using OctoCodes.Models;

namespace OctoCodes.Controllers
{
    public class ArticleController : Controller
    {
        private readonly OctoCodesContext ctx;

        public ArticleController(OctoCodesContext ctx)
        {
            this.ctx = ctx;
        }
        public IActionResult Index()
        {  
            return View(ctx.Articles.OrderByDescending(a => a.CreatedDate));
        }

        public async Task<IActionResult> Article(int? id)
        {
            if (id == null)
                return NotFound();

            var article = await ctx.Articles.FirstOrDefaultAsync(a => a.Id == id);
            if (article == null)
                return NotFound();
            _ = IncrementArticleViewsAsync(article);
            return View(article);
        }

        private async Task IncrementArticleViewsAsync(Article article)
        {
            article.Views++;
            ctx.Articles.Update(article);
            await ctx.SaveChangesAsync();
        }
    }
}
