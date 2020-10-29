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
            await IncrementArticleViewsAsync(model.Article);

            model.Comments = ctx.Comments.Where(comment => comment.Article.Id == model.Article.Id && comment.ParentComment == null).OrderByDescending(comment => comment.CreatedDateTime);
            foreach (var comment in model.Comments)
            {
                comment.SubComments = ctx.Comments.Where(subComment =>
                        subComment.Article.Id == model.Article.Id && subComment.ParentComment.Id == comment.Id)
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

        [HttpPost]
        public IActionResult AddComment([Bind("ArticleId, Author, Text")]ArticleCommentsViewModel viewModel)
        {
            var article = ctx.Articles.First(article => article.Id == viewModel.ArticleId);
            if (article == null)
                return NotFound();

            var comment = new Comment
            {
                CreatedDateTime = DateTime.Now,
                Author = viewModel.Author,
                Article = article,
                Text = viewModel.Text,
            };
            ctx.Comments.Add(comment);
            ctx.SaveChanges();
            return RedirectToAction(nameof(Article), new{id = comment.Article.Id});
        }
    }
}
