using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using MauiAppAula0705;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace MauiAppAula0705
{
    public partial class MainPage : ContentPage
    {
        private readonly DatabaseService _dbService;
        private string _caminhoFotoAtual;

        public ObservableCollection<Postagem> Postagens { get; } = new();

        public MainPage()
        {
            InitializeComponent();
            _dbService = new DatabaseService();
            BindingContext = this;
            CarregarPostagens();
        }

        private async void CarregarPostagens()
        {
            try
            {
                var postagens = await _dbService.ListarPostagens();
                Postagens.Clear();

                foreach (var post in postagens.OrderByDescending(p => p.DataPostagem))
                {
                    Postagens.Add(post);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Falha ao carregar postagens: {ex.Message}", "OK");
            }
        }

        private async void btnTirarFoto_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (!MediaPicker.Default.IsCaptureSupported)
                {
                    await DisplayAlert("Erro", "Captura de foto não suportada", "OK");
                    return;
                }

                var photo = await MediaPicker.Default.CapturePhotoAsync();
                if (photo != null)
                {
                    await GravarFoto(photo);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Falha ao capturar foto: {ex.Message}", "OK");
            }
        }

        private async Task GravarFoto(FileResult photo)
        {
            try
            {
                // Salva em AppDataDirectory (persistente) em vez de CacheDirectory (temporário)
                var nomeArquivo = $"{Guid.NewGuid()}.jpg";
                _caminhoFotoAtual = Path.Combine(FileSystem.AppDataDirectory, nomeArquivo);

                using (var sourceStream = await photo.OpenReadAsync())
                using (var localFileStream = File.OpenWrite(_caminhoFotoAtual))
                {
                    await sourceStream.CopyToAsync(localFileStream);
                }

                imageFoto.Source = ImageSource.FromFile(_caminhoFotoAtual);
            }
            catch (Exception ex)
            {
                _caminhoFotoAtual = null;
                await DisplayAlert("Erro", $"Falha ao salvar foto: {ex.Message}", "OK");
            }
        }

        private async void btnSelecionarFoto_Clicked_1(object sender, EventArgs e)
        {
            try
            {
                var photo = await MediaPicker.Default.PickPhotoAsync();
                if (photo != null)
                {
                    await GravarFoto(photo);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Falha ao selecionar foto: {ex.Message}", "OK");
            }
        }

        private void BtnDelete_OnClicked(object sender, EventArgs e)
        {
            _caminhoFotoAtual = null;
            imageFoto.Source = null;
        }

        private async void OnSalvarPostagem(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(entryComentario.Text))
                {
                    await DisplayAlert("Aviso", "Por favor, adicione um comentário.", "OK");
                    return;
                }

                if (string.IsNullOrEmpty(_caminhoFotoAtual) || !File.Exists(_caminhoFotoAtual))
                {
                    await DisplayAlert("Aviso", "Nenhuma foto válida selecionada.", "OK");
                    return;
                }

                var postagem = new Postagem
                {
                    Comentario = entryComentario.Text,
                    FotoCaminho = _caminhoFotoAtual,
                    DataPostagem = DateTime.Now
                };

                await _dbService.InserirPostagem(postagem);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Postagens.Insert(0, postagem);
                    entryComentario.Text = string.Empty;
                    imageFoto.Source = null;
                    _caminhoFotoAtual = null;
                });

                await DisplayAlert("Sucesso", "Postagem salva com sucesso!", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Falha ao salvar postagem: {ex.Message}", "OK");
            }
        }

        private async void ExcluirPostagem(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is int postId)
            {
                try
                {
                    // Remove do banco de dados
                    await _dbService.ExcluirPostagem(postId);

                    // Remove da lista (atualiza automaticamente a UI)
                    var postagem = Postagens.FirstOrDefault(p => p.Id == postId);
                    if (postagem != null)
                    {
                        Postagens.Remove(postagem);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao excluir: {ex.Message}");
                }
            }
        }


        private async void BtnDestroy_OnClicked(object? sender, EventArgs e)
        {
                if (sender is Button button && button.CommandParameter is int postId)
                {
                    // Remove diretamente sem confirmação
                    await _dbService.ExcluirPostagem(postId);
        
                    // Atualiza a lista local
                    var postagem = Postagens.FirstOrDefault(p => p.Id == postId);
                    if (postagem != null) 
                    {
                        Postagens.Remove(postagem);
                    }
                }
            }
        }
    }
