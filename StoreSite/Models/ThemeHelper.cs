using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;

namespace StoreSite.Models
{
    public class ProductTheme
    {
        public string ImageUrl { get; set; }                         // ảnh nền, null nếu không có
        public string Accent { get; set; } = "#00b8c4";              // nút, size, breadcrumb
        public string Panel { get; set; } = "rgba(255,255,255,.9)";  // nền khung nội dung
        public string Text { get; set; } = "#333333";                // màu chữ
    }

    public static class ThemeHelper
    {
        private const string Folder = "~/ImageAssets/SpecialTheme/";
        private static readonly string[] Extensions = { ".jpg", ".jpeg", ".png", ".webp" };

        // Khóa = tên file ảnh sản phẩm (bỏ đuôi), không phân biệt hoa thường
        private static readonly Dictionary<string, ProductTheme> Presets =
            new Dictionary<string, ProductTheme>(StringComparer.OrdinalIgnoreCase)
        {
            { "Nike Phantom 6 Erling Haaland",
              new ProductTheme { Accent = "#5690BF", Panel = "rgba(233,253,253,.85)" } },
            { "Adidas F50 Messi",
              new ProductTheme { Accent = "#A1EDDE", Panel = "rgba(222, 251, 243, 0.8)" } },
            { "Nike Mercurial Superfly",
              new ProductTheme { Accent = "#DE533A", Panel = "rgba(243, 197, 197, 0.8)" } },
        };

        public static ProductTheme GetTheme(string imgUrl)
        {
            if (string.IsNullOrWhiteSpace(imgUrl)) return null;

            string name = Path.GetFileNameWithoutExtension(imgUrl);
            string image = FindImage(name);

            ProductTheme preset;
            bool hasPreset = Presets.TryGetValue(name, out preset);

            if (image == null && !hasPreset) return null;   // sản phẩm thường, không theme

            var b = hasPreset ? preset : new ProductTheme();
            return new ProductTheme
            {
                ImageUrl = image,
                Accent = b.Accent,
                Panel = b.Panel,
                Text = b.Text
            };
        }

        private static string FindImage(string name)
        {
            string dir = HostingEnvironment.MapPath(Folder);
            if (dir == null || !Directory.Exists(dir)) return null;

            string file = Directory.EnumerateFiles(dir).FirstOrDefault(f =>
                Extensions.Contains(Path.GetExtension(f).ToLowerInvariant()) &&
                string.Equals(Path.GetFileNameWithoutExtension(f), name, StringComparison.OrdinalIgnoreCase));

            return file == null ? null : Folder + Path.GetFileName(file);
        }
    }
}