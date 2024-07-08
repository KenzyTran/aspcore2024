using Bestshop.MyHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;

namespace Bestshop.Pages.Admin.Books
{
	[RequireAuth(RequiredRole = "admin")]
	public class EditModel : PageModel
    {
        [BindProperty]
        public int Id { get; set; }

		[BindProperty]
		[Required(ErrorMessage = "The Title is required")]
		[MaxLength(100, ErrorMessage = "The Title cannot exceed 100 characters")]
		public string Title { get; set; } = "";

		[BindProperty]
		[Required(ErrorMessage = "Authors is required")]
		[MaxLength(255, ErrorMessage = "The Authors cannot exceed 255 characters")]
		public string Authors { get; set; } = "";

		[BindProperty]
		[Required(ErrorMessage = "The ISBN is required")]
		[MaxLength(20, ErrorMessage = "The ISBN cannot exceed 100 characters")]
		public string ISBN { get; set; } = "";

		[BindProperty]
		[Required(ErrorMessage = "The Number of Pages is required")]
		[Range(1, 10000, ErrorMessage = "The Number of Pages must be in the range from 1 to 10000")]
		public int NumPages { get; set; }

		[BindProperty]
		[Required(ErrorMessage = "The Price is required")]
		public decimal Price { get; set; }

		[BindProperty]
		public string Category { get; set; } = "";

		[BindProperty]
		[MaxLength(1000, ErrorMessage = "The Description cannot exceed 1000 characters")]
		public string? Description { get; set; } = "";

		[BindProperty]
		public string ImageFileName { get; set; } = "";

		[BindProperty]
		public IFormFile? ImageFile { get; set; }

		public string errorMessage = "";
		public string successMessage = "";

		private IWebHostEnvironment webHostEnvironment;

		public EditModel (IWebHostEnvironment env)
		{
			webHostEnvironment = env;
		}

		public void OnGet()
        {
			string requestId = Request.Query["id"];

			try
			{
				string connectionString = "Data Source=.\\sqlexpress;Initial Catalog=bestshop;Integrated Security=True";

				using(SqlConnection connection = new SqlConnection(connectionString))
				{
					connection.Open();

					string sql = "SELECT * FROM books WHERE id=@id";

					using (SqlCommand command = new SqlCommand(sql, connection))
					{
						command.Parameters.AddWithValue("@id", requestId);
						using(SqlDataReader reader = command.ExecuteReader()) 
						{
							if(reader.Read())
							{
								Id = reader.GetInt32(0);
								Title = reader.GetString(1);
								Authors = reader.GetString(2);
								ISBN = reader.GetString(3);
								NumPages = reader.GetInt32(4);
								Price = reader.GetDecimal(5);
								Category = reader.GetString(6);
								Description = reader.GetString(7);
								ImageFileName = reader.GetString(8);
							}
							else
							{
								Response.Redirect("/Admin/Books/Index");
							}	
						}
					}
				}
			}
			catch (Exception ex)
			{
                Console.WriteLine(ex.Message);
				Response.Redirect("/Admin/Books/Index");
            }
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

			//if we have a new ImageFile => upload the new image and delete the old image
			string newFileName = ImageFileName;
			if(ImageFile != null)
			{
				newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
				newFileName += Path.GetExtension(ImageFile.FileName);

				string imageFolder = webHostEnvironment.WebRootPath + "/images/books";
				string imageFullPath = Path.Combine(imageFolder, newFileName);

				using (var stream = System.IO.File.Create(imageFullPath))
				{
					ImageFile.CopyTo(stream);
				}

				//delete old image
				string oldImageFullPath = Path.Combine(imageFolder, ImageFileName);
				System.IO.File.Delete(oldImageFullPath);
                Console.WriteLine("Delete Image " + oldImageFullPath);
            }

			//update the book data in the database

			try
			{
                string connectionString = "Data Source=.\\sqlexpress;Initial Catalog=bestshop;Integrated Security=True";
                using (SqlConnection connection = new SqlConnection(connectionString))
				{
					connection.Open();
					string sql = "UPDATE books SET title=@title, authors=@authors, isbn=@isbn, " + 
						"num_pages=@num_pages, price=@price, category=@category, " +
						"description=@description, image_filename=@image_filename WHERE id=@id;";

                    using (SqlCommand command = new SqlCommand(sql, connection))
					{
                        command.Parameters.AddWithValue("@title", Title);
                        command.Parameters.AddWithValue("@authors", Authors);
                        command.Parameters.AddWithValue("@isbn", ISBN);
                        command.Parameters.AddWithValue("@num_pages", NumPages);
                        command.Parameters.AddWithValue("@price", Price);
                        command.Parameters.AddWithValue("@category", Category);
                        command.Parameters.AddWithValue("@description", Description);
                        command.Parameters.AddWithValue("@image_filename", newFileName);
						command.Parameters.AddWithValue("@id", Id);

						command.ExecuteNonQuery();
                    }	

                }	

            }
			catch(Exception ex)
			{
				errorMessage = ex.Message;
				return;
			}

			successMessage = "Data saved correctly";
			Response.Redirect("/Admin/Books/Index");
		}
    }
}
