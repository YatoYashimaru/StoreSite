using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StoreSite.Models
{
    /// <summary>Dữ liệu cho trang danh sách sản phẩm của admin.</summary>
    public class AdminProductListViewModel
    {
        public List<SanPham> Items { get; set; } = new List<SanPham>();
        public int Page { get; set; }
        public int TotalPages { get; set; }
        public int Total { get; set; }

        // Bộ lọc đang chọn
        public string Search { get; set; }
        public string Loai { get; set; }
        public string Sport { get; set; }
        public string Gender { get; set; }

        // Danh sách để đổ vào ô chọn
        public List<string> Brands { get; set; } = new List<string>();
        public List<string> Sports { get; set; } = new List<string>();

        public bool HasFilter
        {
            get
            {
                return !string.IsNullOrWhiteSpace(Search) || !string.IsNullOrEmpty(Loai)
                    || !string.IsNullOrEmpty(Sport) || !string.IsNullOrEmpty(Gender);
            }
        }
    }

    /// <summary>
    /// Form thêm / sửa sản phẩm. Dùng ViewModel riêng thay vì sửa class SanPham do Entity Framework sinh ra
    /// (file đó bị ghi đè mỗi khi cập nhật model).
    /// </summary>
    public class ProductFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm")]
        [StringLength(200, ErrorMessage = "Tên tối đa 200 ký tự")]
        [Display(Name = "Tên sản phẩm")]
        public string ItemName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập hãng")]
        [StringLength(80, ErrorMessage = "Hãng tối đa 80 ký tự")]
        [Display(Name = "Hãng")]
        public string LoaiGiay { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập môn thể thao")]
        [StringLength(80, ErrorMessage = "Môn tối đa 80 ký tự")]
        [Display(Name = "Môn thể thao")]
        public string Sport { get; set; }

        [Display(Name = "Giới tính")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá bán")]
        [Range(0, 1000000000, ErrorMessage = "Giá bán không hợp lệ")]
        [Display(Name = "Giá bán (đ)")]
        public decimal? GiaTien { get; set; }

        [Range(0, 1000000000, ErrorMessage = "Giá gốc không hợp lệ")]
        [Display(Name = "Giá gốc (đ), để trống nếu không giảm giá")]
        public decimal? GiaTienGoc { get; set; }

        [StringLength(300, ErrorMessage = "Đường dẫn tối đa 300 ký tự")]
        [Display(Name = "Đường dẫn ảnh đại diện")]
        public string ImgURL { get; set; }

        [Display(Name = "Mô tả")]
        public string DescriptionTxt { get; set; }

        // Kiểu chữ để không bị lệch dấu thập phân theo ngôn ngữ của máy (xem AdminController.ValidateProduct)
        [Display(Name = "Đánh giá (0 - 5)")]
        public string RatingText { get; set; }

        [Display(Name = "Đánh dấu là sản phẩm mới")]
        public bool SanPhamMoi { get; set; }
    }
}
