using CareConnect.Mobile.Services;
using CareConnect.Shared.DTOs;
using CareConnect.Shared.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace CareConnect.Mobile.ViewModels.Gestor;

public partial class AdicionarUtenteViewModel : ObservableObject
{
    private readonly INotificationService _notificationService;
    private readonly PatientService _patientService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotLoading))]
    private bool _isLoading;
    public bool IsNotLoading => !IsLoading;

    // --- CAMPOS DO FORMULÁRIO (Mapeados para o Patient) ---
    [ObservableProperty] private string _nomeCompleto = string.Empty;
    [ObservableProperty] private int _idade = 65; 
    [ObservableProperty] private ImageSource _fotoCaminho = ImageSource.FromFile("avatar_elderly.png"); 
    
    [ObservableProperty] private string _contacto = string.Empty;
    [ObservableProperty] private string _contactoEmergencia = string.Empty;
    [ObservableProperty] private string _alergias = string.Empty;
    [ObservableProperty] private string _notas = string.Empty;

    private FileResult? _fotoFicheiro;

    // --- CAIXAS DE SELEÇÃO ---
    [ObservableProperty]
    private ObservableCollection<CuidadorResumo> _cuidadoresDisponiveis = new();

    [ObservableProperty]
    private CuidadorResumo _cuidadorSelecionado;

    [ObservableProperty] private ObservableCollection<string> _condicoesDisponiveis = new();
    [ObservableProperty] private string _condicaoSelecionada = string.Empty;

    public AdicionarUtenteViewModel(INotificationService notificationService, PatientService patientService)
    {
        _notificationService = notificationService;
        _patientService = patientService;
        
        _ = CarregarDadosReaisAsync();
    }

    private async Task CarregarDadosReaisAsync()
    {
        // Mantém as condições médicas predefinidas
        CondicoesDisponiveis = new ObservableCollection<string> { "Nenhuma", "Diabetes Tipo 2", "Hipertensão", "DPOC", "Alzheimer", "Cardiopatia" };
        CondicaoSelecionada = "Nenhuma";

        try
        {
            // 1. Vai à API buscar a lista real de cuidadores
            var cuidadoresReais = await _patientService.ObterCuidadoresDisponiveisAsync();

            // 2. Limpa a lista atual (caso tenha lixo) e adiciona os reais
            CuidadoresDisponiveis.Clear();
            foreach (var cuidador in cuidadoresReais)
            {
                CuidadoresDisponiveis.Add(cuidador);
            }
        }
        catch (Exception ex)
        {
            await _notificationService.MostrarErroAsync("Não foi possível carregar a lista de cuidadores.");
        }
    }

    [RelayCommand] private void AumentarIdade() => Idade++;
    [RelayCommand] private void DiminuirIdade() { if (Idade > 0) Idade--; }

    [RelayCommand]
    private async Task VoltarAsync() => await Shell.Current.GoToAsync("..");

    [RelayCommand]
    private async Task EscolherFotoAsync()
    {
        try
        {
            string acao = await Application.Current!.Windows[0].Page!.DisplayActionSheet("Foto de Perfil", "Cancelar", null, "Tirar Foto", "Escolher da Galeria");

            if (acao == "Tirar Foto" && MediaPicker.Default.IsCaptureSupported)
            {
                _fotoFicheiro = await MediaPicker.Default.CapturePhotoAsync();
            }
            else if (acao == "Escolher da Galeria")
            {
                _fotoFicheiro = await MediaPicker.Default.PickPhotoAsync();
            }

            if (_fotoFicheiro != null)
            {
                string caminhoLocal = Path.Combine(FileSystem.CacheDirectory, _fotoFicheiro.FileName);
                using Stream streamOrigem = await _fotoFicheiro.OpenReadAsync();
                using FileStream streamDestino = File.OpenWrite(caminhoLocal);
                await streamOrigem.CopyToAsync(streamDestino);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    FotoCaminho = ImageSource.FromFile(caminhoLocal);
                });
                
                _fotoFicheiro = new FileResult(caminhoLocal);
            }
        }
        catch (Exception ex)
        {
            await _notificationService.MostrarErroAsync("Erro com a foto: " + ex.Message);
        }
    }

    [RelayCommand]
    private async Task GuardarUtenteAsync()
    {
        if (string.IsNullOrWhiteSpace(NomeCompleto))
        {
            await _notificationService.MostrarAvisoAsync("Por favor, preencha o Nome Completo do utente.");
            return;
        }

        if (IsLoading) return;

        try
        {
            IsLoading = true;

            var novoPaciente = new Patient
            {
                Nome = NomeCompleto,
                DataNascimento = DateTime.UtcNow.AddYears(-Idade),
                Contacto = Contacto,
                ContactoEmergencia = ContactoEmergencia,
                CondicoesMedicas = CondicaoSelecionada,
                Alergias = Alergias,
                Ativo = true,
                Notas = Notas,
                CuidadoresIds = CuidadorSelecionado != null
                    ? new List<Guid> { CuidadorSelecionado.Id }
                    : new List<Guid>()
            };

            // 2. Grava na Base de Dados
            var pacienteCriado = await _patientService.CreatePatientAsync(novoPaciente);

            if (pacienteCriado != null)
            {
                // 3. Faz o Upload da Foto para a Amazon S3
                if (_fotoFicheiro != null)
                {
                    await _patientService.UploadPatientAvatarAsync(pacienteCriado.Id, _fotoFicheiro.FullPath);
                }

                await _notificationService.MostrarSucessoAsync("Ficha de Utente gravada com sucesso!");
                await Shell.Current.GoToAsync("..");
            }
        }
        catch (Exception ex)
        {
            await _notificationService.MostrarErroAsync($"Falha a gravar: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
}