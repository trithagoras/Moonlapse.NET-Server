﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoonlapseNetworking.Models.Components
{
    public class Position : Component
    {
        public int X { get; set; }

        public int Y { get; set; }

        [Required]
        [ForeignKey("RoomId")]
        public Room Room { get; set; }
    }
}
