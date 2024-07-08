using Bestshop.MyHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;

namespace Bestshop.Pages.Admin.Users
{
    [RequireAuth(RequiredRole = "admin")]
    public class IndexModel : PageModel
    {
        public List<UserInfo> listUsers = new List<UserInfo>();

        public int page = 1;
        public int totalPages = 0;
        private readonly int pageSize = 5;

        public void OnGet()
        {
            page = 1;
            string requestPage = Request.Query["page"];
            if (requestPage != null)
            {
                try
                {
                    page = int.Parse(requestPage);
                }
                catch (Exception ex)
                {
                    page = 1;
                }
            }

            try
            {
				string connectionString = "Data Source=.\\sqlexpress;Initial Catalog=bestshop;Integrated Security=True";
				using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // tìm số lượng users
                    string sqlCount = "SELECT COUNT(*) FROM users";
                    using (SqlCommand command = new SqlCommand(sqlCount, connection))
                    {
                        decimal count = (int)command.ExecuteScalar();
                        totalPages = (int)Math.Ceiling(count / pageSize);
                    }

                    string sql = "SELECT * FROM users ORDER BY id DESC";
                    sql += " OFFSET @skip ROWS FETCH NEXT @pageSize ROWS ONLY";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@skip", (page - 1) * pageSize);
                        command.Parameters.AddWithValue("@pageSize", pageSize);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                UserInfo userInfo = new UserInfo();

								userInfo.id = reader.GetInt32(0);
								userInfo.firstname = reader.GetString(1);
								userInfo.lastname = reader.GetString(2);
								userInfo.email = reader.GetString(3);
								userInfo.phone = reader.GetString(4);
								userInfo.address = reader.GetString(5);
								userInfo.password = reader.GetString(6);
								userInfo.role = reader.GetString(7);
								userInfo.createdAt = reader.GetDateTime(8).ToString("MM/dd/yyyy");

								listUsers.Add(userInfo);
							}
                        }    

					}    

				}    

			}
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    public class UserInfo
    {
        public int id;
        public string firstname;
        public string lastname;
        public string email;
        public string phone;
        public string address;
        public string password;
        public string role;
        public string createdAt;
    }
}
