using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;

namespace StoreSite.Models
{
    public class CartItem
    {
        public const int MaxQuantity = 10; //
        public int ProductId { get; private set; }
        public int Size { get; private set; }
        public int Quantity { get; private set; }
        public CartItem(int productId, int size, int quantity)
        {
            ProductId = productId;
            Size = size;
            Quantity = quantity;
        }
        public void AddQuantity(int add)
        {
            Quantity = Math.Min(Quantity + add, MaxQuantity);
        }
        // Dat lai so luong (khong cong don), luon giu trong khoang 1..MaxQuantity
        public void SetQuantity(int quantity)
        {
            Quantity = Math.Max(1, Math.Min(quantity, MaxQuantity));
        }
    }
}