using System.Text.Json.Serialization;

namespace ServeurApiREST;

public class Utilisateur
{
        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("password")]
        public string Password { get; set; }
}