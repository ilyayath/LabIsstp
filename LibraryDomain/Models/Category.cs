using System;
using System.Collections.Generic;

namespace ShopDomain.Models;

public partial class Category : Entity
{


    public string CategoryName { get; set; } = null!;

    public virtual ICollection<Merchandise> Merchandises { get; set; } = new List<Merchandise>();
}
