using System;
using System.Collections.Generic;

namespace ShopDomain.Models;

public partial class Payment : Entity
{


    public string TypePayment { get; set; } = null!;

    public virtual ICollection<MerchOrder> MerchOrders { get; set; } = new List<MerchOrder>();
}
