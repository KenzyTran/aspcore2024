using Bestshop.MyHelpers;
using Bestshop.Pages.Admin.Orders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;

namespace Bestshop.Pages.Client.Orders
{
    [RequireAuth(RequiredRole = "client")]
    public class IndexModel : PageModel
    {
		public List<OrderInfo> listOrders = new List<OrderInfo>();

		public int page = 1; // trang html hiện tại
		public int totalPages = 0;
		private readonly int pageSize = 3; // đơn đặt hàng trên mỗi trang

		public void OnGet()
		{
			int clientId = HttpContext.Session.GetInt32("id") ?? 0;

			try
			{
				string requestPage = Request.Query["page"];
				page = int.Parse(requestPage);
			}
			catch (Exception ex)
			{
				page = 1;
			}

			try
			{
				string connectionString = "Data Source=.\\sqlexpress;Initial Catalog=bestshop;Integrated Security=True";
				using (SqlConnection connection = new SqlConnection(connectionString))
				{
					connection.Open();

					string sqlCount = "SELECT COUNT(*) FROM orders WHERE client_id=@client_id";
					using (SqlCommand command = new SqlCommand(sqlCount, connection))
					{
						command.Parameters.AddWithValue("@client_id", clientId);

						decimal count = (int)command.ExecuteScalar();
						totalPages = (int)Math.Ceiling(count / pageSize);
					}

					string sql = "SELECT * FROM orders WHERE client_id=@client_id ORDER BY id DESC";
					sql += " OFFSET @skip ROWS FETCH NEXT @pageSize ROWS ONLY";
					using (SqlCommand command = new SqlCommand(sql, connection))
					{
						command.Parameters.AddWithValue("@client_id", clientId);
						command.Parameters.AddWithValue("@skip", (page - 1) * pageSize);
						command.Parameters.AddWithValue("@pageSize", pageSize);

						using (SqlDataReader reader = command.ExecuteReader())
						{
							while (reader.Read())
							{
								OrderInfo orderInfo = new OrderInfo();
								orderInfo.id = reader.GetInt32(0);
								orderInfo.clientId = reader.GetInt32(1);
								orderInfo.orderDate = reader.GetDateTime(2).ToString("MM/dd/yyyy");
								orderInfo.shippingFee = reader.GetDecimal(3);
								orderInfo.deliveryAddress = reader.GetString(4);
								orderInfo.paymentMethod = reader.GetString(5);
								orderInfo.paymentStatus = reader.GetString(6);
								orderInfo.orderStatus = reader.GetString(7);

								orderInfo.items = OrderInfo.GetOrderItems(orderInfo.id);

								listOrders.Add(orderInfo);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}
	}
}
