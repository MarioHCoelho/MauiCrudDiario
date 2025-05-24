using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MauiAppAula0705;

public partial class MainPage : ContentPage
{
    private DatabaseService _dbService;
    private string _caminhoFotoAtual;

    public ObservableCollection<Postagem> Postagens { get; private set; } = new();
	public MainPage()
	{
		InitializeComponent();
        _dbService = new DatabaseService();
        BindingContext = this;
        CarregarPostagens();

    }

    private async void CarregarPostagens()
    {
        var postagens = await _dbService.ListarPostagens();
        Postagens.Clear();
        foreach (var post in postagens)
            postagens.Add(post);
        {
            
        }
    }
    private async void btnTirarFoto_Clicked(object sender, EventArgs e)
    {
        if (MediaPicker.Default.IsCaptureSupported)
        {
            FileResult photo = await MediaPicker.Default.CapturePhotoAsync();
            GravarFoto(photo);
        }
        else
        {
            await DisplayAlert("Erro", "Captura de foto n�o suportada", "OK");
        }
    }
    private async void GravarFoto(FileResult photo)
    {
        if (photo != null)
        {
            string arquivoLocal = Path.Combine(FileSystem.CacheDirectory, photo.FileName);
            using Stream sourceStream = await photo.OpenReadAsync();
            using FileStream localFileStream = File.OpenWrite(arquivoLocal);
            await sourceStream.CopyToAsync(localFileStream);
            imageFoto.Source=arquivoLocal;
        }
        else
        {
            imageFoto.Source = null;
        }
    }

    private async Task btnSelecionarFoto_Clicked(object sender, EventArgs e)
    {
        FileResult? photo = await MediaPicker.Default.PickPhotoAsync();
        GravarFoto(photo);
    }

    private async void btnSelecionarFoto_Clicked_1(object sender, EventArgs e)
    {
        FileResult? photo = await MediaPicker.Default.PickPhotoAsync();
        GravarFoto(photo);
    }

    private void BtnDelete_OnClicked(object? sender, EventArgs e)
    {
        _caminhoFotoAtual = null;
        imageFoto.Source = null;
    }


    private async void OnSalvarPostagem(object? sender, EventArgs e)
    {
        {
            if (string.IsNullOrWhiteSpace(entryComentario.Text) || _caminhoFotoAtual == null)
            {
                await DisplayAlert("Aviso", "Preencha o comentário e selecione uma foto.", "OK");
                return;
            }

            var postagem = new Postagem
            {
                Comentario = entryComentario.Text,
                FotoCaminho = _caminhoFotoAtual,
                DataPostagem = DateTime.Now
            };

            await _dbService.InserirPostagem(postagem);
            Postagens.Add(postagem); // Atualiza a lista automaticamente
        
            // Limpa os campos
            entryComentario.Text = string.Empty;
            imageFoto.Source = null;
            _caminhoFotoAtual = null;
        }
    }
}