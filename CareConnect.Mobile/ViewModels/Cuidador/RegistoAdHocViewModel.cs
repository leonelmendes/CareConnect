using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CareConnect.Shared.DTOs;
using CareConnect.Mobile.Services;
using System.Collections.ObjectModel;
using Microsoft.Maui.Media;

namespace CareConnect.Mobile.ViewModels.Cuidador;

public partial class RegistoAdHocViewModel : ObservableObject
{
    private readonly TarefaService _tarefaService;
    private readonly PatientService _patientService;

    [ObservableProperty]
    private ObservableCollection<UtenteResumo> utentesDisponiveis = new();

    [ObservableProperty]
    private UtenteResumo utenteSelecionado;

    [ObservableProperty]
    private string categoriaSelecionada = "Sinais Vitais"; // Predefinição

    [ObservableProperty]
    private TimeSpan horaSelecionada = DateTime.Now.TimeOfDay; // Pega a hora atual

    [ObservableProperty]
    private string notas;

    // Propriedades para a Foto
    [ObservableProperty]
    private ImageSource fotoPreview;

    [ObservableProperty]
    private bool temFoto = false;

    private FileResult _fotoSelecionada;

    public RegistoAdHocViewModel(TarefaService tarefaService, PatientService patientService)
    {
        _tarefaService = tarefaService;
        _patientService = patientService;
        _ = CarregarDadosIniciaisAsync();
    }

    // Carrega os utentes reais assim que a página abre
    [RelayCommand]
    public async Task CarregarDadosIniciaisAsync()
    {
        try
        {
            // 1. Recebes a lista de Patient (modelo completo)
            var pacientes = await _patientService.GetMeusPacientesAsync();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (pacientes != null && pacientes.Any())
                {
                    // 2. Transformamos cada 'Patient' num 'UtenteResumo'
                    var utentesConvertidos = pacientes.Select(p => new UtenteResumo
                    {
                        Id = p.Id,
                        Nome = p.Nome // Nota: Se na tua classe Patient a propriedade for "Name" em vez de "Nome", ajusta aqui!
                    }).ToList();

                    // 3. Entregamos a lista convertida ao ecrã
                    UtentesDisponiveis = new ObservableCollection<UtenteResumo>(utentesConvertidos);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("A API não devolveu nenhum utente para este cuidador.");
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao carregar utentes no Ad-Hoc: {ex.Message}");
        }
    }

    // Muda a categoria ao clicar nos botões
    [RelayCommand]
    private void SelecionarCategoria(string categoria)
    {
        CategoriaSelecionada = categoria;
    }

    [RelayCommand]
    private async Task AnexarFotoAsync()
    {
        try
        {
            var action = await Shell.Current.DisplayActionSheet("Adicionar Foto", "Cancelar", null, "Tirar Foto", "Escolher da Galeria");

            if (action == "Tirar Foto")
                _fotoSelecionada = await MediaPicker.Default.CapturePhotoAsync();
            else if (action == "Escolher da Galeria")
                _fotoSelecionada = await MediaPicker.Default.PickPhotoAsync();

            if (_fotoSelecionada != null)
            {
                // Mostra o preview na UI
                var stream = await _fotoSelecionada.OpenReadAsync();
                FotoPreview = ImageSource.FromStream(() => stream);
                TemFoto = true;
            }
        }
        catch (Exception)
        {
            await Shell.Current.DisplayAlert("Erro", "Não foi possível carregar a imagem.", "OK");
        }
    }

    [RelayCommand]
    private async Task SubmeterRegistoAsync()
    {
        if (UtenteSelecionado == null)
        {
            await Shell.Current.DisplayAlert("Aviso", "Por favor, selecione um utente.", "OK");
            return;
        }

        string awsImageUrl = string.Empty;

        // 1. Faz o Upload da foto para a AWS usando o caminho físico do ficheiro
        if (_fotoSelecionada != null)
        {
            awsImageUrl = await _tarefaService.UploadFotoAdHocAsync(_fotoSelecionada.FullPath);

            if (string.IsNullOrEmpty(awsImageUrl))
            {
                // Não cancelamos o fluxo, apenas avisamos. Garante que a app não bloqueia na defesa!
                await Shell.Current.DisplayAlert("Aviso", "A foto não foi guardada, mas vamos submeter o registo principal.", "OK");
            }
        }

        // 2. Prepara o DTO com a URL gerada (ou vazia, caso não haja foto)
        var dataFinal = DateTime.Today.Add(HoraSelecionada);

        var dto = new RegistoAdHocDto
        {
            UtenteId = UtenteSelecionado.Id,
            Titulo = $"Ocorrência: {CategoriaSelecionada}",
            Categoria = CategoriaSelecionada,
            Notas = Notas ?? string.Empty,
            DataHora = dataFinal,
            FotoUrl = awsImageUrl
        };

        // 3. Grava a tarefa na API
        bool sucesso = await _tarefaService.RegistarAdHocAsync(dto);

        if (sucesso)
        {
            await Shell.Current.DisplayAlert("Sucesso", "Registo submetido com sucesso!", "OK");
            await Shell.Current.GoToAsync(".."); // Volta ao ecrã anterior
        }
        else
        {
            await Shell.Current.DisplayAlert("Erro", "Falha ao enviar o registo principal para a API.", "OK");
        }
    }
}