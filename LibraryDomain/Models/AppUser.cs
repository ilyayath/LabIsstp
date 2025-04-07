using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;

namespace ShopDomain.Models
{
    public class AppUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string? Country { get; set; } // Місце проживання
        public DateTime? BirthDate { get; set; } // Дата народження
        public string? ShippingAddress { get; set; } // Адреса доставки
    }
}

