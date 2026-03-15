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

                        dep.Department AS DepartmentName,
                        cat.Category AS CategoryName,
                        pm.PaymentMode,
                        w.Day AS WeeklyOff

                    FROM T_Dealer d

                    LEFT JOIN T_Department dep ON d.DepartmentId = dep.DepID
                    LEFT JOIN T_Category cat ON d.CategoryId = cat.ID
                    LEFT JOIN T_PaymentMode pm ON d.DefaultPaymentModeId = pm.ID
                    LEFT JOIN T_WeeklyOff w ON d.WeeklyOffDayId = w.ID";

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

                            dealer.DepartmentName = rdr["DepartmentName"]?.ToString();
                            dealer.CategoryName = rdr["CategoryName"]?.ToString();
                            dealer.PaymentMode = rdr["PaymentMode"]?.ToString();
                            dealer.WeeklyOff = rdr["WeeklyOff"]?.ToString();

                            dealer.IsActive = Convert.ToBoolean(rdr["IsActive"]);

                            list.Add(dealer);
                        }
                    }
                }
            }

            return list;
        }
        public void InsertDealerFull(DealerViewModel model)
        {
            using SqlConnection con = new SqlConnection(_connectionString);

            con.Open();

            SqlTransaction tran = con.BeginTransaction();

            try
            {

                string dealerQuery = @"INSERT INTO T_Dealer
                (DealerCode,DealerName,OwnerName,GSTNo,PANNo,
                DepartmentId,WeeklyOffDayId,CategoryId,DefaultPaymentModeId,
                Notes,IsActive,CreatedDate,CreatedBy)

                VALUES
                (@DealerCode,@DealerName,@OwnerName,@GSTNo,@PANNo,
                @DepartmentId,@WeeklyOffDayId,@CategoryId,@DefaultPaymentModeId,
                @Notes,@IsActive,GETDATE(),1);

                SELECT SCOPE_IDENTITY();";

                SqlCommand cmd = new SqlCommand(dealerQuery, con, tran);

                cmd.Parameters.AddWithValue("@DealerCode", model.Dealer.DealerCode);
                cmd.Parameters.AddWithValue("@DealerName", model.Dealer.DealerName);
                cmd.Parameters.AddWithValue("@OwnerName", model.Dealer.OwnerName);
                cmd.Parameters.AddWithValue("@GSTNo", model.Dealer.GSTNo);
                cmd.Parameters.AddWithValue("@PANNo", model.Dealer.PANNo);
                cmd.Parameters.AddWithValue("@DepartmentId", model.Dealer.DepartmentId);
                cmd.Parameters.AddWithValue("@WeeklyOffDayId", model.Dealer.WeeklyOffDayId);
                cmd.Parameters.AddWithValue("@CategoryId", model.Dealer.CategoryId);
                cmd.Parameters.AddWithValue("@DefaultPaymentModeId", model.Dealer.DefaultPaymentModeId);
                cmd.Parameters.AddWithValue("@Notes", model.Dealer.Notes ?? "");
                cmd.Parameters.AddWithValue("@IsActive", model.Dealer.IsActive);

                int dealerId = Convert.ToInt32(cmd.ExecuteScalar());

                foreach (var addr in model.Addresses)
                {
                    string addrQuery = @"INSERT INTO T_Address
            (DealerID,AddressType,AddressLine,CityID,Pincode)
            VALUES
            (@DealerID,@AddressType,@AddressLine,@CityID,@Pincode)";

                    SqlCommand addrCmd = new SqlCommand(addrQuery, con, tran);

                    addrCmd.Parameters.AddWithValue("@DealerID", dealerId);
                    addrCmd.Parameters.AddWithValue("@AddressType", addr.AddressType);
                    addrCmd.Parameters.AddWithValue("@AddressLine", addr.AddressLine);
                    addrCmd.Parameters.AddWithValue("@CityID", addr.CityID);
                    addrCmd.Parameters.AddWithValue("@Pincode", addr.Pincode);

                    addrCmd.ExecuteNonQuery();
                }

                foreach (var com in model.Communications)
                {
                    string commQuery = @"INSERT INTO T_CommunicationDetails
            (DealerID,Type,Value,IsActive)
            VALUES
            (@DealerID,@Type,@Value,1)";

                    SqlCommand comCmd = new SqlCommand(commQuery, con, tran);

                    comCmd.Parameters.AddWithValue("@DealerID", dealerId);
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

            SqlTransaction tran = con.BeginTransaction();

            try
            {
                // Delete Communication
                SqlCommand commCmd = new SqlCommand(
                "DELETE FROM T_CommunicationDetails WHERE DealerID=@DealerID",
                con, tran);

                commCmd.Parameters.AddWithValue("@DealerID", dealerId);

                commCmd.ExecuteNonQuery();


                // Delete Address
                SqlCommand addrCmd = new SqlCommand(
                "DELETE FROM T_Address WHERE DealerID=@DealerID",
                con, tran);

                addrCmd.Parameters.AddWithValue("@DealerID", dealerId);

                addrCmd.ExecuteNonQuery();


                // Delete Dealer
                SqlCommand dealerCmd = new SqlCommand(
                "DELETE FROM T_Dealer WHERE DealerID=@DealerID",
                con, tran);

                dealerCmd.Parameters.AddWithValue("@DealerID", dealerId);

                dealerCmd.ExecuteNonQuery();

                tran.Commit();
            }
            catch
            {
                tran.Rollback();
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

            string query = @"SELECT 
                     a.ID,
                     a.AddressType,
                     a.AddressLine,
                     a.Pincode,
                     c.City
                     FROM T_Address a
                     INNER JOIN T_City c ON a.CityID = c.CityID
                     WHERE a.DealerID=@DealerID";

            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@DealerID", dealerId);

            con.Open();

            SqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                list.Add(new Address
                {
                    ID = Convert.ToInt32(rdr["ID"]),
                    AddressType = rdr["AddressType"].ToString(),
                    AddressLine = rdr["AddressLine"].ToString(),
                    Pincode = rdr["Pincode"].ToString(),
                    CityName = rdr["City"].ToString()
                });
            }

            return list;
        }

        public List<CommunicationDetails> GetDealerCommunication(int dealerId)
        {
            List<CommunicationDetails> list = new List<CommunicationDetails>();

            using SqlConnection con = new SqlConnection(_connectionString);

            string query = @"SELECT 
                     CommunicationID,
                     Type,
                     Value
                     FROM T_CommunicationDetails
                     WHERE DealerID=@DealerID
                     AND IsActive=1";

            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@DealerID", dealerId);

            con.Open();

            SqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                list.Add(new CommunicationDetails
                {
                    CommunicationID = Convert.ToInt32(rdr["CommunicationID"]),
                    Type = rdr["Type"].ToString(),
                    Value = rdr["Value"].ToString()
                });
            }

            return list;
        }
    }
}