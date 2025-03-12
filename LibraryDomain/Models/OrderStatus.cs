using System;
using System.Collections.Generic;

namespace ShopDomain.Models;

public partial class OrderStatus : Entity
{


    public string StatusName { get; set; } = null!;

    public virtual ICollection<MerchOrder> MerchOrders { get; set; } = new List<MerchOrder>();
}
