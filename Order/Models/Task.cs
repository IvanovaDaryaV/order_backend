﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Order.Models
{
    public class Task
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        public DateOnly HardDeadline { get; set; }
        public DateOnly SoftDeadline { get; set; }
        [Required]
        public bool Status { get; set; }
        public int ContextId { get; set; }
        public int Priority { get; set; }
        [Required]
        [Column(TypeName = "uuid")]
        public Guid UserId { get; set; }
        public int EventId { get; set; }
        public DateOnly CallendarDate { get; set; }
        public bool IsPrivate { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; } //навигационное свойство
        [ForeignKey("ContextId")]
        public Context Context { get; set; } //навигационное свойство
        [ForeignKey("EventId")]
        public Event Event { get; set; } //навигационное свойство
    }
}