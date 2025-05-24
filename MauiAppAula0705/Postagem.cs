using SQLite;

namespace MauiAppAula0705;

public class Postagem
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public DateTime DataPostagem { get; set; }
    public string Comentario { get; set; }
    public string FotoCaminho { get; set; }  // Armazena o caminho da foto no dispositivo
}