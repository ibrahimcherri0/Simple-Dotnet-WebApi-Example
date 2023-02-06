using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MySql.Data.MySqlClient;

namespace ServeurApiREST.Controllers;

[ApiController]
[Route("[controller]")]
public class Authentification : Controller
{
    private const string ConnectionString = "Server=127.0.0.1;Database=db;Uid=root;Pwd=;port=3306;";
    private MySqlCommand _command;
    private MySqlDataReader reader;

    private readonly IConfiguration _config;

    public Authentification(IConfiguration config)
    {
        _config = config;
    }

    [HttpPost("/login")]
    public IActionResult Login([FromBody] Utilisateur content)
    {
        var username = content.Username;
        var password = content.Password;


        using (MySqlConnection connection = new MySqlConnection(ConnectionString))
        {
            _command = connection.CreateCommand();
            connection.Open();
            _command.CommandText =
                "SELECT COUNT(*) FROM utilisateur where username =  @username AND password = @password";
            _command.Parameters.AddWithValue("@username", username);
            _command.Parameters.AddWithValue("@password", password);
            int count = Convert.ToInt32(_command.ExecuteScalar());
            if (count == 1)
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, username),
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Super_secret_signin_key_#2345"));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    "WebApi",
                    "WebApi",
                    claims,
                    expires: DateTime.Now.AddMinutes(60),
                    signingCredentials: credentials
                );

                return new OkObjectResult(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
            }

            connection.Close();
        }

        return Unauthorized();
    }

    [HttpPost("/signup")]
    public IActionResult Signup([FromBody] Utilisateur content)
    {
        var username = content.Username;
        var password = content.Password;

        using (MySqlConnection connection = new MySqlConnection(ConnectionString))
        {
            _command = connection.CreateCommand();
            connection.Open();
            _command.CommandText = "SELECT COUNT(*) FROM utilisateur where username =  @username ";
            _command.Parameters.AddWithValue("@username", username);
            _command.Parameters.AddWithValue("@password", password);
            int count = Convert.ToInt32(_command.ExecuteScalar());

            if (count == 0)
            {
                var insertCommand = $"INSERT INTO utilisateur (username, password) VALUES ('{username}', '{password}')";
                using (var command = new MySqlCommand(insertCommand, connection))
                {
                    var rowsAffected = command.ExecuteNonQuery();
                    return Ok(new { message = "done" });
                }
            }
            else if (count == 1)
            {
                return StatusCode(400, new { message = "utilisateur existe deja" });
            }

            connection.Close();
        }

        return StatusCode(400, new { message = "Erreur" });
    }
}