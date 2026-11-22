using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;

namespace StoreSite.Models
{
    public class CatalogItem
    {
        public string Name { get; set; }      // giá trị lưu trong database
        public string Label { get; set; }     // tên hiển thị
        public int Count { get; set; }        // số sản phẩm đang dùng
    }

    public class CatalogViewModel
    {
        public string Kind { get; set; }      // "brand" hoặc "sport"
        public string Title { get; set; }     // "Nhãn hàng" / "Bộ môn"
        public List<CatalogItem> Items { get; set; } = new List<CatalogItem>();
    }

    /// <summary>
    /// Nơi lưu nhãn hàng / bộ môn chưa có sản phẩm nào (không cần sửa database).
    ///  - Nhãn hàng: chính là folder ~/ImageAssets/Men/&lt;Hãng&gt; và ~/ImageAssets/Women/&lt;Hãng&gt;.
    ///    Thêm nhãn hàng thì tạo 2 folder này, xóa nhãn hàng thì xóa 2 folder này.
    ///  - Bộ môn: danh sách tên trong ~/App_Data/ExtraSports.txt (mỗi dòng một môn).
    /// </summary>
    public static class CatalogStore
    {
        private static readonly string[] GenderFolders = { "Men", "Women" };
        private static readonly object Lock = new object();

        private static string ImageRoot()
        {
            return HostingEnvironment.MapPath("~/ImageAssets");
        }

        private static string SportFile()
        {
            return HostingEnvironment.MapPath("~/App_Data/ExtraSports.txt");
        }

        /// <returns>null nếu tên hợp lệ, ngược lại là thông báo lỗi.</returns>
        public static string ValidateName(string name, string what)
        {
            if (string.IsNullOrWhiteSpace(name) || name.Length > 80)
                return "Tên " + what + " không hợp lệ (1 - 80 ký tự).";
            if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                return "Tên " + what + " không được chứa các ký tự \\ / : * ? \" < > |";
            if (name.EndsWith(".") || name.EndsWith(" ") || name.StartsWith("."))
                return "Tên " + what + " không được bắt đầu bằng dấu chấm hoặc kết thúc bằng dấu chấm / khoảng trắng.";
            return null;
        }

        // ---------------- Nhãn hàng (folder) ----------------

        /// <summary>Tên các folder nhãn hàng đang có trong Men và Women (gộp, không trùng).</summary>
        public static List<string> BrandFolders()
        {
            var result = new List<string>();
            string root = ImageRoot();
            if (root == null) return result;

            foreach (string g in GenderFolders)
            {
                string dir = Path.Combine(root, g);
                if (!Directory.Exists(dir)) continue;
                foreach (string d in Directory.GetDirectories(dir))
                {
                    string n = Path.GetFileName(d);
                    if (!result.Contains(n, StringComparer.OrdinalIgnoreCase)) result.Add(n);
                }
            }
            return result;
        }

        /// <summary>Tạo ImageAssets/Men/Tên và ImageAssets/Women/Tên (nếu có rồi thì giữ nguyên).</summary>
        public static void CreateBrandFolders(string name)
        {
            string root = ImageRoot();
            foreach (string g in GenderFolders)
            {
                Directory.CreateDirectory(Path.Combine(root, g, name));
            }
        }

        /// <summary>Xóa ImageAssets/Men/Tên và ImageAssets/Women/Tên cùng mọi thứ bên trong.</summary>
        public static void DeleteBrandFolders(string name)
        {
            string root = ImageRoot();
            foreach (string g in GenderFolders)
            {
                string parent = Path.Combine(root, g);
                if (!Directory.Exists(parent)) continue;

                // Tìm folder theo tên (không phân biệt hoa thường) thay vì tự ghép đường dẫn
                string target = Directory.GetDirectories(parent)
                    .FirstOrDefault(d => string.Equals(Path.GetFileName(d), name, StringComparison.OrdinalIgnoreCase));
                if (target != null) Directory.Delete(target, true);
            }
        }

        // ---------------- Bộ môn (file văn bản) ----------------

        public static List<string> ExtraSports()
        {
            var list = new List<string>();
            try
            {
                string f = SportFile();
                if (f != null && File.Exists(f))
                {
                    lock (Lock)
                    {
                        list = File.ReadAllLines(f)
                                   .Select(x => x.Trim())
                                   .Where(x => x.Length > 0)
                                   .Distinct(StringComparer.OrdinalIgnoreCase)
                                   .ToList();
                    }
                }
            }
            catch (IOException) { }
            return list;
        }

        public static void AddSport(string name)
        {
            lock (Lock)
            {
                var list = ExtraSports();
                if (list.Contains(name, StringComparer.OrdinalIgnoreCase)) return;
                list.Add(name);
                SaveSports(list);
            }
        }

        public static void RemoveSport(string name)
        {
            lock (Lock)
            {
                var list = ExtraSports()
                    .Where(x => !string.Equals(x, name, StringComparison.OrdinalIgnoreCase)).ToList();
                SaveSports(list);
            }
        }

        private static void SaveSports(List<string> list)
        {
            string f = SportFile();
            Directory.CreateDirectory(Path.GetDirectoryName(f));
            File.WriteAllLines(f, list);
        }

        // ---------------- Dùng chung ----------------

        /// <summary>Gộp hai danh sách tên, không phân biệt hoa thường, sắp xếp A-Z.</summary>
        public static List<string> Merge(IEnumerable<string> a, IEnumerable<string> b)
        {
            return a.Concat(b)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .GroupBy(x => x, StringComparer.OrdinalIgnoreCase)
                    .Select(g => g.First())
                    .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                    .ToList();
        }
    }
}
