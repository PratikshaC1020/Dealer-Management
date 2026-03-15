using System.ComponentModel.DataAnnotations;

namespace BespokeSoftware.Models
{
    public class Dealer
    {
        public int DealerId { get; set; }

        [Required(ErrorMessage = "Dealer Code is required")]
        [StringLength(50)]
        public string? DealerCode { get; set; }

        [Required(ErrorMessage = "Dealer Name is required")]
        [StringLength(200)]
        public string? DealerName { get; set; }

        [Required(ErrorMessage = "Owner Name is required")]
        public string? OwnerName { get; set; }

        [Required(ErrorMessage = "GST No required")]
        [RegularExpression(@"^[0-9A-Z]{15}$", ErrorMessage = "Invalid GST")]
        public string? GSTNo { get; set; }

        [Required(ErrorMessage = "PAN required")]
        [RegularExpression(@"[A-Z]{5}[0-9]{4}[A-Z]{1}", ErrorMessage = "Invalid PAN")]
        public string? PANNo { get; set; }

        [Required]
        public int DepartmentId { get; set; }

        [Required]
        public int WeeklyOffDayId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public int DefaultPaymentModeId { get; set; }

        public string? PhotoType { get; set; }

        public string? ImgFile { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public int CreatedBy { get; set; }

        public int UpdatedBy { get; set; }

        public string DepartmentName { get; set; }

        public string CategoryName { get; set; }

        public string PaymentMode { get; set; }

        public string WeeklyOff { get; set; }

        public class modelDepartment
        {
            public int DepID { get; set; }

            public string? DepartmentName { get; set; }
        }
        public class modelCategory
        {
            public int ID { get; set; }

            public string? CategoryName { get; set; }
        }
        public class DrpPaymentMode
        {
            public int ID { get; set; }

            public string? PaymentModeName { get; set; }
        }
        public class DrpWeeklyOff
        {
            public int ID { get; set; }

            public string? Day { get; set; }
        }
        public class Country
        {
            public int CountryID { get; set; }

            public string CountryName { get; set; }
        }
        public class State
        {
            public int StateID { get; set; }

            public string StateName { get; set; }

            public int CountryID { get; set; }
        }
        public class City
        {
            public int CityID { get; set; }

            public string CityName { get; set; }

            public int StateID { get; set; }
        }
        public class DealerViewModel
        {
            public Dealer Dealer { get; set; }

            public List<Address> Addresses { get; set; }

            public List<CommunicationDetails> Communications { get; set; }

            public List<Department> Departments { get; set; }
            public List<Category> Categories { get; set; }
            public List<DrpPaymentMode> PaymentModes { get; set; }
            public List<DrpWeeklyOff> WeeklyOffDays { get; set; }

            public List<City> Cities { get; set; }
            public List<State> States { get; set; }
        }
        public class Address
        {
            public int ID { get; set; }

            public int DealerID { get; set; }

            public string AddressType { get; set; }

            public string AddressLine { get; set; }

            public int StateID { get; set; }

            public int CityID { get; set; }

            public string Pincode { get; set; }
        }

        public class CommunicationDetails
        {
            public int CommunicationID { get; set; }

            public int DealerID { get; set; }

            public string Type { get; set; }

            public string Value { get; set; }

            public bool IsActive { get; set; }
        }

    }
}