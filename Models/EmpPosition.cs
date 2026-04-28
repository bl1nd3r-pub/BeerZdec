using System;
using System.Collections.Generic;

namespace BeerZdec.Models;

public partial class EmpPosition
{
    public int EmpPosition_ID { get; set; }

    public string? EmpPos_Name { get; set; }

    public decimal? Emp_BaseSalary { get; set; }

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
