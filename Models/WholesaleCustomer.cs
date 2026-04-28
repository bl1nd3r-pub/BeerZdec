using System;
using System.Collections.Generic;

namespace BeerZdec.Models;

public partial class WholesaleCustomer
{
    public int Customer_ID { get; set; }

    public string Customer_CompanyName { get; set; } = null!;

    public string? Customer_INN { get; set; }

    public string? Customer_ContactPerson { get; set; }

    public string? Customer_Phone { get; set; }

    public string? Customer_Email { get; set; }

    public string? Customer_DeliveryAddress { get; set; }

    public bool? Customer_IsActive { get; set; }

    public virtual ICollection<BeerOrder> BeerOrders { get; set; } = new List<BeerOrder>();
}
