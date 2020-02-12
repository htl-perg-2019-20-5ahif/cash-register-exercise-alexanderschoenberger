using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashRegister
{
    class Product
    {
        [Key]
        int ID { get; set; }
        [Required]
        string Name { get; set; }
        [Required]
        decimal Price { get; set; }
    }

    class Line
    {
        [Key]
        int ID { get; set; }
        int ProductID { get; set; }
        Product Product { get; set; }
        int Amount { get; set; }

        decimal TotalPrice { get; set; }
    }

    class Receipt
    {
        [Key]
        int ID { get; set; }
        DateTime TimeStamp { get; set; }

        decimal TotalPrice { get; set; }
    }
}
