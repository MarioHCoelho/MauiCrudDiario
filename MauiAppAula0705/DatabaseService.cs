namespace MauiAppAula0705;

using SQLite;
using System.IO;

public class DatabaseService
{
    private SQLiteAsyncConnection _database;

    public DatabaseService()
    {
        string dbPath = Path.Combine(FileSystem.AppDataDirectory, "diario.db3");
        _database = new SQLiteAsyncConnection(dbPath);
        _database.CreateTableAsync<Postagem>().Wait(); // Cria a tabela se não existir
    }

    // Métodos CRUD aqui (serão implementados abaixo)
    public async Task InserirPostagem(Postagem postagem)
    {
        await _database.InsertAsync(postagem);
    }
    public async Task<List<Postagem>> ListarPostagens()
    {
        return await _database.Table<Postagem>().OrderByDescending(p => p.DataPostagem).ToListAsync();
    }
    public async Task ExcluirPostagem(int id)
    {
        var postagem = await _database.Table<Postagem>().Where(p => p.Id == id).FirstOrDefaultAsync();
        if (postagem != null)
        {
            try { File.Delete(postagem.FotoCaminho); } catch { }
            await _database.DeleteAsync(postagem);
            
        }
    }
}