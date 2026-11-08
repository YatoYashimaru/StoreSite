using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Hosting;

namespace StoreSite.Models
{
    /// <summary>
    /// Tìm danh sách ảnh của một sản phẩm trong ~/ImageAssets.
    ///
    /// Cấu trúc thư mục:
    ///   ImageAssets/Men/Adidas/Adidas F50 Messi/1.jpg, 2.jpg, 3.jpg ...
    ///   ImageAssets/Women/Nike/Ten San Pham/1.png, 2.png ...
    ///
    /// Thư mục sản phẩm được xác định theo thứ tự:
    ///   1. Chính thư mục chứa ảnh ImgURL (nếu ảnh đó nằm trong thư mục sản phẩm, tên file là số).
    ///   2. Thư mục có tên trùng với tên file ImgURL hoặc tên sản phẩm
    ///      (không phân biệt hoa thường, bỏ dấu cách, gạch dưới, gạch ngang).
    ///   3. Thư mục có tên nằm trong tên sản phẩm (hoặc ngược lại), ưu tiên tên dài nhất.
    /// Không tìm thấy thì trả về đúng một ảnh là ImgURL, trang chi tiết vẫn hoạt động như cũ.
    /// </summary>
    public static class ProductGallery
    {
        private const string Root = "~/ImageAssets/";
        private static readonly string[] Extensions = { ".jpg", ".jpeg", ".png", ".webp", ".avif", ".gif" };

        // Các thư mục không chứa ảnh sản phẩm
        private static readonly string[] SkipFolders = { "SpecialTheme" };

        /// <returns>Danh sách đường dẫn dạng "~/ImageAssets/...", đã sắp xếp theo số thứ tự 1..n.</returns>
        public static List<string> GetImages(string name, string imgUrl)
        {
            var result = new List<string>();

            try
            {
                string rootDir = HostingEnvironment.MapPath(Root);
                if (rootDir != null && Directory.Exists(rootDir))
                {
                    string folder = FindFolder(rootDir, name, imgUrl);
                    if (folder != null)
                        result = ListImages(rootDir, folder);
                }
            }
            catch (Exception)
            {
                // Lỗi đọc ổ đĩa thì dùng ảnh dự phòng bên dưới, không làm hỏng trang
                result = new List<string>();
            }

            if (result.Count == 0 && !string.IsNullOrWhiteSpace(imgUrl))
                result.Add("~" + imgUrl);

            return result;
        }

        // ---------------------------------------------------------------

        private static string FindFolder(string rootDir, string name, string imgUrl)
        {
            // 1. Ảnh ImgURL nằm sẵn trong thư mục sản phẩm
            if (!string.IsNullOrWhiteSpace(imgUrl))
            {
                string imgPath = HostingEnvironment.MapPath("~" + imgUrl);
                if (imgPath != null && File.Exists(imgPath))
                {
                    string dir = Path.GetDirectoryName(imgPath);
                    bool numbered = IsNumbered(Path.GetFileNameWithoutExtension(imgPath));
                    bool deeper = !string.Equals(dir.TrimEnd('\\', '/'), rootDir.TrimEnd('\\', '/'),
                                                 StringComparison.OrdinalIgnoreCase);
                    if (numbered && deeper && !IsSkipped(rootDir, dir))
                        return dir;
                }
            }

            // Mọi thư mục có chứa ảnh (trừ SpecialTheme)
            var folders = Directory.EnumerateDirectories(rootDir, "*", SearchOption.AllDirectories)
                                   .Where(d => !IsSkipped(rootDir, d))
                                   .Where(HasImages)
                                   .Select(d => new { Path = d, Key = Normalize(Path.GetFileName(d)) })
                                   .Where(x => x.Key.Length > 0)
                                   .ToList();

            string nameKey = Normalize(name);
            string fileKey = string.IsNullOrWhiteSpace(imgUrl)
                ? ""
                : Normalize(Path.GetFileNameWithoutExtension(imgUrl));

            // 2. Trùng tên hoàn toàn
            var exact = folders.FirstOrDefault(f => f.Key == fileKey || f.Key == nameKey);
            if (exact != null) return exact.Path;

            // 3. Tên thư mục nằm trong tên sản phẩm (hoặc ngược lại), lấy tên dài nhất cho chính xác
            var partial = folders
                .Where(f => f.Key.Length >= 4 &&
                            ((nameKey.Length > 0 && (nameKey.Contains(f.Key) || f.Key.Contains(nameKey))) ||
                             (fileKey.Length > 0 && (fileKey.Contains(f.Key) || f.Key.Contains(fileKey)))))
                .OrderByDescending(f => f.Key.Length)
                .FirstOrDefault();

            return partial == null ? null : partial.Path;
        }

        private static List<string> ListImages(string rootDir, string folder)
        {
            var files = Directory.EnumerateFiles(folder)
                                 .Where(f => Extensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
                                 .Select(f => new
                                 {
                                     Path = f,
                                     Stem = Path.GetFileNameWithoutExtension(f)
                                 })
                                 .Select(x =>
                                 {
                                     int n;
                                     bool isNum = int.TryParse(x.Stem, NumberStyles.None, CultureInfo.InvariantCulture, out n);
                                     return new { x.Path, x.Stem, Num = isNum ? n : int.MaxValue };
                                 })
                                 // 1, 2, 3 ... 10 theo giá trị số (không phải theo chữ: 1, 10, 2); file không phải số xếp sau, theo tên
                                 .OrderBy(x => x.Num)
                                 .ThenBy(x => x.Stem, StringComparer.OrdinalIgnoreCase)
                                 .ToList();

            string rootFull = Path.GetFullPath(rootDir).TrimEnd('\\', '/');

            return files.Select(x => ToVirtual(rootFull, x.Path)).ToList();
        }

        /// <summary>Đổi đường dẫn ổ đĩa thành "~/ImageAssets/a/b/c.jpg", mã hóa từng đoạn (dấu cách thành %20).</summary>
        private static string ToVirtual(string rootFull, string filePath)
        {
            string rel = Path.GetFullPath(filePath).Substring(rootFull.Length)
                             .TrimStart('\\', '/');

            string[] parts = rel.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
            return Root + string.Join("/", parts.Select(Uri.EscapeDataString));
        }

        private static bool HasImages(string dir)
        {
            return Directory.EnumerateFiles(dir)
                            .Any(f => Extensions.Contains(Path.GetExtension(f).ToLowerInvariant()));
        }

        private static bool IsNumbered(string stem)
        {
            int n;
            return int.TryParse(stem, NumberStyles.None, CultureInfo.InvariantCulture, out n);
        }

        private static bool IsSkipped(string rootDir, string dir)
        {
            string rel = Path.GetFullPath(dir).Substring(Path.GetFullPath(rootDir).TrimEnd('\\', '/').Length);
            string[] parts = rel.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
            return parts.Any(p => SkipFolders.Contains(p, StringComparer.OrdinalIgnoreCase));
        }

        /// <summary>Chữ thường, bỏ dấu tiếng Việt, chỉ giữ chữ và số: "Adidas_F50 Messi" thành "adidasf50messi".</summary>
        private static string Normalize(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "";

            string d = s.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(d.Length);
            foreach (char c in d)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark) continue;
                if (c == 'đ' || c == 'Đ') { sb.Append('d'); continue; }
                if (char.IsLetterOrDigit(c)) sb.Append(char.ToLowerInvariant(c));
            }
            return sb.ToString();
        }
    }
}
