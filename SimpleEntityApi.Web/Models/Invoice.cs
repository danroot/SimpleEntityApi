using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimpleEntityApi.Web.Models
{
    public class Invoice
    {
        public Invoice()
        {
            this.LineItems = new List<InvoiceLineItem>();
            this.TaxRate = 0.07;
            this.CreatedDateUtc = DateTime.UtcNow;
            this.ModifiedDateUtc = DateTime.UtcNow;
        }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [DisplayName("Invoice Id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        [MinLength(5)]
        [RegularExpression("C.*")]
        [DisplayName("Customer")]
        public string CustomerName { get; set; }
        /// <summary>
        /// Company name
        /// </summary>
        [Description("Company")]
        public string CustomerCompany { get; set; }
        [Required(ErrorMessage = "Street is required.")]
        public string CustomerStreet1 { get; set; }
        public string CustomerStreet2 { get; set; }
        public string CustomerCity { get; set; }
        public string CustomerState { get; set; }
        public string CustomerZip { get; set; }
        public double SubTotal { get; set; }
        public double Tax { get; set; }
        public double TaxRate { get; set; }
        public double Total { get; set; }
        public string Comments { get; set; }

        public virtual IList<InvoiceLineItem> LineItems { get; set; }


        [DisplayName("Created")]
        public DateTime? CreatedDateUtc { get; set; }
        [DisplayName("Modified")]
        public DateTime? ModifiedDateUtc { get; set; }

    }
}