using Bestshop.MyHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;

namespace Bestshop.Pages.Admin.Books
{
	[RequireAuth(RequiredRole = "admin")]
    public class CreateModel : PageModel
    {
        [BindProperty]
		[Required(ErrorMessage = "Vui lòng điền tên sách")]
		[MaxLength(100, ErrorMessage = "Tiêu đề sách không thể vượt quá 100 ký tự")]
        public string Title { get; set; } = "";

		[BindProperty]
		[Required(ErrorMessage = "Vui lòng điền tên tác giả")]
		[MaxLength(255, ErrorMessage = "Tên tác giả không thể vượt quá 255 ký tự")]
		public string Authors { get; set; } = "";

		[BindProperty]
		[Required(ErrorMessage = "Vui lòng điền ISBN")]
		[MaxLength(20, ErrorMessage = "ISBN không được vượt quá 100 ký tự")]
		public string ISBN { get; set; } = "";

		[BindProperty]
		[Required(ErrorMessage = "Vui lòng điền số trang của sách")]
		[Range(1, 10000, ErrorMessage = "Số trang phải nằm trong khoảng từ 1 đến 10000")]
		public int NumPages { get; set; }

		[BindProperty]
		[Required(ErrorMessage = "Vui lòng điền giá sách")]
		public decimal Price { get; set; }

		[BindProperty]
		public string Category { get; set; } = "";

		[BindProperty]
		[MaxLength(1000, ErrorMessage = "Mô tả không thể vượt quá 1000 ký tự")]
		public string? Description { get; set; } = "";

		[BindProperty]
		[Required(ErrorMessage = "File ảnh là bắt buộc")]
		public IFormFile ImageFile { get; set; }

		public string errorMessage = "";
		public string successMessage = "";

		private IWebHostEnvironment webHostEnvironment;

		public CreateModel(IWebHostEnvironment env)
		{
			webHostEnvironment = env;
		}
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

			//successfull data validation

			if (Description == null) Description = "";

			//lưu file ảnh lên server
			string newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
			newFileName += Path.GetExtension(ImageFile.FileName);

			string imageFolder = webHostEnvironment.WebRootPath + "/images/books";

			string imageFullPath = Path.Combine(imageFolder, newFileName);

			using(var stream = System.IO.File.Create(imageFullPath))
			{
				ImageFile.CopyTo(stream);
			}
			// Lưu sách mới vào database
			try {
				string connectionString = "Data Source=.\\sqlexpress;Initial Catalog=bestshop;Integrated Security=True";
				
				using(SqlConnection connection = new SqlConnection(connectionString))
				{
					connection.Open();
					string sql = "INSERT INTO books" +
					"(title, authors, isbn, num_pages, price, category, description, image_filename) VALUES" +
					"(@title, @authors, @isbn, @num_pages, @price, @category, @description, @image_filename);";

					using(SqlCommand command = new SqlCommand(sql, connection))
					{
						command.Parameters.AddWithValue("@title", Title);
						command.Parameters.AddWithValue("@authors", Authors);
						command.Parameters.AddWithValue("@isbn", ISBN);
						command.Parameters.AddWithValue("@num_pages", NumPages);
						command.Parameters.AddWithValue("@price", Price);
						command.Parameters.AddWithValue("@category", Category);
						command.Parameters.AddWithValue("@description", Description);
						command.Parameters.AddWithValue("@image_filename", newFileName);

						command.ExecuteNonQuery();
					}
				}
			}
			catch (Exception ex)
			{
				errorMessage = ex.Message;
				return;
			}
			successMessage = "Data saved correctly";
			Response.Redirect("/Admin/Books/Index");
		}
    }
}
