using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Hosting;

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

    /// Form thêm / sửa sản phẩm. Dùng ViewModel riêng thay vì sửa class SanPham do Entity Framework sinh ra
    /// (file đó bị ghi đè mỗi khi cập nhật model).
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
        [Range(1000, 1000000000, ErrorMessage = "Giá bán phải từ 1.000đ trở lên")]
        [Display(Name = "Giá bán (đ)")]
        public decimal? GiaTien { get; set; }

        [Range(1000, 1000000000, ErrorMessage = "Giá gốc phải từ 1.000đ trở lên")]
        [Display(Name = "Giá gốc (đ), để trống nếu không giảm giá")]
        public decimal? GiaTienGoc { get; set; }

        // Đường dẫn ảnh đại diện (ảnh số 1). Không nhập tay nữa: tự gán sau khi tải ảnh lên.
        public string ImgURL { get; set; }

        // Chỉ để hiển thị trong form sửa
        public string ImageFolderText { get; set; }
        public List<ExistingImage> ExistingImages { get; set; } = new List<ExistingImage>();

        [Display(Name = "Mô tả")]
        public string DescriptionTxt { get; set; }

        [Display(Name = "Đánh dấu là sản phẩm mới")]
        public bool SanPhamMoi { get; set; }
    }

    public class ExistingImage
    {
        public string FileName { get; set; }   // vd "2.jpg"
        public string Url { get; set; }        // đường dẫn dùng cho thẻ img
    }

    /// <summary>Kế hoạch xử lý ảnh của một sản phẩm (đã kiểm tra hợp lệ, chưa ghi gì xuống đĩa).</summary>
    public class ImagePlan
    {
        public string Folder { get; set; }                       // đường dẫn ổ đĩa của thư mục sản phẩm
        public bool IsNewFolder { get; set; }                    // thư mục chưa tồn tại, sẽ được tạo
        public List<string> Remove { get; set; } = new List<string>();
        public List<HttpPostedFileBase> Uploads { get; set; } = new List<HttpPostedFileBase>();
        public bool Skip { get; set; }                           // không đụng đến ảnh (giữ nguyên ImgURL cũ)
    }

    /// <summary>
    /// Ảnh sản phẩm nằm ở ImageAssets/Men|Women/[Hãng]/[Tên sản phẩm]/1.jpg, 2.jpg ... (đúng cách trang chi tiết quét thư mục:
    /// ProductGallery sắp xếp theo số 1..n, ảnh số 1 là ảnh đại diện). Mọi ảnh tải lên đều được đặt lại tên 1..n liên tục.
    /// </summary>
    public static class ProductImageStore
    {
        public const int MaxFiles = 12;                    // tối đa ảnh cho mỗi sản phẩm
        public const int MaxFileBytes = 6 * 1024 * 1024;   // tối đa mỗi ảnh (6 MB)

        private static readonly string[] Extensions = { ".jpg", ".jpeg", ".png", ".webp", ".avif", ".gif" };

        private static string Root()
        {
            return Path.GetFullPath(HostingEnvironment.MapPath("~/ImageAssets"));
        }

        public static string GenderFolder(string gender)
        {
            return gender == "Nu" ? "Women" : "Men";
        }

        /// <summary>Tên thư mục từ tên sản phẩm: bỏ ký tự cấm, gộp khoảng trắng ("Nike Air Max 1" giữ nguyên).</summary>
        public static string FolderNameFor(string productName)
        {
            var bad = Path.GetInvalidFileNameChars().Concat(new[] { '#', '%', '&', '+', '?' }).ToArray();
            string s = productName ?? "";
            foreach (char c in bad) s = s.Replace(c, ' ');
            s = Regex.Replace(s, @"\s+", " ").Trim(' ', '.');
            if (s.Length > 100) s = s.Substring(0, 100).Trim(' ', '.');
            return s.Length == 0 ? "San pham" : s;
        }

        /// <summary>Thư mục dành cho sản phẩm: ImageAssets/Men|Women/[Hãng]/[Tên sản phẩm]. Hãng phải là tên hợp lệ.</summary>
        public static string FolderPath(string gender, string brand, string productName)
        {
            string root = Root();
            string full = Path.GetFullPath(Path.Combine(root, GenderFolder(gender), brand, FolderNameFor(productName)));
            if (!full.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Đường dẫn thư mục không hợp lệ.");
            return full;
        }

        /// <summary>
        /// Thư mục ảnh hiện tại của sản phẩm, suy ra từ ImgURL. Chỉ nhận đúng dạng
        /// /ImageAssets/Men|Women/[Hãng]/[Sản phẩm]/[tệp]; ảnh nằm chỗ khác thì trả về null để không đụng vào thư mục chung.
        /// </summary>
        public static string CurrentFolder(string imgUrl)
        {
            if (string.IsNullOrWhiteSpace(imgUrl)) return null;
            string u = imgUrl.Trim().Replace('\\', '/');
            if (u.StartsWith("~")) u = u.Substring(1);

            const string pre = "/ImageAssets/";
            if (!u.StartsWith(pre, StringComparison.OrdinalIgnoreCase)) return null;

            string[] parts = u.Substring(pre.Length).Split('/');
            if (parts.Length != 4) return null;
            if (!string.Equals(parts[0], "Men", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(parts[0], "Women", StringComparison.OrdinalIgnoreCase)) return null;
            if (parts.Any(x => x.Length == 0 || x == "." || x == "..")) return null;

            string root = Root();
            string dir = Path.GetFullPath(Path.Combine(root, parts[0], parts[1], parts[2]));
            if (!dir.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) return null;
            return Directory.Exists(dir) ? dir : null;
        }

        /// <summary>"ImageAssets/Men/Nike/Tên" (để hiển thị).</summary>
        public static string RelativeText(string folder)
        {
            string rel = Path.GetFullPath(folder).Substring(Root().Length).TrimStart('\\', '/').Replace('\\', '/');
            return "ImageAssets/" + rel;
        }

        /// <summary>"/ImageAssets/Men/Nike/Tên/1.jpg" (lưu vào SanPhams.ImgURL).</summary>
        public static string UrlOf(string folder, string fileName)
        {
            return "/" + RelativeText(folder) + "/" + fileName;
        }

        /// <summary>Tên các tệp ảnh trong thư mục, xếp theo số 1, 2, ... 10 (giống ProductGallery).</summary>
        public static List<string> ListFiles(string folder)
        {
            if (!Directory.Exists(folder)) return new List<string>();
            return Directory.EnumerateFiles(folder)
                            .Where(f => Extensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
                            .Select(f => Path.GetFileName(f))
                            .Select(n =>
                            {
                                int num;
                                bool isNum = int.TryParse(Path.GetFileNameWithoutExtension(n),
                                    System.Globalization.NumberStyles.None,
                                    System.Globalization.CultureInfo.InvariantCulture, out num);
                                return new { Name = n, Num = isNum ? num : int.MaxValue };
                            })
                            .OrderBy(x => x.Num)
                            .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                            .Select(x => x.Name)
                            .ToList();
        }

        /// <summary>Các tệp ảnh người dùng vừa chọn (đúng thứ tự trình duyệt gửi lên).</summary>
        public static List<HttpPostedFileBase> ReadUploads(HttpRequestBase request)
        {
            var list = new List<HttpPostedFileBase>();
            foreach (var f in request.Files.GetMultiple("images"))
            {
                if (f != null && f.ContentLength > 0 && !string.IsNullOrEmpty(f.FileName)) list.Add(f);
            }
            return list;
        }

        /// <returns>null nếu hợp lệ, ngược lại là thông báo lỗi.</returns>
        public static string ValidateUploads(IList<HttpPostedFileBase> uploads, int keptCount)
        {
            if (keptCount + uploads.Count > MaxFiles)
                return "Mỗi sản phẩm tối đa " + MaxFiles + " ảnh (hiện giữ " + keptCount + ", thêm " + uploads.Count + ").";

            foreach (var f in uploads)
            {
                string name = Path.GetFileName(f.FileName);
                string ext = Path.GetExtension(name).ToLowerInvariant();
                if (!Extensions.Contains(ext))
                    return "“" + name + "” không phải ảnh được hỗ trợ (jpg, jpeg, png, webp, avif, gif).";
                if (f.ContentLength > MaxFileBytes)
                    return "“" + name + "” lớn hơn " + (MaxFileBytes / 1024 / 1024) + " MB.";
                if (!LooksLikeImage(f))
                    return "“" + name + "” không phải tệp ảnh hợp lệ.";
            }
            return null;
        }

        // Kiểm tra vài byte đầu của tệp để chặn tệp đội lốt đuôi ảnh
        private static bool LooksLikeImage(HttpPostedFileBase f)
        {
            var s = f.InputStream;
            byte[] h = new byte[12];
            s.Position = 0;
            int n = s.Read(h, 0, 12);
            s.Position = 0;
            if (n < 12) return false;

            bool jpg = h[0] == 0xFF && h[1] == 0xD8 && h[2] == 0xFF;
            bool png = h[0] == 0x89 && h[1] == 0x50 && h[2] == 0x4E && h[3] == 0x47;
            bool gif = h[0] == 0x47 && h[1] == 0x49 && h[2] == 0x46 && h[3] == 0x38;
            bool webp = h[0] == 'R' && h[1] == 'I' && h[2] == 'F' && h[3] == 'F' &&
                        h[8] == 'W' && h[9] == 'E' && h[10] == 'B' && h[11] == 'P';
            bool avif = h[4] == 'f' && h[5] == 't' && h[6] == 'y' && h[7] == 'p';
            return jpg || png || gif || webp || avif;
        }

        /// <summary>Thư mục này đang là thư mục ảnh của một sản phẩm khác (không phải sản phẩm excludeId)?</summary>
        public static bool InUseByOther(StoreDbEntities db, string folder, int excludeId)
        {
            string prefix = UrlOf(folder, "");
            return db.SanPhams.Any(p => p.Id != excludeId && p.ImgURL != null && p.ImgURL.StartsWith(prefix));
        }

        /// <summary>
        /// Ghi ảnh theo kế hoạch: xóa ảnh bị đánh dấu, rồi đặt lại tên các ảnh còn lại + ảnh mới thành 1, 2, 3 ... liên tục.
        /// </summary>
        /// <returns>Tên tệp ảnh số 1 (ảnh đại diện).</returns>
        public static string Execute(ImagePlan plan)
        {
            string folder = plan.Folder;
            Directory.CreateDirectory(folder);

            var existing = ListFiles(folder);
            var removeSet = new HashSet<string>(plan.Remove, StringComparer.OrdinalIgnoreCase);
            var keep = existing.Where(f => !removeSet.Contains(f)).ToList();

            if (removeSet.Count == 0 && plan.Uploads.Count == 0)
                return keep.Count > 0 ? keep[0] : null;

            foreach (var f in existing.Where(f => removeSet.Contains(f)))
                File.Delete(Path.Combine(folder, f));

            // Bước 1: chuyển ảnh giữ lại sang tên tạm để không đè lên nhau khi đánh số lại
            var staged = new List<string>();
            int i = 0;
            foreach (var f in keep)
            {
                string tmp = Path.Combine(folder, "~tmp" + (i++) + Path.GetExtension(f).ToLowerInvariant());
                File.Move(Path.Combine(folder, f), tmp);
                staged.Add(tmp);
            }

            // Bước 2: đặt tên chính thức 1..n (ảnh cũ trước, ảnh mới sau)
            int n = 1;
            string first = null;
            foreach (var tmp in staged)
            {
                string name = n + Path.GetExtension(tmp);
                File.Move(tmp, Path.Combine(folder, name));
                if (n == 1) first = name;
                n++;
            }
            foreach (var up in plan.Uploads)
            {
                string name = n + Path.GetExtension(up.FileName).ToLowerInvariant();
                up.SaveAs(Path.Combine(folder, name));
                if (n == 1) first = name;
                n++;
            }
            return first;
        }
    }
}
