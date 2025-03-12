using System;
using System.Collections.Generic;

namespace ShopDomain.Models;

public partial class Shipment : Entity
{


    public string TypeShipment { get; set; } = null!;

    public virtual ICollection<MerchOrder> MerchOrders { get; set; } = new List<MerchOrder>();
}
