using System.ComponentModel.DataAnnotations;

namespace BespokeSoftware.Models
{
    public class PaymentMode
    {
        public int PaymentModeID { get; set; }
        [Required(ErrorMessage = "Payment Mode is required")]
        public string Payment { get; set; }
        public bool IsDelete { get; set; }

        public List<PaymentMode> PaymentModeList { get; set; } = new List<PaymentMode>();
    }
}
