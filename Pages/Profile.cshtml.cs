using Bestshop.MyHelpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;

namespace Bestshop.Pages
{
    [RequireAuth]
    public class ProfileModel : PageModel
    {
        [Required(ErrorMessage = "Vui lòng điền tên của bạn")]
        public string Firstname { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng điền họ và tên lót")]
        public string Lastname { get; set; } = "";
        [Required(ErrorMessage = "Vui lòng điền Email"), EmailAddress]
        public string Email { get; set; } = "";
        public string? Phone { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng điền địa chỉ")]
        public string Address { get; set; } = "";

        public string? Password { get; set; } = "";
        public string? ConfirmPassword { get; set; } = "";

        public string errorMessage = "";
        public string successMessage = "";

        public void OnGet()
        {
            Firstname = HttpContext.Session.GetString("firstname") ?? "";
            Lastname = HttpContext.Session.GetString("lastname") ?? "";
            Email = HttpContext.Session.GetString("email") ?? "";
            Phone = HttpContext.Session.GetString("phone");
            Address = HttpContext.Session.GetString("address") ?? "";
        }

        public void OnPost()
        {
            if (!ModelState.IsValid)
            {
                errorMessage = "Data validation failed";
                return;
            }

            // succuessfull data valiation
            if (Phone == null) Phone = "";

            // update the user profile or the passwords
            string submitButton = Request.Form["action"];

			string connectionString = "Data Source=.\\sqlexpress;Initial Catalog=bestshop;Integrated Security=True";


            if (submitButton.Equals("profile"))
            {
                //update the user profile in the database
                try
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        string sql = "UPDATE users SET firstname=@firstname, lastname=@lastname, " +
                            "email=@email, phone=@phone, address=@address WHERE id=@id";

                        int? id = HttpContext.Session.GetInt32("id");
                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@firstname", Firstname);
                            command.Parameters.AddWithValue("@lastname", Lastname);
                            command.Parameters.AddWithValue("@email", Email);
                            command.Parameters.AddWithValue("@phone", Phone);
                            command.Parameters.AddWithValue("@address", Address);
                            command.Parameters.AddWithValue("@id", id);

                            command.ExecuteNonQuery();
                        }
                    }    
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                    return;
                }

                // update the session data
                HttpContext.Session.SetString("firstname", Firstname);
                HttpContext.Session.SetString("lastname", Lastname);
                HttpContext.Session.SetString("email", Email);
                HttpContext.Session.SetString("phone", Phone);
                HttpContext.Session.SetString("address", Address);

                successMessage = "Thông tin của bạn được cập nhật thành công";

            }
            else if (submitButton.Equals("password"))
            {
                //validate password and confirmPassword 
                if (Password == null || Password.Length < 5 || Password.Length > 50)
                {
                    errorMessage = "Độ dài mật khẩu phải từ 5 đến 50 ký tự";
                    return;
                }

                if (ConfirmPassword == null || !ConfirmPassword.Equals(Password))
                {
                    errorMessage = "Mật khẩu và Xác nhận mật khẩu không khớp";
                    return;
                }

                //update the password in the database

                try
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        string sql = "UPDATE users SET password=@password WHERE id=@id";

                        int? id = HttpContext.Session.GetInt32("id");

                        var passwordHasher = new PasswordHasher<IdentityUser>();
                        string hashedPassword = passwordHasher.HashPassword(new IdentityUser(), Password);

                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@password", hashedPassword);
                            command.Parameters.AddWithValue("@id", id);

                            command.ExecuteNonQuery();
                        }
                    }    
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                    return;
                }

                successMessage = "Mật khẩu được cập nhật thành công";
            }  
        }
    }
}
