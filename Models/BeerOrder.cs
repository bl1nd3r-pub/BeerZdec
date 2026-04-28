using System;
using System.Collections.Generic;

namespace BeerZdec.Models;

public partial class BeerOrder
{
    public int BeerOrder_ID { get; set; }

    public int? BeerOrder_Seller { get; set; }

    public int? BeerOrder_Employee { get; set; }

    public DateTime? BeerOrder_Datetime { get; set; }

    public double? BeerOrder_TotalAmount { get; set; }

    public string? BeerOrder_Status { get; set; }

    public DateOnly? BeerOrder_DeliveryDate { get; set; }

    public virtual ICollection<BeerOrderItem> BeerOrderItems { get; set; } = new List<BeerOrderItem>();

    public virtual Employee? BeerOrder_EmployeeNavigation { get; set; }

    public virtual WholesaleCustomer? BeerOrder_SellerNavigation { get; set; }
}
