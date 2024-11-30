﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using AutoMapper;

namespace Order.Models
{
    public class Event
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public bool Status { get; set; }
        public int? ContextId { get; set; }
        public int? Priority { get; set; }
        public DateOnly? CalendarDate { get; set; }
        public bool? IsPrivate { get; set; }
        [Required]
        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        [JsonIgnore]
        public User? User { get; set; } 
        [ForeignKey("ContextId")]
        public Context? Context { get; set; } 
        public List<int>? TaskIds { get; set; } // Список привязанных задач (по id)
    }
}
