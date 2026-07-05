using CareConnect.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace CareConnect.Mobile.ViewModels.Gestor;

public partial class AdicionarUtenteViewModel : ObservableObject
{
    private readonly INotificationService _notificationService;

    [ObservableProperty]
    private string _nomeCompleto = string.Empty;

    [ObservableProperty]
    private int _idade = 65; // Idade inicial padrão

    [ObservableProperty]
    private string _fotoCaminho = "avatar_placeholder.png"; // Imagem padrão

    // Ficheiro real que será enviado para a API mais tarde
    private FileResult? _fotoFicheiro;

    // Listas para os Pickers (Dropdowns)
    [ObservableProperty]
    private ObservableCollection<string> _cuidadoresDisponiveis = new();

    [ObservableProperty]
    private string _cuidadorSelecionado = string.Empty;

    [ObservableProperty]
    private ObservableCollection<string> _condicoesDisponiveis = new();

    [ObservableProperty]
    private string _condicaoSelecionada = string.Empty;

    public AdicionarUtenteViewModel(INotificationService notificationService)
    {
        _notificationService = notificationService;
        CarregarListasMock();
    }

    private void CarregarListasMock()
    {
        CuidadoresDisponiveis = new ObservableCollection<string> { "Ana Silva", "Sarah Miller", "Michael Brown" };
        CondicoesDisponiveis = new ObservableCollection<string> { "Diabetes Tipo 2", "Hipertensão", "DPOC", "Alzheimer", "Nenhuma" };
    }

    // --- COMANDOS PARA A IDADE ---
    [RelayCommand]
    private void AumentarIdade() => Idade++;

    [RelayCommand]
    private void DiminuirIdade()
    {
        if (Idade > 0) Idade--;
    }

    // --- COMANDO PARA FOTO ---
    [RelayCommand]
    private async Task EscolherFotoAsync()
    {
        try
        {
            // Pede ao MAUI para abrir a galeria do telemóvel
            var foto = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Por favor, selecione uma foto"
            });

            if (foto != null)
            {
                _fotoFicheiro = foto;
                FotoCaminho = foto.FullPath; // Atualiza a UI para mostrar a foto escolhida
            }
        }
        catch (Exception ex)
        {
            await _notificationService.MostrarErroAsync("Erro ao abrir a galeria: " + ex.Message);
        }
    }

    // --- COMANDOS DE NAVEGAÇÃO E GUARDAR ---
    [RelayCommand]
    private async Task VoltarAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task GuardarUtenteAsync()
    {
        if (string.IsNullOrWhiteSpace(NomeCompleto) || string.IsNullOrWhiteSpace(CuidadorSelecionado))
        {
            await _notificationService.MostrarAvisoAsync("Preencha o nome e atribua um cuidador.");
            return;
        }

        // TODO: Enviar dados e ficheiro (_fotoFicheiro) para a API no futuro.
        await _notificationService.MostrarSucessoAsync("Utente guardado com sucesso!");
        await Shell.Current.GoToAsync("..");
    }
}