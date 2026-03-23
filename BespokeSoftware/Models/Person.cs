using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BespokeSoftware.Models
{
    public class Person
    {
        public int Id { get; set; }

        // ===== BASIC =====

        public string Title { get; set; }

        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public string LastName { get; set; }

        // ===== PERSONAL =====

        public string Gender { get; set; }

        public DateTime? DOB { get; set; }

        public DateTime? AnniversaryDate { get; set; }

        // ===== DOCUMENT =====

        public string AadhaarNo { get; set; }


        public string PANNo { get; set; }

        public string ImagePath { get; set; }

        // ===== TYPE =====
        public string FileName { get; set; }
        public string PersonType { get; set; }

        public string Remark { get; set; }

        // ===== CHILD =====
        public List<PersonAddress> Addresses { get; set; }
        public List<PersonCommunication> Communications { get; set; }
    }

    public class PersonAddress
    {
        public int Id { get; set; }
        public string AddressType { get; set; }
        public string AddressLine { get; set; }
    }

    public class PersonCommunication
    {
        public int Id { get; set; }
        public string CommunicationType { get; set; }
        public string Value { get; set; }
    }
}