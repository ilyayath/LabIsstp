using System;
using System.Collections.Generic;

namespace ShopDomain.Models;

public partial class Review : Entity
{


    public int BuyerId { get; set; }

    public int MerchandiseId { get; set; }

    public int? Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime? ReviewDate { get; set; }

    public virtual Buyer Buyer { get; set; } = null!;

    public virtual Merchandise Merchandise { get; set; } = null!;
}
