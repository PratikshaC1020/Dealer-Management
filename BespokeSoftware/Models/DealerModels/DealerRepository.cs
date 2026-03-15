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

                string query = @"SELECT 
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

                SqlCommand cmd = new SqlCommand(query, con);

                con.Open();

                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    list.Add(new Dealer
                    {
                        DealerId = Convert.ToInt32(rdr["DealerId"]),
                        DealerCode = rdr["DealerCode"].ToString(),
                        DealerName = rdr["DealerName"].ToString(),
                        OwnerName = rdr["OwnerName"].ToString(),
                        GSTNo = rdr["GSTNo"].ToString(),
                        PANNo = rdr["PANNo"].ToString(),
                        DepartmentName = rdr["DepartmentName"].ToString(),
                        CategoryName = rdr["CategoryName"].ToString(),
                        PaymentMode = rdr["PaymentMode"].ToString(),
                        WeeklyOff = rdr["WeeklyOff"].ToString(),
                        IsActive = Convert.ToBoolean(rdr["IsActive"])
                    });
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

        public void DeleteDealer(int id)
        {
            using SqlConnection con = new SqlConnection(_connectionString);

            string query = "DELETE FROM T_Dealer WHERE DealerId=@DealerId";

            SqlCommand cmd = new SqlCommand(query, con);

            cmd.Parameters.AddWithValue("@DealerId", id);

            con.Open();
            cmd.ExecuteNonQuery();
        }

        public List<Department> GetDepartments()
        {
            List<Department> list = new List<Department>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = "SELECT DepID, Department FROM T_Department WHERE IsDelete=0";

                SqlCommand cmd = new SqlCommand(query, con);

                con.Open();

                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    list.Add(new Department
                    {
                        DepID = Convert.ToInt32(rdr["DepID"]),
                        DepartmentName = rdr["Department"].ToString()
                    });
                }
            }

            return list;
        }

        public List<Category> GetCategories()
        {
            List<Category> list = new List<Category>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = "SELECT ID, Category FROM T_Category WHERE IsDelete=0";

                SqlCommand cmd = new SqlCommand(query, con);

                con.Open();

                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    list.Add(new Category
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
    }
}