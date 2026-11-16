using System.ComponentModel.DataAnnotations;

namespace _25DH110979_quangminh.Models.ViewModel
{
    public class CheckoutVM
    {
        [Required(ErrorMessage = "Vui lòng nhập họ và tên người nhận")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại người nhận")]
        [Display(Name = "Số điện thoại")]
        [RegularExpression(@"^0\d{9,10}$", ErrorMessage = "Số điện thoại không hợp lệ (10-11 số, bắt đầu bằng 0)")]
        public string Phone { get; set; }

        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ nhận hàng")]
        [Display(Name = "Địa chỉ giao hàng")]
        public string ShippingAddress { get; set; }

        [Display(Name = "Phương thức giao hàng")]
        public string DeliveryMethod { get; set; } = "Giao hàng thường";

        [Display(Name = "Phương thức thanh toán")]
        public string PaymentMethod { get; set; } = "Tiền mặt khi nhận hàng";

        [Display(Name = "Ghi chú")]
        public string Notes { get; set; }

        public Cart Cart { get; set; }
    }
}
