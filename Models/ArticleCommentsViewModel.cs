using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OctoCodes.Models
{
    public class ArticleCommentsViewModel
    {
        public Article Article { get; set; }
        public IEnumerable<Comment> Comments { get; set; }
    }
}
