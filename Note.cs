namespace ServeurApiREST;

public class Note
{
    public string? Date { get; set; } = null;

    public int Id { get; set; }

    public int IdUtilisateur { get; set; }

    public string Texte { get; set; }
}