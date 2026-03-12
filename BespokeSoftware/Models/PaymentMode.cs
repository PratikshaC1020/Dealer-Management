namespace BespokeSoftware.Models
{
    public class PaymentMode
    {
        public int PaymentModeID { get; set; }
        public string Payment { get; set; }
        public bool IsDelete { get; set; }

        public List<PaymentMode> PaymentModeList { get; set; }
    }
}
