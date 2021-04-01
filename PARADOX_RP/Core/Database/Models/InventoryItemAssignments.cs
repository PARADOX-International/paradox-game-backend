﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PARADOX_RP.Core.Database.Models
{
    [Table("inventory_item_assignments")]
    class InventoryItemAssignments
    {
        public int Id { get; set; }
        public int InventoryId { get; set; }
        public virtual Inventories Inventory { get; set; }

        public string Item { get; set; }
        public string Attribute { get; set; }
        public float Weight { get; set; }
        public int Slot { get; set; }
    }
}