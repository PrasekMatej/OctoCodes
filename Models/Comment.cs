using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OctoCodes.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }
        public Article Article { get; set; }
        public string Author { get; set; }
        public string Text { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public Comment ParentComment { get; set; }
        [NotMapped] 
        public IEnumerable<Comment> SubComments { get; set; }
    }
}
