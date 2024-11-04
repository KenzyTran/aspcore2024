using Bestshop.MyHelpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;

namespace Bestshop.Pages.Auth
{
    [RequireNoAuth]
    [BindProperties]
    public class RegisterModel : PageModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ và tên lót")]
        public string Firstname { get; set; } = "";
        [Required(ErrorMessage = "Vui lòng nhập tên")]
        public string Lastname { get; set; } = "";
        [Required(ErrorMessage = "Vui lòng nhập Email"), EmailAddress]
        public string Email { get; set; } = "";
        public string? Phone { get; set; } = "";
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        public string Address { get; set; } = "";
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [StringLength(50, ErrorMessage ="Mật khẩu phải từ 5 tới 50 kí tự", MinimumLength =5)]
        public string Password { get; set; } = "";
        [Required(ErrorMessage = "Xác nhận lại mật khẩu")]
        [Compare("Password", ErrorMessage = "Mật khẩu và Xác nhận mật khẩu không khớp")]
        public string ConfirmPassword { get; set; } = "";

        public string errorMessage = "";
        public string successMessage = "";

        public void OnGet()
        {

        }

        public void OnPost() 
        {
            if(!ModelState.IsValid)
            {
                errorMessage = "Data validation failed";
                return;
            }

            // successful data validation
            if (Phone == null) Phone = "";

            // add the user details to the database

            string connectionString = "Data Source=.\\sqlexpress;Initial Catalog=bestshop;Integrated Security=True";

            try
            {
                using(SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "INSERT INTO users " +
                    "(firstname, lastname, email, phone, address, password, role) VALUES " +
                    "(@firstname, @lastname, @email, @phone, @address, @password, 'client');";

                    var passwordHasher = new PasswordHasher<IdentityUser>();
                    string hashedPassword = passwordHasher.HashPassword(new IdentityUser(), Password);

                    using(SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@firstname", Firstname);
                        command.Parameters.AddWithValue("@lastname", Lastname);
                        command.Parameters.AddWithValue("@email", Email);
                        command.Parameters.AddWithValue("@phone", Phone);
                        command.Parameters.AddWithValue("@address", Address);
                        command.Parameters.AddWithValue("@password", hashedPassword);

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                if(ex.Message.Contains(Email))
                {
                    errorMessage = "Email đã tồn tại";
                }
                else
                {
                    errorMessage = ex.Message;
                }    

                return;
            }

            // send confirmation email to the user
            string username = Firstname + " " + Lastname;
            string subject = "Đăng kí tài khoản thành công";
            string message = "Dear " + username + ",\n\n" +
                "Tài khoản của bạn đã tạo thành công.\n\n" +
                "Best Regards";
            EmailSender.SendEmail(Email, username, subject, message).Wait();

            // intitialize the authenticated session => add the user details to the session data
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "SELECT * FROM users WHERE email=@email";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@email", Email);

                        using(SqlDataReader reader = command.ExecuteReader())
                        {
                            if(reader.Read())
                            {
                                int id = reader.GetInt32(0);
                                string firstname = reader.GetString(1);
                                string lastname = reader.GetString(2);
                                string email = reader.GetString(3);
                                string phone = reader.GetString(4);
                                string address = reader.GetString(5);
                                //string hashedPassword = reader.GetString(6);
                                string role = reader.GetString(7);
                                string created_at = reader.GetDateTime(8).ToString("MM/dd/yyyy");

                                HttpContext.Session.SetInt32("id", id);
                                HttpContext.Session.SetString("firstname", firstname);
                                HttpContext.Session.SetString("lastname", lastname);
                                HttpContext.Session.SetString("email", email);
                                HttpContext.Session.SetString("phone", phone);
                                HttpContext.Session.SetString("address", address);
                                HttpContext.Session.SetString("role", role);
                                HttpContext.Session.SetString("created_at", created_at);
                            }
                        }
                    }    
                }    
            }
            catch(Exception ex)
            {
                errorMessage = ex.Message;
                return;
            }

            successMessage = "Tạo tài khoản thành công";

            // redirect to the home page
            Response.Redirect("/");
        }
    }
}
