using System.Collections.Generic;

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
}
