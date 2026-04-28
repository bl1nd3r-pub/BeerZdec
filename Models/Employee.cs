using System;
using System.Collections.Generic;

namespace BeerZdec.Models;

public partial class Employee
{
    public int Emp_ID { get; set; }

    public int Emp_Position { get; set; }

    public string? Emp_FirstName { get; set; }

    public string? Emp_SecName { get; set; }

    public string? Emp_LastName { get; set; }

    public string? Emp_Passport { get; set; }

    public string? Emp_INN { get; set; }

    public string? Emp_Phone { get; set; }

    public string? Emp_Email { get; set; }

    public DateOnly? Emp_HireDate { get; set; }

    public virtual ICollection<BeerOrder> BeerOrders { get; set; } = new List<BeerOrder>();

    public virtual ICollection<BrewingBatch> BrewingBatches { get; set; } = new List<BrewingBatch>();

    public virtual EmpPosition Emp_PositionNavigation { get; set; } = null!;

    public virtual ICollection<MaltProcess> MaltProcesses { get; set; } = new List<MaltProcess>();

    public virtual ICollection<RetailStore> RetailStores { get; set; } = new List<RetailStore>();

    public virtual ICollection<StorageMove> StorageMoves { get; set; } = new List<StorageMove>();
}
