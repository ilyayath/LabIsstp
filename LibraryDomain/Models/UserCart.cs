using System;
using System.Collections.Generic;

namespace ShopDomain.Models;

public partial class UserCart : Entity
{


    public string UserId { get; set; } = null!;

    public int MerchandiseId { get; set; }

    public int Quantity { get; set; }

    public decimal Price { get; set; }
    public virtual Merchandise Merchandise { get; set; } = null!;
}
