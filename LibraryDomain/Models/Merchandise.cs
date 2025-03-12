using System;
using System.Collections.Generic;

namespace ShopDomain.Models;

public partial class Merchandise : Entity
{
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }
    public string ImageUrl { get; set; } = string.Empty;

    public int? CategoryId { get; set; }
    public int? BrandId { get; set; }
    public int? SizeId { get; set; }
    public int? TeamId { get; set; }

    public virtual Category? Category { get; set; }
    public virtual Brand? Brand { get; set; }
    public virtual Size? Size { get; set; }
    public virtual Team? Team { get; set; }
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    public virtual ICollection<UserCart> UserCarts { get; set; } = new List<UserCart>();
}
