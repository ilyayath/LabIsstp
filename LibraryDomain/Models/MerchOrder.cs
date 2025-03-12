using System;
using System.Collections.Generic;

namespace ShopDomain.Models;

public partial class MerchOrder : Entity
{


    public int? BuyerId { get; set; }

    public int? ShipmentId { get; set; }

    public int? PaymentId { get; set; }

    public int? StatusId { get; set; }

    public DateTime? OrderDate { get; set; }

    public virtual Buyer? Buyer { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual Payment? Payment { get; set; }

    public virtual Shipment? Shipment { get; set; }

    public virtual OrderStatus? Status { get; set; }
}
