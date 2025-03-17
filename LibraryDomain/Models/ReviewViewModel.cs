using System.ComponentModel.DataAnnotations;

namespace ShopDomain.Models
{
    public class ReviewViewModel
    {
        public int MerchandiseId { get; set; }

        public string MerchandiseName { get; set; }

        [Range(1, 5, ErrorMessage = "Оцінка має бути від 1 до 5")]
        public int? Rating { get; set; }

        [StringLength(500, ErrorMessage = "Коментар не може бути довшим за 500 символів")]
        public string Comment { get; set; }
    }
}