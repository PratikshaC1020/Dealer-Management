using BespokeSoftware.Models;
using Microsoft.Data.SqlClient;
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

        public DealerViewModel GetDealerFullById(int dealerId)
        {
            DealerViewModel model = new DealerViewModel();

            using SqlConnection con = new SqlConnection(_connectionString);

            con.Open();

            // Dealer
            string dealerQuery = "SELECT * FROM T_Dealer WHERE DealerID=@DealerID";

            SqlCommand cmd = new SqlCommand(dealerQuery, con);
            cmd.Parameters.AddWithValue("@DealerID", dealerId);

            SqlDataReader rdr = cmd.ExecuteReader();

            if (rdr.Read())
            {
                model.Dealer = new Dealer
                {
                    DealerId = Convert.ToInt32(rdr["DealerID"]),
                    DealerCode = rdr["DealerCode"].ToString(),
                    DealerName = rdr["DealerName"].ToString(),
                    OwnerName = rdr["OwnerName"].ToString(),
                    GSTNo = rdr["GSTNo"].ToString(),
                    PANNo = rdr["PANNo"].ToString(),
                    DepartmentId = Convert.ToInt32(rdr["DepartmentId"]),
                    CategoryId = Convert.ToInt32(rdr["CategoryId"]),
                    WeeklyOffDayId = Convert.ToInt32(rdr["WeeklyOffDayId"]),
                    DefaultPaymentModeId = Convert.ToInt32(rdr["DefaultPaymentModeId"]),
                    Notes = rdr["Notes"].ToString(),
                    IsActive = Convert.ToBoolean(rdr["IsActive"])
                };
            }

            rdr.Close();

            // Addresses
            model.Addresses = new List<Address>();

            string addrQuery = @"
                    SELECT 
                    A.ID,
                    A.AddressType,
                    A.AddressLine,
                    A.CityID,
                    A.Pincode,
                    C.City as CityName,
                    C.StateID
                    FROM T_Address A
                    INNER JOIN T_City C ON A.CityID = C.CityID
                    WHERE A.DealerID=@DealerID";
            SqlCommand addrCmd = new SqlCommand(addrQuery, con);
            addrCmd.Parameters.AddWithValue("@DealerID", dealerId);

            SqlDataReader addrRdr = addrCmd.ExecuteReader();

            while (addrRdr.Read())
            {
                model.Addresses.Add(new Address
                {
                    ID = Convert.ToInt32(addrRdr["ID"]),
                    AddressType = addrRdr["AddressType"].ToString(),
                    AddressLine = addrRdr["AddressLine"].ToString(),
                    CityID = Convert.ToInt32(addrRdr["CityID"]),
                    StateID = Convert.ToInt32(addrRdr["StateID"]),
                    CityName = addrRdr["CityName"].ToString(),
                    Pincode = addrRdr["Pincode"].ToString()
                });
            }

            addrRdr.Close();

            // Communication
            model.Communications = new List<CommunicationDetails>();

            string commQuery = "SELECT * FROM T_CommunicationDetails WHERE DealerID=@DealerID";

            SqlCommand commCmd = new SqlCommand(commQuery, con);
            commCmd.Parameters.AddWithValue("@DealerID", dealerId);

            SqlDataReader commRdr = commCmd.ExecuteReader();

            while (commRdr.Read())
            {
                model.Communications.Add(new CommunicationDetails
                {
                    CommunicationID = Convert.ToInt32(commRdr["CommunicationID"]),
                    Type = commRdr["Type"].ToString(),
                    Value = commRdr["Value"].ToString(),
                    IsActive = Convert.ToBoolean(commRdr["IsActive"])
                });
            }

            return model;
        }

        public void UpdateDealerFull(DealerViewModel model)
        {
            using SqlConnection con = new SqlConnection(_connectionString);

            con.Open();

            SqlTransaction tran = con.BeginTransaction();

            try
            {

                string dealerQuery = @"UPDATE T_Dealer SET
                    DealerName=@DealerName,
                    OwnerName=@OwnerName,
                    GSTNo=@GSTNo,
                    PANNo=@PANNo,
                    DepartmentId=@DepartmentId,
                    CategoryId=@CategoryId,
                    WeeklyOffDayId=@WeeklyOffDayId,
                    DefaultPaymentModeId=@DefaultPaymentModeId,
                    Notes=@Notes
                    WHERE DealerID=@DealerID";

                SqlCommand cmd = new SqlCommand(dealerQuery, con, tran);

                cmd.Parameters.AddWithValue("@DealerID", model.Dealer.DealerId);
                cmd.Parameters.AddWithValue("@DealerName", model.Dealer.DealerName);
                cmd.Parameters.AddWithValue("@OwnerName", model.Dealer.OwnerName);
                cmd.Parameters.AddWithValue("@GSTNo", model.Dealer.GSTNo);
                cmd.Parameters.AddWithValue("@PANNo", model.Dealer.PANNo);
                cmd.Parameters.AddWithValue("@DepartmentId", model.Dealer.DepartmentId);
                cmd.Parameters.AddWithValue("@CategoryId", model.Dealer.CategoryId);
                cmd.Parameters.AddWithValue("@WeeklyOffDayId", model.Dealer.WeeklyOffDayId);
                cmd.Parameters.AddWithValue("@DefaultPaymentModeId", model.Dealer.DefaultPaymentModeId);
                cmd.Parameters.AddWithValue("@Notes", model.Dealer.Notes ?? "");
                cmd.Parameters.AddWithValue("@UpdatedOn", DateTime.Now);

                cmd.ExecuteNonQuery();


                // Delete old address
                SqlCommand delAddr = new SqlCommand("DELETE FROM T_Address WHERE DealerID=@DealerID", con, tran);
                delAddr.Parameters.AddWithValue("@DealerID", model.Dealer.DealerId);
                delAddr.ExecuteNonQuery();


                foreach (var addr in model.Addresses)
                {

                    string addrQuery = @"INSERT INTO T_Address
                    (DealerID,AddressType,AddressLine,CityID,Pincode)
                    VALUES
                    (@DealerID,@AddressType,@AddressLine,@CityID,@Pincode)";

                    SqlCommand addrCmd = new SqlCommand(addrQuery, con, tran);

                    addrCmd.Parameters.AddWithValue("@DealerID", model.Dealer.DealerId);
                    addrCmd.Parameters.AddWithValue("@AddressType", addr.AddressType);
                    addrCmd.Parameters.AddWithValue("@AddressLine", addr.AddressLine);
                    addrCmd.Parameters.AddWithValue("@CityID", addr.CityID);
                    addrCmd.Parameters.AddWithValue("@Pincode", addr.Pincode);

                    addrCmd.ExecuteNonQuery();
                }


                // Delete old communication
                SqlCommand delComm = new SqlCommand("DELETE FROM T_CommunicationDetails WHERE DealerID=@DealerID", con, tran);
                delComm.Parameters.AddWithValue("@DealerID", model.Dealer.DealerId);
                delComm.ExecuteNonQuery();


                foreach (var com in model.Communications)
                {

                    string commQuery = @"INSERT INTO T_CommunicationDetails
                    (DealerID,Type,Value,IsActive)
                    VALUES
                    (@DealerID,@Type,@Value,1)";

                    SqlCommand comCmd = new SqlCommand(commQuery, con, tran);

                    comCmd.Parameters.AddWithValue("@DealerID", model.Dealer.DealerId);
                    comCmd.Parameters.AddWithValue("@Type", com.Type);
                    comCmd.Parameters.AddWithValue("@Value", com.Value);

                    comCmd.ExecuteNonQuery();
                }

                tran.Commit();

            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        public void DeleteDealer(int dealerId)
        {
            using SqlConnection con = new SqlConnection(_connectionString);

            con.Open();

            try
            {
                string query = @"
                UPDATE T_Dealer
                SET IsActive = 0,
                    UpdatedDate = GETDATE()
                WHERE DealerId = @DealerID";

                SqlCommand cmd = new SqlCommand(query, con);

                cmd.Parameters.AddWithValue("@DealerID", dealerId);

                cmd.ExecuteNonQuery();
            }
            catch
            {
                throw;
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
                    // 👉 adjust as per your model
                    CommunicationID = Convert.ToInt32(rdr["PersonID"]),
                    Type = rdr["PersonType"]?.ToString(),
                    Value = rdr["PersonName"]?.ToString()
                });
            }

            return list;
        }
    }
}