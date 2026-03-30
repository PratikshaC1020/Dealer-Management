using BespokeSoftware.Models;
using BespokeSoftware.Models.DealerModels;
using Microsoft.Data.SqlClient;
using System.Buffers.Text;
using System.Data;
using System.Data.Common;
using static BespokeSoftware.Models.Dealer;

namespace BespokeSoftware.Repository
{
    public class DealerRepository
    {
        private readonly string _connectionString;

        public DealerRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public List<Dealer> GetDealerList()
        {
            List<Dealer> list = new List<Dealer>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = @"
        SELECT 
            d.DealerId,
            d.DealerCode,
            d.DealerName,
            d.OwnerName,
            d.GSTNo,
            d.PANNo,
            d.IsActive,

            pm.PaymentMode,
            w.Day AS WeeklyOff,

            -- 🔥 Dealer Image
            (SELECT TOP 1 ImageBase64 
             FROM T_Image 
             WHERE IdentityID = d.DealerId AND Type = 'Dealer') AS DealerImage,

            -- 🔥 Company Image
            (SELECT TOP 1 ImageBase64 
             FROM T_Image 
             WHERE IdentityID = d.DealerId AND Type = 'Company') AS CompanyImage

        FROM T_Dealer d

        LEFT JOIN T_PaymentMode pm ON d.DefaultPaymentModeId = pm.ID
        LEFT JOIN T_WeeklyOff w ON d.WeeklyOffDayId = w.ID where d.IsActive='1'
        ";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();

                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            Dealer dealer = new Dealer();

                            dealer.DealerId = Convert.ToInt32(rdr["DealerId"]);
                            dealer.DealerCode = rdr["DealerCode"]?.ToString();
                            dealer.DealerName = rdr["DealerName"]?.ToString();
                            dealer.OwnerName = rdr["OwnerName"]?.ToString();
                            dealer.GSTNo = rdr["GSTNo"]?.ToString();
                            dealer.PANNo = rdr["PANNo"]?.ToString();

                            dealer.PaymentMode = rdr["PaymentMode"]?.ToString();
                            dealer.WeeklyOff = rdr["WeeklyOff"]?.ToString();

                            dealer.DealerImage = rdr["DealerImage"] == DBNull.Value ? "" : rdr["DealerImage"].ToString();
                            dealer.CompanyImage = rdr["CompanyImage"] == DBNull.Value ? "" : rdr["CompanyImage"].ToString();

                            dealer.IsActive = Convert.ToBoolean(rdr["IsActive"]);

                            list.Add(dealer);
                        }
                    }
                }
            }

            return list;
        }

        public List<Dealer.Person> GetDealerPersons(int dealerId)
        {
            List<Dealer.Person> list = new List<Dealer.Person>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = @"
        SELECT 
            p.PersonID,
            (ISNULL(p.FirstName,'') + ' ' + ISNULL(p.LastName,'')) AS PersonName,
            p.PersonType
        FROM T_DealerCommunication dc
        INNER JOIN T_Person p ON dc.PersonId = p.PersonID
        WHERE dc.DealerId = @DealerId";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@DealerId", dealerId);

                con.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    list.Add(new Dealer.Person
                    {
                        PersonID = Convert.ToInt32(rdr["PersonID"]),
                        PersonName = rdr["PersonName"].ToString(),
                        PersonTYpe = rdr["PersonType"].ToString()
                    });
                }
            }

            return list;
        }
        public List<Dealer.NoteVM> GetDealerNotes(int dealerId)
        {
            List<Dealer.NoteVM> list = new List<Dealer.NoteVM>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = @"
        SELECT 
            n.NoteText,
            c.Category
        FROM T_DealerNotes n
        LEFT JOIN T_Category c ON n.CategoryId = c.ID
        WHERE n.DealerId = @DealerId";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@DealerId", dealerId);

                con.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    list.Add(new Dealer.NoteVM
                    {
                        NoteText = rdr["NoteText"].ToString(),
                        CategoryName = rdr["Category"].ToString() // optional
                    });
                }
            }

            return list;
        }
        public void InsertDealerFull(DealerViewModel model, IFormFileCollection files)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();

                SqlTransaction trans = con.BeginTransaction();

                try
                {
                    // ================= 1. INSERT DEALER =================
                    string dealerQuery = @"
            INSERT INTO T_Dealer
            (DealerCode, DealerName, OwnerName, GSTNo, PANNo,
             DefaultPaymentModeId, WeeklyOffDayId, IsActive, CreatedDate)
            OUTPUT INSERTED.DealerId
            VALUES
            (@DealerCode, @DealerName, @OwnerName, @GSTNo, @PANNo,
             @PaymentMode, @WeeklyOff, 1, GETDATE())";

                    SqlCommand cmd = new SqlCommand(dealerQuery, con, trans);

                    cmd.Parameters.AddWithValue("@DealerCode", model.Dealer.DealerCode ?? "");
                    cmd.Parameters.AddWithValue("@DealerName", model.Dealer.DealerName ?? "");
                    cmd.Parameters.AddWithValue("@OwnerName", model.Dealer.OwnerName ?? "");
                    cmd.Parameters.AddWithValue("@GSTNo", model.Dealer.GSTNo ?? "");
                    cmd.Parameters.AddWithValue("@PANNo", model.Dealer.PANNo ?? "");
                    cmd.Parameters.AddWithValue("@PaymentMode", model.Dealer.DefaultPaymentModeId);
                    cmd.Parameters.AddWithValue("@WeeklyOff", model.Dealer.WeeklyOffDayId);

                    int dealerId = Convert.ToInt32(cmd.ExecuteScalar());

                    // ================= 2. INSERT ADDRESS =================
                    if (model.Addresses != null)
                    {
                        foreach (var add in model.Addresses)
                        {
                            string addrQuery = @"
                    INSERT INTO T_DealerAddress
                    (DealerId, AddressType, AddressLine)
                    VALUES (@DealerId, @Type, @Line)";

                            SqlCommand addrCmd = new SqlCommand(addrQuery, con, trans);

                            addrCmd.Parameters.AddWithValue("@DealerId", dealerId);
                            addrCmd.Parameters.AddWithValue("@Type", add.AddressType ?? "");
                            addrCmd.Parameters.AddWithValue("@Line", add.AddressLine ?? "");

                            addrCmd.ExecuteNonQuery();
                        }
                    }

                    // ================= 3. INSERT NOTES =================
                    if (model.NotesA != null)
                    {
                        foreach (var note in model.NotesA)
                        {
                            string noteQuery = @"
                    INSERT INTO T_DealerNotes
                    (DealerId, CategoryId, NoteText)
                    VALUES (@DealerId, @Cat, @Text)";

                            SqlCommand noteCmd = new SqlCommand(noteQuery, con, trans);

                            noteCmd.Parameters.AddWithValue("@DealerId", dealerId);
                            noteCmd.Parameters.AddWithValue("@Cat", note.CategoryId);
                            noteCmd.Parameters.AddWithValue("@Text", note.NoteText ?? "");

                            noteCmd.ExecuteNonQuery();
                        }
                    }

                    // ================= 4. INSERT COMMUNICATION (Person Mapping) =================
                    if (model.Dealer.CommunicationIds != null)
                    {
                        foreach (var pid in model.Dealer.CommunicationIds)
                        {
                            string commQuery = @"
                    INSERT INTO T_DealerCommunication
                    (DealerId, PersonId)
                    VALUES (@DealerId, @PersonId)";

                            SqlCommand commCmd = new SqlCommand(commQuery, con, trans);

                            commCmd.Parameters.AddWithValue("@DealerId", dealerId);
                            commCmd.Parameters.AddWithValue("@PersonId", pid);

                            commCmd.ExecuteNonQuery();
                        }
                    }

                    // ================= 5. INSERT IMAGE =================
                    // assume: T_Image (RefId, RefType, FileName)

                    if (files != null && files.Count > 0)
                    {
                        foreach (var file in files)
                        {
                            if (file.Length > 0)
                            {
                                using (var ms = new MemoryStream())
                                {
                                    file.CopyTo(ms);
                                    byte[] fileBytes = ms.ToArray();

                                    string base64 = Convert.ToBase64String(fileBytes);
                                    string fileName = Path.GetFileName(file.FileName);

                                    // 🔥 IMPORTANT LOGIC
                                    string type = "";

                                    if (file.Name == "DealerImage")
                                        type = "Dealer";
                                    else if (file.Name == "CompanyImage")
                                        type = "Company";

                                    string query = @"
                INSERT INTO T_Image
                (Type, IdentityID, ImageBase64, CreatedDate, FileName)
                VALUES
                (@Type, @DealerId, @Base64, GETDATE(), @FileName)";

                                    SqlCommand cmd1 = new SqlCommand(query, con, trans);

                                    cmd1.Parameters.AddWithValue("@Type", type);
                                    cmd1.Parameters.AddWithValue("@DealerId", dealerId);
                                    cmd1.Parameters.AddWithValue("@Base64", base64);
                                    cmd1.Parameters.AddWithValue("@FileName", fileName);

                                    cmd1.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                    // ================= COMMIT =================
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw;
                }
            }
        }
        //public void InsertDealerFull(DealerViewModel model)
        //{
        //    using SqlConnection con = new SqlConnection(_connectionString);

        //    con.Open();

        //    SqlTransaction tran = con.BeginTransaction();

        //    try
        //    {

        //        string dealerQuery = @"INSERT INTO T_Dealer
        //        (DealerCode,DealerName,OwnerName,GSTNo,PANNo,
        //        DepartmentId,WeeklyOffDayId,CategoryId,DefaultPaymentModeId,
        //        Notes,IsActive,CreatedDate,CreatedBy)

        //        VALUES
        //        (@DealerCode,@DealerName,@OwnerName,@GSTNo,@PANNo,
        //        @DepartmentId,@WeeklyOffDayId,@CategoryId,@DefaultPaymentModeId,
        //        @Notes,@IsActive,GETDATE(),1);

        //        SELECT SCOPE_IDENTITY();";

        //        SqlCommand cmd = new SqlCommand(dealerQuery, con, tran);

        //        cmd.Parameters.AddWithValue("@DealerCode", model.Dealer.DealerCode);
        //        cmd.Parameters.AddWithValue("@DealerName", model.Dealer.DealerName);
        //        cmd.Parameters.AddWithValue("@OwnerName", model.Dealer.OwnerName);
        //        cmd.Parameters.AddWithValue("@GSTNo", model.Dealer.GSTNo);
        //        cmd.Parameters.AddWithValue("@PANNo", model.Dealer.PANNo);
        //        cmd.Parameters.AddWithValue("@DepartmentId", model.Dealer.DepartmentId);
        //        cmd.Parameters.AddWithValue("@WeeklyOffDayId", model.Dealer.WeeklyOffDayId);
        //        cmd.Parameters.AddWithValue("@CategoryId", model.Dealer.CategoryId);
        //        cmd.Parameters.AddWithValue("@DefaultPaymentModeId", model.Dealer.DefaultPaymentModeId);
        //        cmd.Parameters.AddWithValue("@Notes", model.Dealer.Notes ?? "");
        //        cmd.Parameters.AddWithValue("@IsActive", model.Dealer.IsActive);

        //        int dealerId = Convert.ToInt32(cmd.ExecuteScalar());

        //        foreach (var addr in model.Addresses)
        //        {
        //            string addrQuery = @"INSERT INTO T_Address
        //    (DealerID,AddressType,AddressLine,CityID,Pincode)
        //    VALUES
        //    (@DealerID,@AddressType,@AddressLine,@CityID,@Pincode)";

        //            SqlCommand addrCmd = new SqlCommand(addrQuery, con, tran);

        //            addrCmd.Parameters.AddWithValue("@DealerID", dealerId);
        //            addrCmd.Parameters.AddWithValue("@AddressType", addr.AddressType);
        //            addrCmd.Parameters.AddWithValue("@AddressLine", addr.AddressLine);
        //            addrCmd.Parameters.AddWithValue("@CityID", addr.CityID);
        //            addrCmd.Parameters.AddWithValue("@Pincode", addr.Pincode);

        //            addrCmd.ExecuteNonQuery();
        //        }

        //        foreach (var com in model.Communications)
        //        {
        //            string commQuery = @"INSERT INTO T_CommunicationDetails
        //    (DealerID,Type,Value,IsActive)
        //    VALUES
        //    (@DealerID,@Type,@Value,1)";

        //            SqlCommand comCmd = new SqlCommand(commQuery, con, tran);

        //            comCmd.Parameters.AddWithValue("@DealerID", dealerId);
        //            comCmd.Parameters.AddWithValue("@Type", com.Type);
        //            comCmd.Parameters.AddWithValue("@Value", com.Value);

        //            comCmd.ExecuteNonQuery();
        //        }

        //        tran.Commit();

        //    }
        //    catch
        //    {
        //        tran.Rollback();
        //        throw;
        //    }
        //}

        public DealerEditVM GetDealerFullById(int dealerId)
        {
            DealerEditVM model = new DealerEditVM();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();

                // ================= DEALER =================
                SqlCommand cmd = new SqlCommand("SELECT * FROM T_Dealer WHERE DealerId=@DealerId", con);
                cmd.Parameters.AddWithValue("@DealerId", dealerId);

                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    model.Dealer = new Models.DealerVM
                    {
                        DealerId = (int)dr["DealerId"],
                        DealerCode = dr["DealerCode"]?.ToString(),
                        DealerName = dr["DealerName"]?.ToString(),
                        OwnerName = dr["OwnerName"]?.ToString(),
                        GSTNo = dr["GSTNo"]?.ToString(),
                        PANNo = dr["PANNo"]?.ToString(),
                        DefaultPaymentModeId = (int)dr["DefaultPaymentModeId"],
                        WeeklyOffDayId = (int)dr["WeeklyOffDayId"],
                        IsActive = (bool)dr["IsActive"]
                    };
                }
                dr.Close();

                // ================= DEALER ADDRESS =================
                cmd = new SqlCommand("SELECT * FROM T_DealerAddress WHERE DealerId=@DealerId", con);
                cmd.Parameters.AddWithValue("@DealerId", dealerId);

                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    model.DealerAddresses.Add(new Models.DealerAddressVM
                    {
                        AddressId = (int)dr["AddressId"],
                        AddressType = dr["AddressType"]?.ToString(),
                        AddressLine = dr["AddressLine"]?.ToString()
                    });
                }
                dr.Close();

                // ================= NOTES =================
                cmd = new SqlCommand("SELECT * FROM T_DealerNotes WHERE DealerId=@DealerId", con);
                cmd.Parameters.AddWithValue("@DealerId", dealerId);

                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    model.DealerNotes.Add(new DealerNoteVM
                    {
                        NoteId = (int)dr["NoteId"],
                        CategoryId = (int)dr["CategoryId"],
                        NoteText = dr["NoteText"]?.ToString(),
                        NoteFor = dr["NoteFor"]?.ToString(),
                        Notedate = dr["NoteDate"] == DBNull.Value
            ? (DateTime?)null
            : Convert.ToDateTime(dr["NoteDate"])
                    });
                }
                dr.Close();

                // ================= DEALER IMAGES =================
                cmd = new SqlCommand("SELECT * FROM T_Image WHERE IdentityID=@DealerId AND Type='Dealer'", con);
                cmd.Parameters.AddWithValue("@DealerId", dealerId);

                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    model.DealerImages.Add(dr["ImageBase64"]?.ToString());
                }
                dr.Close();

                // ================= MAIN OFFICE IMAGE =================
                cmd = new SqlCommand(@"
                    SELECT TOP 1 * FROM T_Image 
                    WHERE IdentityID=@DealerId AND Type='MainOffice'", con);

                cmd.Parameters.AddWithValue("@DealerId", dealerId);

                dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    model.MainOfficeImage = dr["ImageBase64"]?.ToString();
                }

                dr.Close();

                // ================= PERSON =================
                cmd = new SqlCommand("SELECT * FROM T_Person WHERE DealerId=@DealerId", con);
                cmd.Parameters.AddWithValue("@DealerId", dealerId);

                dr = cmd.ExecuteReader();
                List<Models.PersonVM> persons = new List<Models.PersonVM>();

                while (dr.Read())
                {
                    persons.Add(new Models.PersonVM
                    {
                        PersonID = (int)dr["PersonID"],
                        Title = dr["Title"]?.ToString(),
                        FirstName = dr["FirstName"]?.ToString(),
                        MiddleName = dr["MiddleName"]?.ToString(),
                        LastName = dr["LastName"]?.ToString(),
                        Gender = dr["Gender"]?.ToString(),
                        DOB = dr["DOB"] as DateTime?,
                        AnniversaryDate = dr["AnniversaryDate"] as DateTime?,
                        PANNo = dr["PANNo"]?.ToString(),
                        PersonType = dr["PersonType"]?.ToString(),
                        Remark = dr["Remark"]?.ToString()
                    });
                }
                dr.Close();

                // ================= PERSON CHILD =================
                foreach (var p in persons)
                {
                    // ADDRESS
                    SqlCommand cmdAddr = new SqlCommand("SELECT * FROM T_PersonAddress WHERE PersonID=@Pid", con);
                    cmdAddr.Parameters.AddWithValue("@Pid", p.PersonID);

                    SqlDataReader drAddr = cmdAddr.ExecuteReader();
                    while (drAddr.Read())
                    {
                        p.Addresses.Add(new Models.PersonAddressVM
                        {
                            Id = (int)drAddr["ID"],
                            AddressType = drAddr["AddressType"]?.ToString(),
                            AddressLine = drAddr["AddressLine"]?.ToString()
                        });
                    }
                    drAddr.Close();

                    // COMMUNICATION
                    SqlCommand cmdComm = new SqlCommand("SELECT * FROM T_PersonCommunication WHERE PersonID=@Pid", con);
                    cmdComm.Parameters.AddWithValue("@Pid", p.PersonID);

                    SqlDataReader drComm = cmdComm.ExecuteReader();
                    while (drComm.Read())
                    {
                        p.Communications.Add(new Models.PersonCommunicationVM
                        {
                            Id = (int)drComm["ID"],
                            Type = drComm["CommunicationType"]?.ToString(),
                            Label = drComm["CommunicationLabel"]?.ToString(),
                            Value = drComm["Value"]?.ToString()
                        });
                    }
                    drComm.Close();

                    // 🔥 PERSON IMAGES (IMPORTANT FIX)
                    SqlCommand cmdImg = new SqlCommand("SELECT * FROM T_Image WHERE IdentityID=@Pid AND Type='Person'", con);
                    cmdImg.Parameters.AddWithValue("@Pid", p.PersonID);

                    SqlDataReader drImg = cmdImg.ExecuteReader();
                    while (drImg.Read())
                    {
                        p.Images.Add(drImg["ImageBase64"]?.ToString());
                    }
                    drImg.Close();
                }

                model.Persons = persons;
            }

            return model;
        }

        public void UpdateDealerFull(DealerEditVM model)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();

                using (SqlTransaction tran = con.BeginTransaction())
                {
                    try
                    {
                        int dealerId = model.Dealer.DealerId;

                        // ================= FOLDERS =================
                        string dealerFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/dealer");
                        string personFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/person");

                        if (!Directory.Exists(dealerFolder)) Directory.CreateDirectory(dealerFolder);
                        if (!Directory.Exists(personFolder)) Directory.CreateDirectory(personFolder);

                        // ================= DEALER UPDATE =================
                        SqlCommand cmd = new SqlCommand(@"
UPDATE T_Dealer SET
DealerName=@DealerName,
OwnerName=@OwnerName,
GSTNo=@GSTNo,
PANNo=@PANNo,
DefaultPaymentModeId=@PaymentMode,
WeeklyOffDayId=@WeeklyOff,
IsActive=1,
UpdatedDate=GETDATE()
WHERE DealerId=@DealerId", con, tran);

                        cmd.Parameters.AddWithValue("@DealerId", dealerId);
                        cmd.Parameters.AddWithValue("@DealerName", model.Dealer.DealerName ?? "");
                        cmd.Parameters.AddWithValue("@OwnerName", model.Dealer.OwnerName ?? "");
                        cmd.Parameters.AddWithValue("@GSTNo", model.Dealer.GSTNo ?? "");
                        cmd.Parameters.AddWithValue("@PANNo", model.Dealer.PANNo ?? "");
                        cmd.Parameters.AddWithValue("@PaymentMode", model.Dealer.DefaultPaymentModeId);
                        cmd.Parameters.AddWithValue("@WeeklyOff", model.Dealer.WeeklyOffDayId);

                        cmd.ExecuteNonQuery();

                        // ================= DELETE OLD =================

                        // Dealer child
                        Execute("DELETE FROM T_DealerAddress WHERE DealerId=@Id", con, tran, ("@Id", dealerId));
                        Execute("DELETE FROM T_DealerNotes WHERE DealerId=@Id", con, tran, ("@Id", dealerId));

                        // Person child (🔥 FIX)
                        Execute(@"
DELETE FROM T_PersonAddress 
WHERE PersonID IN (SELECT PersonID FROM T_Person WHERE DealerId=@Id)",
                            con, tran, ("@Id", dealerId));

                        Execute(@"
DELETE FROM T_PersonCommunication 
WHERE PersonID IN (SELECT PersonID FROM T_Person WHERE DealerId=@Id)",
                            con, tran, ("@Id", dealerId));

                        Execute(@"
DELETE FROM T_Image 
WHERE Type='Person' AND IdentityID IN 
(SELECT PersonID FROM T_Person WHERE DealerId=@Id)",
                            con, tran, ("@Id", dealerId));

                        // Person
                        Execute("DELETE FROM T_Person WHERE DealerId=@Id", con, tran, ("@Id", dealerId));

                        // Dealer Images
                        Execute("DELETE FROM T_Image WHERE IdentityID=@Id AND Type='Dealer'", con, tran, ("@Id", dealerId));

                        Execute("DELETE FROM T_Image WHERE IdentityID=@Id AND Type='MainOffice'",
    con, tran, ("@Id", dealerId));

                        // ================= DEALER IMAGES =================

                        // EXISTING
                        foreach (var img in model.DealerImages ?? new List<string>())
                        {
                            ExecuteInsert(@"
INSERT INTO T_Image(Type, IdentityID, ImageBase64, CreatedDate)
VALUES('Dealer',@Id,@Img,GETDATE())",
                                con, tran,
                                ("@Id", dealerId),
                                ("@Img", img));
                        }

                        // NEW FILES
                        foreach (var file in model.NewDealerImages ?? new List<IFormFile>())
                        {
                            string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                            string fullPath = Path.Combine(dealerFolder, fileName);

                            using (var stream = new FileStream(fullPath, FileMode.Create))
                            {
                                file.CopyTo(stream);
                            }

                            string dbPath = "/uploads/dealer/" + fileName;

                            ExecuteInsert(@"
INSERT INTO T_Image(Type, IdentityID, ImageBase64, CreatedDate)
VALUES('Dealer',@Id,@Img,GETDATE())",
                                con, tran,
                                ("@Id", dealerId),
                                ("@Img", dbPath));
                        }

                        // ================= MAIN OFFICE IMAGE =================

                        // NEW FILE
                        if (model.MainOfficeImageFile != null)
                        {
                            string fileName = Guid.NewGuid() + Path.GetExtension(model.MainOfficeImageFile.FileName);
                            string fullPath = Path.Combine(dealerFolder, fileName);

                            using (var stream = new FileStream(fullPath, FileMode.Create))
                            {
                                model.MainOfficeImageFile.CopyTo(stream);
                            }

                            string dbPath = "/uploads/dealer/" + fileName;

                            ExecuteInsert(@"
    INSERT INTO T_Image(Type, IdentityID, ImageBase64, CreatedDate)
    VALUES('MainOffice',@Id,@Img,GETDATE())",
                                con, tran,
                                ("@Id", dealerId),
                                ("@Img", dbPath));
                        }

                        // EXISTING KEEP
                        else if (!string.IsNullOrEmpty(model.MainOfficeImagePath) && !model.IsMainRemoved)
                        {
                            ExecuteInsert(@"
    INSERT INTO T_Image(Type, IdentityID, ImageBase64, CreatedDate)
    VALUES('MainOffice',@Id,@Img,GETDATE())",
                                con, tran,
                                ("@Id", dealerId),
                                ("@Img", model.MainOfficeImagePath));
                        }

                        // REMOVE → do nothing (already deleted above)

                        // ================= ADDRESS =================
                        foreach (var a in model.DealerAddresses ?? new List<Models.DealerAddressVM>())
                        {
                            ExecuteInsert(@"
INSERT INTO T_DealerAddress (DealerId, AddressType, AddressLine, CreatedDate)
VALUES (@DealerId,@Type,@Line,GETDATE())",
                                con, tran,
                                ("@DealerId", dealerId),
                                ("@Type", a.AddressType ?? ""),
                                ("@Line", a.AddressLine ?? ""));
                        }

                        // ================= NOTES =================
                        foreach (var n in model.DealerNotes ?? new List<DealerNoteVM>())
                        {
                            ExecuteInsert(@"
INSERT INTO T_DealerNotes 
(DealerId, CategoryId, NoteText, CreatedDate, NoteFor, NoteDate)
VALUES (@DealerId,@Cat,@Text,GETDATE(),@NoteFor,@NoteDate)",
                                con, tran,
                                ("@DealerId", dealerId),
                                ("@Cat", n.CategoryId),
                                ("@Text", n.NoteText ?? ""),
                                ("@NoteFor", n.NoteFor ?? ""),
                                ("@NoteDate", (object)n.Notedate ?? DBNull.Value));
                        }

                        // ================= PERSON =================
                        foreach (var p in model.Persons ?? new List<Models.PersonVM>())
                        {
                            SqlCommand pCmd = new SqlCommand(@"
INSERT INTO T_Person
(Title,FirstName,MiddleName,LastName,Gender,DOB,AnniversaryDate,PANNo,PersonType,Remark,DealerId,CreatedDate)
OUTPUT INSERTED.PersonID
VALUES (@Title,@F,@M,@L,@G,@DOB,@Ann,@PAN,@Type,@Remark,@DealerId,GETDATE())",
                                con, tran);

                            pCmd.Parameters.AddWithValue("@Title", p.Title ?? "");
                            pCmd.Parameters.AddWithValue("@F", p.FirstName ?? "");
                            pCmd.Parameters.AddWithValue("@M", p.MiddleName ?? "");
                            pCmd.Parameters.AddWithValue("@L", p.LastName ?? "");
                            pCmd.Parameters.AddWithValue("@G", p.Gender ?? "");
                            pCmd.Parameters.AddWithValue("@DOB", (object)p.DOB ?? DBNull.Value);
                            pCmd.Parameters.AddWithValue("@Ann", (object)p.AnniversaryDate ?? DBNull.Value);
                            pCmd.Parameters.AddWithValue("@PAN", p.PANNo ?? "");
                            pCmd.Parameters.AddWithValue("@Type", p.PersonType ?? "");
                            pCmd.Parameters.AddWithValue("@Remark", p.Remark ?? "");
                            pCmd.Parameters.AddWithValue("@DealerId", dealerId);

                            int personId = Convert.ToInt32(pCmd.ExecuteScalar());

                            // ADDRESS
                            foreach (var pa in p.Addresses ?? new List<Models.PersonAddressVM>())
                            {
                                ExecuteInsert(@"
INSERT INTO T_PersonAddress (PersonID, AddressType, AddressLine)
VALUES (@Pid,@Type,@Line)",
                                    con, tran,
                                    ("@Pid", personId),
                                    ("@Type", pa.AddressType ?? ""),
                                    ("@Line", pa.AddressLine ?? ""));
                            }

                            // COMMUNICATION
                            foreach (var c in p.Communications ?? new List<Models.PersonCommunicationVM>())
                            {
                                ExecuteInsert(@"
INSERT INTO T_PersonCommunication (PersonID,CommunicationType,CommunicationLabel,Value)
VALUES (@Pid,@Type,@Label,@Value)",
                                    con, tran,
                                    ("@Pid", personId),
                                    ("@Type", c.Type ?? ""),
                                    ("@Label", c.Label ?? ""),
                                    ("@Value", c.Value ?? ""));
                            }

                            // ================= PERSON IMAGES =================

                            // EXISTING
                            foreach (var img in p.Images ?? new List<string>())
                            {
                                ExecuteInsert(@"
INSERT INTO T_Image(Type, IdentityID, ImageBase64, CreatedDate)
VALUES('Person',@Pid,@Img,GETDATE())",
                                    con, tran,
                                    ("@Pid", personId),
                                    ("@Img", img));
                            }

                            // NEW FILES
                            foreach (var file in p.NewImages ?? new List<IFormFile>())
                            {
                                string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                                string fullPath = Path.Combine(personFolder, fileName);

                                using (var stream = new FileStream(fullPath, FileMode.Create))
                                {
                                    file.CopyTo(stream);
                                }

                                string dbPath = "/uploads/person/" + fileName;

                                ExecuteInsert(@"
INSERT INTO T_Image(Type, IdentityID, ImageBase64, CreatedDate)
VALUES('Person',@Pid,@Img,GETDATE())",
                                    con, tran,
                                    ("@Pid", personId),
                                    ("@Img", dbPath));
                            }
                        }

                        tran.Commit();
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }

        public void Execute(string query, SqlConnection con, SqlTransaction tran, params (string, object)[] parameters)
        {
            SqlCommand cmd = new SqlCommand(query, con, tran);

            foreach (var p in parameters)
            {
                cmd.Parameters.AddWithValue(p.Item1, p.Item2 ?? DBNull.Value);
            }

            cmd.ExecuteNonQuery();
        }

        private void ExecuteInsert(string query, SqlConnection con, SqlTransaction tran, params (string, object)[] parameters)
        {
            SqlCommand cmd = new SqlCommand(query, con, tran);

            foreach (var p in parameters)
                cmd.Parameters.AddWithValue(p.Item1, p.Item2 ?? DBNull.Value);

            cmd.ExecuteNonQuery();
        }

        //public void DeleteDealer(int dealerId)
        //{
        //    using SqlConnection con = new SqlConnection(_connectionString);

        //    con.Open();

        //    try
        //    {
        //        string query = @"
        //        UPDATE T_Dealer
        //        SET IsActive = 0,
        //            UpdatedDate = GETDATE()
        //        WHERE DealerId = @DealerID";

        //        SqlCommand cmd = new SqlCommand(query, con);

        //        cmd.Parameters.AddWithValue("@DealerID", dealerId);

        //        cmd.ExecuteNonQuery();
        //    }
        //    catch
        //    {
        //        throw;
        //    }
        //}

        public void DeleteDealer(int dealerId)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("SP_DeleteDealerFull", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@DealerId", SqlDbType.Int).Value = dealerId;

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }
        public List<modelDepartment> GetDepartments()
        {
            List<modelDepartment> list = new List<modelDepartment>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = "SELECT DepID, Department FROM T_Department WHERE IsDelete=0";

                SqlCommand cmd = new SqlCommand(query, con);

                con.Open();

                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    list.Add(new modelDepartment
                    {
                        DepID = Convert.ToInt32(rdr["DepID"]),
                        DepartmentName = rdr["Department"].ToString()
                    });
                }
            }

            return list;
        }


        public List<Models.Dealer.Person> GetPersonData()
        {
            List<Models.Dealer.Person> list = new List<Models.Dealer.Person>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT PersonID, PersonType,
                           ISNULL(Title,'') + ' ' +
                           ISNULL(FirstName,'') + ' ' +
                           ISNULL(MiddleName,'') + ' ' +
                           ISNULL(LastName,'') AS FullName
                    FROM T_Person";

                SqlCommand cmd = new SqlCommand(query, con);

                con.Open();

                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    list.Add(new Models.Dealer.Person
                    {
                        PersonID = Convert.ToInt32(rdr["PersonID"]),

                        // 👉 Text: PersonType - FullName
                        PersonName = rdr["PersonType"].ToString()
                                     + " - " + rdr["FullName"].ToString().Trim()
                    });
                }
            }

            return list;
        }

        public List<modelCategory> GetCategories()
        {
            List<modelCategory> list = new List<modelCategory>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = "SELECT ID, Category FROM T_Category WHERE IsDelete=0";

                SqlCommand cmd = new SqlCommand(query, con);

                con.Open();

                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    list.Add(new modelCategory
                    {
                        ID = Convert.ToInt32(rdr["ID"]),
                        CategoryName = rdr["Category"].ToString()
                    });
                }
            }

            return list;
        }

        public List<DrpPaymentMode> GetPaymentModes()
        {
            List<DrpPaymentMode> list = new List<DrpPaymentMode>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = "SELECT ID, PaymentMode FROM T_PaymentMode WHERE IsDelete=0";

                SqlCommand cmd = new SqlCommand(query, con);

                con.Open();

                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    list.Add(new DrpPaymentMode
                    {
                        ID = Convert.ToInt32(rdr["ID"]),
                        PaymentModeName = rdr["PaymentMode"].ToString()
                    });
                }
            }

            return list;
        }

        public List<DrpWeeklyOff> GetWeeklyOffDays()
        {
            List<DrpWeeklyOff> list = new List<DrpWeeklyOff>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = "SELECT ID, Day FROM T_WeeklyOff";

                SqlCommand cmd = new SqlCommand(query, con);

                con.Open();

                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    list.Add(new DrpWeeklyOff
                    {
                        ID = Convert.ToInt32(rdr["ID"]),
                        Day = rdr["Day"].ToString()
                    });
                }
            }

            return list;
        }

        public List<State> GetStates()
        {
            List<State> list = new List<State>();

            using SqlConnection con = new SqlConnection(_connectionString);

            string query = "SELECT StateID,State FROM T_State";

            SqlCommand cmd = new SqlCommand(query, con);

            con.Open();

            SqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                list.Add(new State
                {
                    StateID = Convert.ToInt32(rdr["StateID"]),
                    StateName = rdr["State"].ToString()
                });
            }

            return list;
        }

        public List<City> GetCitiesByState(int stateId)
        {
            List<City> list = new List<City>();

            using SqlConnection con = new SqlConnection(_connectionString);

            string query = "SELECT CityID,City FROM T_City WHERE StateID=@StateID";

            SqlCommand cmd = new SqlCommand(query, con);

            cmd.Parameters.AddWithValue("@StateID", stateId);

            con.Open();

            SqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                list.Add(new City
                {
                    CityID = Convert.ToInt32(rdr["CityID"]),
                    CityName = rdr["City"].ToString()
                });
            }

            return list;
        }

        public string GetDealerCode()
        {
            string code = "";

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = @"SELECT 
        'DLR' + RIGHT('000' + CAST(ISNULL(MAX(CAST(SUBSTRING(DealerCode,4,10) AS INT)),0) + 1 AS VARCHAR),3)
        FROM T_Dealer";

                SqlCommand cmd = new SqlCommand(query, con);

                con.Open();

                code = cmd.ExecuteScalar().ToString();
            }

            return code;
        }

        public List<Address> GetDealerAddress(int dealerId)
        {
            List<Address> list = new List<Address>();

            using SqlConnection con = new SqlConnection(_connectionString);

            string query = @"
        SELECT 
            AddressType,
            AddressLine
        FROM T_DealerAddress
        WHERE DealerId = @DealerID";

            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@DealerID", dealerId);

            con.Open();

            SqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                list.Add(new Address
                {
                    AddressType = rdr["AddressType"]?.ToString(),
                    AddressLine = rdr["AddressLine"]?.ToString()
                });
            }

            return list;
        }

        public List<CommunicationDetails> GetDealerCommunication(int dealerId)
        {
            List<CommunicationDetails> list = new List<CommunicationDetails>();

            using SqlConnection con = new SqlConnection(_connectionString);

            string query = @"
        SELECT 
            p.PersonID,
            (ISNULL(p.FirstName,'') + ' ' + ISNULL(p.LastName,'')) AS PersonName,
            p.PersonType
        FROM T_DealerCommunication dc
        INNER JOIN T_Person p ON dc.PersonId = p.PersonID
        WHERE dc.DealerId = @DealerID";

            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@DealerID", dealerId);

            con.Open();

            SqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                list.Add(new CommunicationDetails
                {
                    // adjust as per your model
                    CommunicationID = Convert.ToInt32(rdr["PersonID"]),
                    Type = rdr["PersonType"]?.ToString(),
                    Value = rdr["PersonName"]?.ToString()
                });
            }

            return list;
        }

        public async Task<bool> SaveDealerFull(DealerViewModel model)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                await con.OpenAsync();
                SqlTransaction tran = con.BeginTransaction();

                try
                {
                    int dealerId = 0;

                    string ownerName = model.Persons?
                        .FirstOrDefault(x => x.Type == "Owner") is var owner && owner != null
                        ? $"{owner.First} {owner.Last}".Trim()
                        : null;

                    // ================= FOLDERS =================
                    string dealerFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/dealer");
                    string personFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/person");

                    if (!Directory.Exists(dealerFolder)) Directory.CreateDirectory(dealerFolder);
                    if (!Directory.Exists(personFolder)) Directory.CreateDirectory(personFolder);

                    // ================= MAIN DEALER IMAGE =================
                    string dealerFileName = null;
                    string dealerFilePath = null;

                    var firstDealerImg = model.DealerImages?.FirstOrDefault();

                    if (firstDealerImg != null)
                    {
                        dealerFileName = Guid.NewGuid() + Path.GetExtension(firstDealerImg.FileName);
                        string fullPath = Path.Combine(dealerFolder, dealerFileName);

                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            await firstDealerImg.CopyToAsync(stream);
                        }

                        dealerFilePath = "/uploads/dealer/" + dealerFileName;
                    }

                    // ================= DEALER INSERT =================
                    using (SqlCommand cmd = new SqlCommand("sp_InsertDealerWithImage", con, tran))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@DealerCode", model.Dealer.DealerCode);
                        cmd.Parameters.AddWithValue("@DealerName", model.Dealer.DealerName);
                        cmd.Parameters.AddWithValue("@OwnerName", ownerName ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@GSTNo", model.Dealer.GSTNo ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@PANNo", model.Dealer.PANNo ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@DefaultPaymentModeId", model.Dealer.DefaultPaymentModeId);
                        cmd.Parameters.AddWithValue("@WeeklyOffDayId", model.Dealer.WeeklyOffDayId);
                        cmd.Parameters.AddWithValue("@CreatedBy", 1);

                        cmd.Parameters.AddWithValue("@ImageBase64", dealerFilePath ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@FileName", dealerFileName ?? (object)DBNull.Value);

                        dealerId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    }

                    // ================= DEALER IMAGE SAVE =================

                     dealerFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/dealer");

                    if (!Directory.Exists(dealerFolder))
                    {
                        Directory.CreateDirectory(dealerFolder);
                    }

                    // ================= FIRST IMAGE =================
                    if (model.DealerImages != null && model.DealerImages.Count > 0)
                    {
                        var firstImg = model.DealerImages[0];

                        string fileName = Guid.NewGuid() + Path.GetExtension(firstImg.FileName);
                        string fullPath = Path.Combine(dealerFolder, fileName);

                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            await firstImg.CopyToAsync(stream);
                        }

                        string dbPath = "/uploads/dealer/" + fileName;

                        // TYPE FIX (FIRST IMAGE)
                        string firstType = "Dealer";

                        if (model.DealerImageTypes != null && model.DealerImageTypes.Count > 0)
                        {
                            firstType = model.DealerImageTypes[0] == "MainOffice"
                                ? "MainOffice"
                                : "Dealer";
                        }

                        using (SqlCommand cmd = new SqlCommand(
                            "INSERT INTO T_Image(Type, IdentityID, ImageBase64, CreatedDate, FileName) VALUES(@Type,@DealerId,@ImageBase64,GETDATE(),@FileName)",
                            con, tran))
                        {
                            cmd.Parameters.AddWithValue("@DealerId", dealerId);
                            cmd.Parameters.AddWithValue("@ImageBase64", dbPath);
                            cmd.Parameters.AddWithValue("@Type", firstType);
                            cmd.Parameters.AddWithValue("@FileName", fileName);

                            await cmd.ExecuteNonQueryAsync();
                        }
                    }

                    // ================= EXTRA IMAGES =================
                    if (model.DealerImages != null && model.DealerImages.Count > 1)
                    {
                        for (int i = 1; i < model.DealerImages.Count; i++)
                        {
                            var img = model.DealerImages[i];

                            string fileName = Guid.NewGuid() + Path.GetExtension(img.FileName);
                            string fullPath = Path.Combine(dealerFolder, fileName);

                            using (var stream = new FileStream(fullPath, FileMode.Create))
                            {
                                await img.CopyToAsync(stream);
                            }

                            string dbPath = "/uploads/dealer/" + fileName;

                            //  TYPE FIX
                            string type = "Dealer";

                            if (model.DealerImageTypes != null && model.DealerImageTypes.Count > i)
                            {
                                type = model.DealerImageTypes[i] == "MainOffice"
                                    ? "MainOffice"
                                    : "Dealer";
                            }

                            using (SqlCommand cmd = new SqlCommand(
                                "INSERT INTO T_Image(Type, IdentityID, ImageBase64, CreatedDate, FileName) VALUES(@Type,@DealerId,@ImageBase64,GETDATE(),@FileName)",
                                con, tran))
                            {
                                cmd.Parameters.AddWithValue("@DealerId", dealerId);
                                cmd.Parameters.AddWithValue("@ImageBase64", dbPath);
                                cmd.Parameters.AddWithValue("@Type", type);
                                cmd.Parameters.AddWithValue("@FileName", fileName);

                                await cmd.ExecuteNonQueryAsync();
                            }
                        }
                    }                    // ================= DEALER ADDRESS =================
                    foreach (var addr in model.DealerAddresses)
                    {
                        using (SqlCommand cmd = new SqlCommand("sp_InsertDealerAddress", con, tran))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;

                            cmd.Parameters.AddWithValue("@DealerId", dealerId);
                            cmd.Parameters.AddWithValue("@AddressType", addr.AddressType ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@AddressLine", addr.AddressLine ?? (object)DBNull.Value);

                            await cmd.ExecuteNonQueryAsync();
                        }
                    }

                    // ================= DEALER NOTES =================
                    foreach (var note in model.DealerNotes)
                    {
                        using (SqlCommand cmd = new SqlCommand("sp_InsertDealerNote", con, tran))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;

                            cmd.Parameters.AddWithValue("@DealerId", dealerId);
                            cmd.Parameters.AddWithValue("@CategoryId", note.CategoryId);
                            cmd.Parameters.AddWithValue("@NoteText", note.NoteText ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@NoteFor", note.NoteFor ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@NoteDate", note.NoteDate ?? (object)DBNull.Value);

                            await cmd.ExecuteNonQueryAsync();
                        }
                    }

                    // ================= PERSON =================
                    foreach (var p in model.Persons)
                    {
                        int personId = 0;

                        var firstComm = p.Communications?.FirstOrDefault();
                        var firstAddr = p.Addresses?.FirstOrDefault();
                        var firstImg = p.Images?.FirstOrDefault();

                        string personFileName = null;
                        string personFilePath = null;

                        // 🔥 PERSON IMAGE SAVE (PATH)
                        if (firstImg != null)
                        {
                            personFileName = Guid.NewGuid() + Path.GetExtension(firstImg.FileName);
                            string fullPath = Path.Combine(personFolder, personFileName);

                            using (var stream = new FileStream(fullPath, FileMode.Create))
                            {
                                await firstImg.CopyToAsync(stream);
                            }

                            personFilePath = "/uploads/person/" + personFileName;
                        }

                        // ===== INSERT PERSON =====
                        using (SqlCommand cmd = new SqlCommand("sp_InsertPersonFull", con, tran))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;

                            cmd.Parameters.AddWithValue("@Title", p.Title ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@FirstName", p.First);
                            cmd.Parameters.AddWithValue("@MiddleName", p.Middle ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@LastName", p.Last ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Gender", p.Gender ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@DOB", p.Dob ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@AnniversaryDate", p.Anniversary ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@PANNo", p.Pan ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@PersonType", p.Type ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Remark", p.Remark ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@DealerId", dealerId);

                            cmd.Parameters.AddWithValue("@CommunicationType", firstComm?.Type ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Value", firstComm?.Value ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@CommunicationLabel", firstComm?.Label ?? (object)DBNull.Value);

                            cmd.Parameters.AddWithValue("@AddressType", firstAddr?.Type ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@AddressLine", firstAddr?.Address ?? (object)DBNull.Value);

                            // 🔥 CHANGE HERE
                            cmd.Parameters.AddWithValue("@ImageBase64", personFilePath ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@FileName", personFileName ?? (object)DBNull.Value);

                            personId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                        }

                        // ================= EXTRA PERSON IMAGES =================
                        if (p.Images != null && p.Images.Count > 1)
                        {
                            foreach (var img in p.Images.Skip(1))
                            {
                                string fileName = Guid.NewGuid() + Path.GetExtension(img.FileName);
                                string fullPath = Path.Combine(personFolder, fileName);

                                using (var stream = new FileStream(fullPath, FileMode.Create))
                                {
                                    await img.CopyToAsync(stream);
                                }

                                string dbPath = "/uploads/person/" + fileName;

                                using (SqlCommand cmd = new SqlCommand(
                                    "INSERT INTO T_Image(Type, IdentityID, ImageBase64, CreatedDate, FileName) VALUES('Person',@PersonId,@ImageBase64,GETDATE(),@FileName)",
                                    con, tran))
                                {
                                    cmd.Parameters.AddWithValue("@PersonId", personId);
                                    cmd.Parameters.AddWithValue("@ImageBase64", dbPath);
                                    cmd.Parameters.AddWithValue("@FileName", fileName);

                                    await cmd.ExecuteNonQueryAsync();
                                }
                            }
                        }
                    }

                    tran.Commit();
                    return true;
                }
                catch (Exception)
                {
                    tran.Rollback();
                    throw;
                }
            }
        }

        public DealerFullViewModel GetDealerFullDetails(int dealerId)
        {
            DealerFullViewModel model = new DealerFullViewModel();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("SP_GetDealerFullDetails", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@DealerId", dealerId);

                con.Open();

                SqlDataReader rdr = cmd.ExecuteReader();


                if (rdr.Read())
                {
                    model.Dealer = new Models.DealerModels.DealerVM
                    {
                        DealerId = Convert.ToInt32(rdr["DealerId"]),
                        DealerName = rdr["DealerName"]?.ToString(),
                        OwnerName = rdr["OwnerName"]?.ToString(),
                        GSTNo = rdr["GSTNo"]?.ToString(),
                        PANNo = rdr["PANNo"]?.ToString(),
                        PaymentMode = rdr["PaymentMode"]?.ToString(),
                        WeeklyOff = rdr["WeeklyOff"]?.ToString()
                    };
                }


                if (rdr.NextResult())
                {
                    while (rdr.Read())
                    {
                        model.Addresses.Add(new Models.DealerModels.DealerAddressVM
                        {
                            AddressType = rdr["AddressType"]?.ToString(),
                            AddressLine = rdr["AddressLine"]?.ToString()
                        });
                    }
                }


                if (rdr.NextResult())
                {
                    while (rdr.Read())
                    {
                        model.Notes.Add(new DealerNotesVM
                        {
                            NoteText = rdr["NoteText"]?.ToString(),
                            NoteFor = rdr["NoteFor"]?.ToString(),
                            NoteDate = rdr["NoteDate"] == DBNull.Value
                                        ? (DateTime?)null
                                        : Convert.ToDateTime(rdr["NoteDate"]),
                            CategoryId = Convert.ToInt32(rdr["CategoryId"]),
                            CategoryName = rdr["CategoryName"]?.ToString() // ✅ FIX
                        });
                    }
                }


                if (rdr.NextResult())
                {
                    while (rdr.Read())
                    {
                        model.Persons.Add(new Models.DealerModels.PersonVM
                        {
                            PersonID = Convert.ToInt32(rdr["PersonID"]),
                            Title = rdr["Title"]?.ToString(),
                            FirstName = rdr["FirstName"]?.ToString(),
                            MiddleName = rdr["MiddleName"]?.ToString(),
                            LastName = rdr["LastName"]?.ToString(),
                            Gender = rdr["Gender"]?.ToString(),
                            DOB = rdr["DOB"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rdr["DOB"]),
                            AnniversaryDate = rdr["AnniversaryDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(rdr["AnniversaryDate"]),
                            AadhaarNo = rdr["AadhaarNo"]?.ToString(),
                            PANNo = rdr["PANNo"]?.ToString(),
                            PersonType = rdr["PersonType"]?.ToString(),
                            Remark = rdr["Remark"]?.ToString(),
                            DealerCode = rdr["DealerCode"]?.ToString(),
                            DealerId = Convert.ToInt32(rdr["DealerId"])
                        });
                    }
                }


                if (rdr.NextResult())
                {
                    while (rdr.Read())
                    {
                        model.PersonAddresses.Add(new Models.DealerModels.PersonAddressVM
                        {
                            PersonID = Convert.ToInt32(rdr["PersonID"]),
                            AddressLine = rdr["AddressLine"]?.ToString()
                        });
                    }
                }


                if (rdr.NextResult())
                {
                    while (rdr.Read())
                    {
                        model.Communications.Add(new Models.DealerModels.PersonCommunicationVM
                        {
                            PersonID = Convert.ToInt32(rdr["PersonID"]),
                            Value = rdr["Value"]?.ToString()
                        });
                    }
                }

                if (rdr.NextResult())
                {
                    while (rdr.Read())
                    {
                        model.Images.Add(new ImageVM
                        {
                            IdentityID = Convert.ToInt32(rdr["IdentityID"]),
                            Type = rdr["Type"]?.ToString(),
                            ImageBase64 = rdr["ImageBase64"]?.ToString()
                        });
                    }
                }
                if (rdr.NextResult())
                {
                    while (rdr.Read())
                    {
                        model.ImagesOwner.Add(new ImageVMOwner
                        {
                            IdentityID = Convert.ToInt32(rdr["IdentityID"]),
                            Type = rdr["Type"]?.ToString(),
                            ImageBase64 = rdr["ImageBase64"]?.ToString()
                        });
                    }
                }
            }

            return model;
        }

        public void AddCategory(string category)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = "insert into T_Category(Category,IsDelete) values(@cat,0)";

                SqlCommand cmd = new SqlCommand(query, con);

                cmd.Parameters.AddWithValue("@cat", category);

                con.Open();

                cmd.ExecuteNonQuery();
            }
        }



    }
}