using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yapper.Tests.Data
{
    [Table("Categories")]
    public class Categories
    {
        [Column, Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CategoryID { get; set; }
        [Column, Required]
        public string CategoryName { get; set; }
        [Column]
        public string Description { get; set; }
        [Column]
        public byte[] Picture { get; set; }

        public ICollection<Products> Products { get; set; }
    }
    [Table("Categories")]
    public class Category
    {
        [Column("CategoryID"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        [Column("CategoryName"), Required]
        public string Name { get; set; }
        [Column]
        public string Description { get; set; }
        [Column]
        public byte[] Picture { get; set; }
    }
    [Table("Products")]
    public class Products
    {
        [Column, Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductID { get; set; }
        [Column, Required]
        public string ProductName { get; set; }
        [Column]
        public int SupplierID { get; set; }
        [Column]
        public int CategoryID { get; set; }
        [Column]
        public string QuantityPerUnit { get; set; }
        [Column]
        public decimal? UnitPrice { get; set; }
        [Column]
        public short UnitsInStock { get; set; }
        [Column]
        public short UnitsOnOrder { get; set; }
        [Column]
        public short ReorderLevel { get; set; }
        [Column, Required]
        public bool Discontinued { get; set; }

        public virtual Categories Category { get; set; }
    }
}