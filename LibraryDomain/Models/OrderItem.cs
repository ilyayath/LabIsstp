using System;
using System.Collections.Generic;

namespace ShopDomain.Models;

public partial class OrderItem : Entity
{
    public int OrderId { get; set; }

    public int MerchId { get; set; }

    public int Quantity { get; set; }


    public decimal Price { get; set; }
    public virtual Merchandise Merch { get; set; } = null!;

    public virtual MerchOrder Order { get; set; } = null!;
}
