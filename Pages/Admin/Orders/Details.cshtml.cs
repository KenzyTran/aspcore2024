﻿using Bestshop.MyHelpers;
using Bestshop.Pages.Admin.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;

namespace Bestshop.Pages.Admin.Orders
{
    [RequireAuth(RequiredRole = "admin")]
    public class DetailsModel : PageModel
    {
        public OrderInfo orderInfo = new OrderInfo();
        public UserInfo userInfo = new UserInfo();

        public void OnGet(int id)
        {
            if(id < 1)
            {
                Response.Redirect("/Admin/Orders/Index");
                return;
            }
            try
            {
                string connectionString = "Data Source=.\\sqlexpress;Initial Catalog=bestshop;Integrated Security=True";
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = "SELECT * FROM orders WHERE id=@id";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                orderInfo.id = reader.GetInt32(0);
                                orderInfo.clientId = reader.GetInt32(1);
                                orderInfo.orderDate = reader.GetDateTime(2).ToString("MM/dd/yyyy");
                                orderInfo.shippingFee = reader.GetDecimal(3);
                                orderInfo.deliveryAddress = reader.GetString(4);
                                orderInfo.paymentMethod = reader.GetString(5);
                                orderInfo.paymentStatus = reader.GetString(6);
                                orderInfo.orderStatus = reader.GetString(7);

                                orderInfo.items = OrderInfo.GetOrderItems(orderInfo.id);
                            }   
                            else
                            {
                                Response.Redirect("/Admin/Orders/Index");
                                return;
                            }    
                        }    
                    }

                    sql = "SELECT * FROM users WHERE id=@id";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@id", orderInfo.clientId);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                userInfo.id = reader.GetInt32(0);
                                userInfo.firstname = reader.GetString(1);
                                userInfo.lastname = reader.GetString(2);
                                userInfo.email = reader.GetString(3);
                                userInfo.phone = reader.GetString(4);
                                userInfo.address = reader.GetString(5);
                                userInfo.password = reader.GetString(6);
                                userInfo.role = reader.GetString(7);
                                userInfo.createdAt = reader.GetDateTime(8).ToString("MM/dd/yyyy");
                            }
                            else
                            {
                                Console.WriteLine("Không tìm thấy client, id=" + orderInfo.clientId);
                                Response.Redirect("/Admin/Orders/Index");
                                return;
                            }
                        }    
                    }    
                }    
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Response.Redirect("/Admin/Orders/Index");
            }
        }
    }
}
