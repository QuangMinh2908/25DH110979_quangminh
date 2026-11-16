namespace _25DH110979_quangminh.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    public partial class OrderDetail
    {
        [Display(Name = "Mã chi tiết")]
        public int ID { get; set; }

        [Display(Name = "Mã sản phẩm")]
        public int ProductID { get; set; }

        [Display(Name = "Mã đơn hàng")]
        public int OrderID { get; set; }

        [Display(Name = "Số lượng")]
        [Required(ErrorMessage = "Vui lòng nhập số lượng")]
        [Range(1, 10000, ErrorMessage = "Số lượng phải từ 1 trở lên")]
        public int Quantity { get; set; }

        [Display(Name = "Đơn giá")]
        [DisplayFormat(DataFormatString = "{0:N0} đ", ApplyFormatInEditMode = false)]
        public decimal UnitPrice { get; set; }

        [Display(Name = "Thành tiền")]
        [DisplayFormat(DataFormatString = "{0:N0} đ", ApplyFormatInEditMode = false)]
        public Nullable<decimal> TotalPrice { get; set; }
    
        [Display(Name = "Đơn hàng")]
        public virtual Order Order { get; set; }

        [Display(Name = "Sản phẩm")]
        public virtual Product Product { get; set; }
    }
}
