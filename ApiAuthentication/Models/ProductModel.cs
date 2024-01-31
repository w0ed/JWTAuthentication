using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ApiAuthentication.Models
{
    public class ProductModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }
        public int StockQuantity { get; set; }
        public int UserId { get; set; }
    }

}