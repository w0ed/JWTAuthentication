using System.Web.Http;
using System.Data;
using System.Data.SqlClient;
using System;
using ApiAuthentication.Models;
using System.Configuration;
using System.Web.Helpers;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Collections.Generic;
using System.Linq;


namespace ApiAuthentication.Controllers
{
    public class WebApiController : ApiController
    {
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["cs"].ConnectionString);
        SqlCommand cmd;
        SqlDataAdapter sda;
        DataTable dt;

        [Route("gettoken")]
        [HttpPost]
        public IHttpActionResult GetToken(UserModel user)
        {
            if (IsValidUser(user.Username, user.Password))
            {
                var token = GenerateJwtToken(user.Username);
                var response = new
                {
                    Message = "Token Generated",
                    Token = token
                };
                return Ok(response);
            }

            return Ok("Token Not Generated");
        }
        public string GetUsernameFromToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;



                if (jwtToken != null)
                {
                    var usernameClaim = jwtToken.Claims.FirstOrDefault(claim => claim.Type == "sub");
                    if (usernameClaim != null)
                    {
                        return usernameClaim.Value;
                    }
                }
            }
            catch (Exception)
            {
            }
            return null;
        }
        [TokenAuthorize]
        [Route("getproduct")]
        [HttpGet]
        public IHttpActionResult GetProduct()
        {
            var token = Request.Headers.Authorization?.Parameter;
            string username = GetUsernameFromToken(token);

            List<ProductModel> products = new List<ProductModel>();
                con.Open();

                using (SqlCommand cmd = new SqlCommand("GetAllProducts", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Username", username);


                using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ProductModel product = new ProductModel
                            {
                                ProductId = Convert.ToInt32(reader["product_id"]),
                                ProductName = reader["product_name"].ToString(),
                                Description = reader["description"].ToString(),
                                Price = Convert.ToDecimal(reader["price"]),
                                Category = reader["category"].ToString(),
                                StockQuantity = Convert.ToInt32(reader["stock_quantity"]),
                                UserId = Convert.ToInt32(reader["user_id"])
                            };

                            products.Add(product);
                        }
                    }
                
            }

            return Ok(products);
        }
        private string GenerateJwtToken(string username)
        {
            var jwtSettings = ConfigurationManager.AppSettings; // Get settings from your configuration file



            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["JwtSecretKey"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);



            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };



            var token = new JwtSecurityToken(
                issuer: jwtSettings["JwtIssuer"],
                audience: jwtSettings["JwtAudience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToInt32(jwtSettings["JwtExpireMinutes"])),
                signingCredentials: credentials
            );



            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [Route("newUser")]
        [HttpPost]
        public IHttpActionResult newUser(UserModel userModel)
        {
            try
            {

                con.Open();
                using (var command = new SqlCommand("sp_UserRegistration", con))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@username", userModel.Username);
                    command.Parameters.AddWithValue("@email", userModel.Email);
                    command.Parameters.AddWithValue("@Password", userModel.Password);
                    command.Parameters.AddWithValue("@Fname", userModel.Fname);
                    command.Parameters.AddWithValue("@Lname", userModel.Lname);

                    var registrationStatus = command.ExecuteScalar();

                    if (registrationStatus != null && registrationStatus is int)
                    {
                        int status = (int)registrationStatus;
                        if (status == 1)
                        {
                            return Ok("User registered successfully.");
                        }
                        else if (status == -1)
                        {
                            return BadRequest("User with the same email already exists.");
                        }
                    }
                }


                return InternalServerError(new Exception("User registration failed."));
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }       
        private bool IsValidUser(string username, string password)
        {

            using (SqlCommand cmd = new SqlCommand("sp_UserLogin", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@password", password);

                con.Open();
                cmd.ExecuteNonQuery();
                return true;
            }
        }

    }

}
