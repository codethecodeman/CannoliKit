﻿using DisCannoli.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DisCannoli.Models
{
    [Index(nameof(Type))]
    [Index(nameof(StateId))]
    public sealed class Route
    {
        [Key]
        public string RouteId { get; set; } = null!;

        [Required]
        public RouteType Type { get; set; }

        [Required]
        public string CallbackType { get; set; } = null!;

        [Required]
        public string CallbackMethod { get; set; } = null!;

        [Required]
        public string StateId { get; set; } = null!;

        [ForeignKey(nameof(StateId))]
        public SaveState SaveState { get; set; } = null!;

        public string? StateIdToBeDeleted { get; set; } = null!;

        [Required]
        public Priority Priority { get; set; }

        public string? Parameter1 { get; set; }

        public string? Parameter2 { get; set; }

        public string? Parameter3 { get; set; }
    }
}
