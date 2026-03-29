using System.ComponentModel.DataAnnotations;
using static BespokeSoftware.Models.Dealer;

namespace BespokeSoftware.Models
{
    public class Dealer
    {
        public int DealerId { get; set; }

        [Required(ErrorMessage = "Dealer Code is required")]
        [StringLength(50)]
        public string? DealerCode { get; set; }
        public string? Remark { get; set; }

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

        [Required(ErrorMessage = "Department is required")]
        public int DepartmentId { get; set; }

        [Required(ErrorMessage = "WeeklyOffDay is required")]
        public int WeeklyOffDayId { get; set; }

        [Required(ErrorMessage = "Category is required")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Payment Mode is required")]
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
        public List<int> CommunicationIds { get; set; }
        public List<int> NoteIds { get; set; }

        public class NoteVM
        {
            public int CategoryId { get; set; }
            public string NoteText { get; set; }
            public string CategoryName { get; set; }
        }
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
        public class Person
        {
            public int PersonID { get; set; }

            public string PersonName { get; set; }

            public string PersonTYpe { get; set; }
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
            public List<NoteVM> NotesA { get; set; }

            public List<DealerAddressAdd> DealerAddresses { get; set; }
            public List<DealerNoteAdd> DealerNotes { get; set; }
            public List<PersonVMAdd> Persons { get; set; }
            public List<IFormFile> DealerImages { get; set; }
            public List<IFormFile> Images { get; set; }
            public List<string> DealerImageTypes { get; set; }

            public class PersonVMAdd
            {
                public string Title { get; set; }
                public string First { get; set; }
                public string Middle { get; set; }
                public string Last { get; set; }
                public string Gender { get; set; }
                public DateTime? Dob { get; set; }
                public DateTime? Anniversary { get; set; }
                public string Pan { get; set; }
                public string Type { get; set; }
                public string Remark { get; set; }

                public List<IFormFile> Images { get; set; }
                public List<CommunicationVMAdd> Communications { get; set; }
                public List<AddressVMAdd> Addresses { get; set; }
            }

            public class CommunicationVMAdd
            {
                public string Type { get; set; }
                public string Label { get; set; }
                public string Value { get; set; }
            }

            public class AddressVMAdd
            {
                public string Type { get; set; }
                public string Address { get; set; }
            }

            public class DealerAddressAdd
            {
                public string AddressType { get; set; }
                public string AddressLine { get; set; }
            }

            public class DealerNoteAdd
            {
                public int CategoryId { get; set; }
                public string NoteText { get; set; }
                public string NoteFor { get; set; }
                public DateTime? NoteDate { get; set; }
            }

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
            public string CityName { get; set; }
        }
        public string DealerImage { get; set; }
        public string CompanyImage { get; set; }
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