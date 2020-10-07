using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace OctoCodes.Models
{
    public class Article
    {
        [Key]
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Author { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public string Image { get; set; }
        public int Views { get; set; }
        public string Category { get; set; }
        [NotMapped] 
        public IFormFile ImageFile { get; set; }
    }
}
