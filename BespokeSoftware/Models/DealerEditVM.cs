namespace BespokeSoftware.Models
{
    public class DealerEditVM
    {
        public DealerVM Dealer { get; set; } = new();

        public List<DealerAddressVM> DealerAddresses { get; set; } = new();
        public List<DealerNoteVM> DealerNotes { get; set; } = new();
        public List<string> DealerImages { get; set; } = new();
        public List<IFormFile> NewDealerImages { get; set; } = new();
        public List<PersonVM> Persons { get; set; } = new();
        public string MainOfficeImage { get; set; }
        public string MainOfficeImagePath { get; set; }
        public IFormFile MainOfficeImageFile { get; set; }
        public bool IsMainRemoved { get; set; }
    }

    public class DealerVM
    {
        public int DealerId { get; set; }
        public string DealerCode { get; set; }
        public string DealerName { get; set; }
        public string OwnerName { get; set; }
        public string GSTNo { get; set; }
        public string PANNo { get; set; }
        public int DefaultPaymentModeId { get; set; }
        public int WeeklyOffDayId { get; set; }
        public bool IsActive { get; set; }
    }

    public class DealerAddressVM
    {
        public int AddressId { get; set; }
        public string AddressType { get; set; }
        public string AddressLine { get; set; }
    }

    public class DealerNoteVM
    {
        public int NoteId { get; set; }
        public int CategoryId { get; set; }
        public string NoteText { get; set; }
        public string NoteFor { get; set; }
        public DateTime? Notedate { get; set; }
    }

    public class PersonVM
    {
        public int PersonID { get; set; }
        public List<string> Images { get; set; } = new();
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public List<IFormFile> NewImages { get; set; } = new();
        public string Gender { get; set; }
        public DateTime? DOB { get; set; }
        public DateTime? AnniversaryDate { get; set; }

        public string PANNo { get; set; }
        public string PersonType { get; set; }
        public string Remark { get; set; }

        public List<PersonCommunicationVM> Communications { get; set; } = new();
        public List<PersonAddressVM> Addresses { get; set; } = new();
    }

    public class PersonCommunicationVM
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Label { get; set; }
        public string Value { get; set; }
    }

    public class PersonAddressVM
    {
        public int Id { get; set; }
        public string AddressType { get; set; }
        public string AddressLine { get; set; }
    }
}