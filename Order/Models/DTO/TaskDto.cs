﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Order.Models.DTO
{
    public class TaskDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public DateOnly? HardDeadline { get; set; }
        public DateOnly? SoftDeadline { get; set; }
        public bool? Status { get; set; }
        public int? ContextId { get; set; }
        public int? Priority { get; set; }
        [Column(TypeName = "uuid")]
        public Guid? UserId { get; set; }
        public int? EventId { get; set; }
        public DateOnly? CalendarDate { get; set; }
        public bool? IsPrivate { get; set; }
        public int? ProjectId { get; set; }

        [ForeignKey("UserId")]
        [JsonIgnore]
        public User? User { get; set; }
        [ForeignKey("ContextId")]
        public Context? Context { get; set; }
        [ForeignKey("EventId")]
        public Event? Event { get; set; }
    }
}
