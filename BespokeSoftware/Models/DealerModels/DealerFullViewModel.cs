namespace BespokeSoftware.Models.DealerModels
{
    public class DealerFullViewModel
    {
        public DealerVM Dealer { get; set; }

        public List<DealerAddressVM> Addresses { get; set; } = new();
        public List<DealerNotesVM> Notes { get; set; } = new();
        public List<PersonVM> Persons { get; set; } = new();
        public List<PersonAddressVM> PersonAddresses { get; set; } = new();
        public List<PersonCommunicationVM> Communications { get; set; } = new();
        public List<ImageVM> Images { get; set; } = new();
        public List<ImageVMOwner> ImagesOwner { get; set; } = new();
        public List<CategoryVM> Categories { get; set; } = new();
    }

    

    public class DealerVM
    {
        public int DealerId { get; set; }
        public string DealerName { get; set; }
        public string OwnerName { get; set; }
        public string GSTNo { get; set; }
        public string PANNo { get; set; }
        public string PaymentMode { get; set; }
        public string WeeklyOff { get; set; }
        public bool IsActive { get; set; }
    }

    public class DealerAddressVM
    {
        public int AddressId { get; set; }
        public string AddressType { get; set; }
        public string AddressLine { get; set; }
    }

    public class DealerNotesVM
    {
        public int NoteId { get; set; }
        public string NoteText { get; set; }
        public string NoteFor { get; set; }
        public DateTime? NoteDate { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string DealerId { get; set; }
    }

    public class PersonVM
    {
        public int PersonID { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public DateTime? DOB { get; set; }
        public DateTime? AnniversaryDate { get; set; }
        public string AadhaarNo { get; set; }
        public string PANNo { get; set; }
        public string PersonType { get; set; }
        public string Remark { get; set; }
        public string DealerCode { get; set; }
        public int DealerId { get; set; }
    }

    public class PersonAddressVM
    {
        public int PersonID { get; set; }
        public string AddressLine { get; set; }
    }

    public class PersonCommunicationVM
    {
        public int PersonID { get; set; }
        public string Value { get; set; }
    }
    public class CategoryVM
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
    }
    public class ImageVM
    {
        public int IdentityID { get; set; }
        public string Type { get; set; }
        public string ImageBase64 { get; set; }
    }
    public class ImageVMOwner
    {
        public int IdentityID { get; set; }
        public string Type { get; set; }
        public string ImageBase64 { get; set; }
    }
}