using System;
using System.Collections.Generic;

namespace StoreSite.Models
{
    public static class SportNames
    {
        private static readonly Dictionary<string, string> Map =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Running",      "Chạy bộ" },
            { "Football",     "Bóng đá" },
            { "Basketball",   "Bóng rổ" },
        };

        // Môn nào chưa có trong danh sách thì hiện nguyên tên gốc
        public static string Label(string sport)
        {
            string v;
            return sport != null && Map.TryGetValue(sport, out v) ? v : sport;
        }
    }
}