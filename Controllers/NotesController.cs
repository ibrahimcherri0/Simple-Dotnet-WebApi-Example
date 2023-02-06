using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace ServeurApiREST.Controllers;

[ApiController]
[Authorize]
[Route("[controller]")]
public class NotesController : Controller
{
    private const string ConnectionString = "Server=127.0.0.1;Database=db;Uid=root;Pwd=;port=3306;";
    private MySqlCommand _command;
    private MySqlDataReader reader;

    [HttpGet("/notes")]
    public IActionResult GET()
    {
        var username = GetUsernameFromToken();
        var idUtilisateur = GetUserId(username);
        List<Note> notes = new List<Note>();
        using (MySqlConnection connection = new MySqlConnection(ConnectionString))
        {
            _command = connection.CreateCommand();
            connection.Open();
            _command.CommandText = "SELECT * FROM note where id_utilisateur =  @id_utilisateur";
            _command.Parameters.AddWithValue("@id_utilisateur", idUtilisateur);
            reader = _command.ExecuteReader();

            while (reader.Read())
            {
                Note note = new Note();
                note.Id = int.Parse(reader["id"].ToString());
                note.IdUtilisateur = int.Parse(reader["id_utilisateur"].ToString());
                note.Texte = reader["text"].ToString();
                note.Date = reader["date"].ToString();
                notes.Add(note);
            }

            connection.Close();
        }

        return Ok(notes);
    }

    [HttpPost("/notes")]
    public IActionResult Create([FromBody] Note note)
    {
        var username = GetUsernameFromToken();
        var idUtilisateur = GetUserId(username);
        var text = note.Texte;
        using (MySqlConnection connection = new MySqlConnection(ConnectionString))
        {
            connection.Open();
            var insertCommand = $"INSERT INTO note (id_utilisateur, text) VALUES ('{idUtilisateur}', '{text}')";
            using (var command = new MySqlCommand(insertCommand, connection))
            {
                var rowsAffected = command.ExecuteNonQuery();
                if (rowsAffected == 1)
                    return Ok(new { message = "note ajouté" });
            }

            connection.Close();
        }

        return StatusCode(400, new { message = "Erreur" });
    }

    [HttpPut("/notes")]
    public IActionResult Update([FromBody] Note note)
    {
        var username = GetUsernameFromToken();
        var idUtilisateur = GetUserId(username);

        using (MySqlConnection connection = new MySqlConnection(ConnectionString))
        {
            _command = connection.CreateCommand();
            connection.Open();
            _command.CommandText = "UPDATE note SET text = @text WHERE id_utilisateur = @id_utilisateur AND id = @id";
            _command.Parameters.AddWithValue("@id", note.Id);
            _command.Parameters.AddWithValue("@id_utilisateur", idUtilisateur);
            _command.Parameters.AddWithValue("@text", note.Texte);
            int affectedRows = _command.ExecuteNonQuery();

            if (affectedRows > 0)
            {
                connection.Close();
                return StatusCode(200, new { message = "note Modifié" });
            }

            connection.Close();
        }

        return StatusCode(400, new { message = "Erreur" });
    }

    [HttpDelete("/notes/{idNote}")]
    public IActionResult Create(int idNote)
    {
        var username = GetUsernameFromToken();
        var idUtilisateur = GetUserId(username);

        using (MySqlConnection connection = new MySqlConnection(ConnectionString))
        {
            _command = connection.CreateCommand();
            connection.Open();
            _command.CommandText = "DELETE FROM note where id=  @id AND id_utilisateur = id_utilisateur";
            _command.Parameters.AddWithValue("@id", idNote);
            _command.Parameters.AddWithValue("@id_utilisateur", idUtilisateur);
            int affectedRows = _command.ExecuteNonQuery();

            if (affectedRows > 0)
            {
                connection.Close();
                return StatusCode(200, new { message = "note supprimé" });
            }

            connection.Close();
        }

        return StatusCode(400, new { message = "Erreur" });
    }


    private string GetUsernameFromToken()
    {
        var username = string.Empty;
        var token = string.Empty;
        string authorizationHeader = Request.Headers["Authorization"];
        if (authorizationHeader != null && authorizationHeader.StartsWith("Bearer "))
        {
            token = authorizationHeader.Substring("Bearer ".Length).Trim();
        }

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadToken(token) as JwtSecurityToken;

        foreach (var claim in jwt.Claims)
        {
            if (claim.Type == ClaimTypes.Name)
            {
                username = claim.Value;
                break;
            }
        }

        return username;
    }

    private string GetUserId(string username)
    {
        var idUtilisateur = String.Empty;
        using (MySqlConnection connection = new MySqlConnection(ConnectionString))
        {
            _command = connection.CreateCommand();
            connection.Open();
            _command.CommandText = "SELECT * FROM utilisateur where username =  @username";
            _command.Parameters.AddWithValue("@username", username);
            reader = _command.ExecuteReader();

            while (reader.Read())
            {
                idUtilisateur = reader["id"].ToString();
            }

            connection.Close();
        }

        return idUtilisateur;
    }
}