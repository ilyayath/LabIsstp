using System;
using System.Collections.Generic;

namespace ShopDomain.Models;

public partial class Team : Entity
{


    public string TeamName { get; set; } = null!;

    public virtual ICollection<Merchandise> Merchandises { get; set; } = new List<Merchandise>();
}
