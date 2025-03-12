using System;
using System.Collections.Generic;

namespace ShopDomain.Models;

public class Buyer : Entity
{
    public string UserId { get; set; } = null!;
    public virtual AppUser User { get; set; } = null!;

    public int? CityId { get; set; }
    public string? Address { get; set; }
    public string Username { get; set; } = string.Empty; // ✅ Додано Username
    public virtual City? City { get; set; }

    public virtual ICollection<MerchOrder> MerchOrders { get; set; } = new List<MerchOrder>();
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>(); // ✅ Додано Reviews
}


