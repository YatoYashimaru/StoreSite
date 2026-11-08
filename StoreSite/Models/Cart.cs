using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StoreSite.Models
{
    public class Cart
    {
        private List<CartItem> items = new List<CartItem>();
        public List<CartItem> TakeList() => items.ToList(); // Lay danh sach san pham trong gio hang
        public int SumQuantity() => items.Sum(i => i.Quantity); // Tinh tong so luong san pham trong gio hang

        // Tim dong co cung san pham + cung size (tra ve null neu chua co)
        private CartItem FindItem(int productId, int size) =>
            items.FirstOrDefault(i => i.ProductId == productId && i.Size == size);

        public bool AddItem(int productId, int size, int quantity)
        {
            if (size < 36 || size > 45) return false; // Kiem tra size hop le
            if (quantity < 1 || quantity > CartItem.MaxQuantity) return false; // Kiem tra so luong hop le
            // Kiem tra xem san pham da co trong gio hang chua, neu co thi cong them so luong, neu khong thi them moi:
            var existingItem = FindItem(productId, size);
            if (existingItem != null)
                existingItem.AddQuantity(quantity);
            else items.Add(new CartItem(productId, size, quantity));
            return true;
        }

        // Dat lai so luong cua 1 dong (khong cong don nhu AddItem)
        public bool UpdateQuantity(int productId, int size, int quantity)
        {
            if (quantity < 1 || quantity > CartItem.MaxQuantity) return false;
            var item = FindItem(productId, size);
            if (item == null) return false;
            item.SetQuantity(quantity);
            return true;
        }

        // Xoa 1 dong khoi gio. RemoveAll tra ve so dong da xoa, nen > 0 nghia la xoa duoc
        public bool RemoveItem(int productId, int size)
        {
            return items.RemoveAll(i => i.ProductId == productId && i.Size == size) > 0;
        }

        // Xoa sach gio (dung sau khi dat hang thanh cong)
        public void Clear()
        {
            items.Clear();
        }
    }
}