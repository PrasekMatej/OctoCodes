using System;
using System.Collections.Generic;
using System.Dynamic;
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

            var model = new ArticleCommentsViewModel
            {
                Article = await ctx.Articles.FirstOrDefaultAsync(a => a.Id == id)
            };

            if (model.Article == null)
                return NotFound();
            _ = IncrementArticleViewsAsync(model.Article);

            model.Comments = ctx.Comments.Where(comment => comment.Article.Equals(model.Article) && comment.ParentComment == null).OrderByDescending(comment => comment.CreatedDateTime);
            foreach (var comment in model.Comments)
            {
                model.Comments = ctx.Comments.Where(subComment =>
                        subComment.Article.Equals(model.Article) && subComment.ParentComment.Id == comment.Id)
                    .OrderBy(subComment => subComment.CreatedDateTime);
            }
            return View(model);
        }

        private async Task IncrementArticleViewsAsync(Article article)
        {
            article.Views++;
            ctx.Articles.Update(article);
            await ctx.SaveChangesAsync();
        }
    }
}
