using System;
using System.Collections.Generic;

namespace BeerZdec.Models;

public partial class RetailStore
{
    public int RetailStore_ID { get; set; }

    public int? RetailStore_Manager { get; set; }

    public string? RetailStore_Name { get; set; }

    public string? RetailStore_Address { get; set; }

    public bool? RetailStore_IsActive { get; set; }

    public virtual Employee? RetailStore_ManagerNavigation { get; set; }
}
