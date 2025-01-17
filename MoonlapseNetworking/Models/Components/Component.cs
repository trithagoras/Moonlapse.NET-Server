﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace MoonlapseNetworking.Models.Components
{
    public class Component
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("EntityId")]
        public Entity Entity { get; set; }
    }
}
