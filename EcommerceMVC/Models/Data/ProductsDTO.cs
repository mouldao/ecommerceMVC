using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EcommerceMVC.Models.Data
{
    [Table("tblProducts")]
    public class ProductsDTO
    {
        [Key]
        public int id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string CategoryName { get; set; }
        public int CategoryId { get; set; }
        public string  ImageName { get; set; }

        [ForeignKey("CategoryId")]
        public virtual CategoryDTO Category { get; set; }
    }
}