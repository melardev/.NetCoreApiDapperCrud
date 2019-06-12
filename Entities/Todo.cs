using System;
using System.ComponentModel.DataAnnotations;

namespace ApiCoreDapperCrud.Entities
{
    public class Todo
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        [Required(ErrorMessage = "Completed flag must be set")]
        public bool Completed { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }
    }
}