using CareConnect.Mobile.Models;
using CareConnect.Mobile.Services;
using CareConnect.Mobile.Views.Cuidador;
using CareConnect.Shared.DTOs;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace CareConnect.Mobile.ViewModels.Cuidador;

public partial class CuidadorHomeViewModel : ObservableObject
{
    private readonly TarefaService _tarefaService;
    private readonly INotificationService _notificationService;

    [ObservableProperty]
    private string tituloAdHoc;

    [ObservableProperty]
    private string categoriaAdHoc;

    [ObservableProperty]
    private string notasAdHoc;

    [ObservableProperty]
    private string _nomeCuidador = "Cuidador(a)";

    [ObservableProperty]
    private string _dataAtualExtenso = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private DateTime _dataSelecionada = DateTime.Today;

    // AQUI ESTAVA O SEGREDO: Transformámos a lista num ObservableProperty
    [ObservableProperty]
    private ObservableCollection<TarefaResumo> _proximasTarefas = new();

    public ObservableCollection<DiaSemanaModel> DiasSemana { get; set; } = new();

    public CuidadorHomeViewModel(TarefaService tarefaService, INotificationService notificationService)
    {
        _tarefaService = tarefaService;
        _notificationService = notificationService;
    }

    [RelayCommand]
    private async Task CarregarDadosIniciaisAsync()
    {
        if (IsBusy) return;

        IsBusy = true; // Inicia o estado de carregamento e mostra o spinner

        try
        {
            //await Task.Delay(50); // Dá tempo à UI para respirar

            // 1. Carregar o nome do Cuidador do SecureStorage
            // (Certifica-te que tens a propriedade NomeCuidador criada no topo da ViewModel)
            var nomeGuardado = Preferences.Default.Get("user_nome", string.Empty);
            NomeCuidador = string.IsNullOrWhiteSpace(nomeGuardado) ? "Cuidador(a)" : nomeGuardado;

            // 2. Carregar o resto da UI
            GerarCalendarioSemanal(DateTime.Today);
            await CarregarTarefasDaDataAsync(DataSelecionada);
        }
        finally
        {
            IsBusy = false; // Termina o carregamento com toda a segurança
        }
    }

    [RelayCommand]
    private async Task SelecionarDiaAsync(DiaSemanaModel diaClicado)
    {
        if (diaClicado == null || IsBusy) return;

        DataSelecionada = diaClicado.Data;
        DataAtualExtenso = DataSelecionada.ToString("dddd, dd 'de' MMMM", new System.Globalization.CultureInfo("pt-PT"));

        // Atualiza as cores imediatamente
        foreach (var dia in DiasSemana)
        {
            if (dia.Data.Date == diaClicado.Data.Date)
            {
                dia.IsSelected = true;
                dia.CorFundo = "#1E40AF";
                dia.CorTexto = "White";
                dia.CorPonto = "White";
            }
            else
            {
                dia.IsSelected = false;
                dia.CorFundo = "Transparent";
                dia.CorTexto = "#6B7280";
                dia.CorPonto = "#1E40AF";
            }
        }

        IsBusy = true;
        await Task.Delay(50);

        await CarregarTarefasDaDataAsync(DataSelecionada);

        IsBusy = false;
    }

    private async Task CarregarTarefasDaDataAsync(DateTime data)
    {
        try
        {
            var tarefasDaApi = await _tarefaService.ObterTarefasPorDataAsync(data);

            // 2. A CORREÇÃO: Limpar a lista atual e adicionar os novos itens um a um
            ProximasTarefas.Clear();

            if (tarefasDaApi != null && tarefasDaApi.Any())
            {
                foreach (var tarefa in tarefasDaApi)
                {
                    ProximasTarefas.Add(tarefa);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"🔥 ERRO: {ex.Message}");
        }
    }

    private void GerarCalendarioSemanal(DateTime dataReferencia)
    {
        DiasSemana.Clear();
        DataAtualExtenso = dataReferencia.ToString("dddd, dd 'de' MMMM", new System.Globalization.CultureInfo("pt-PT"));

        var inicioSemana = dataReferencia.Date;

        for (int i = 0; i < 7; i++)
        {
            var dia = inicioSemana.AddDays(i);
            bool isHoje = dia.Date == DateTime.Today.Date;

            DiasSemana.Add(new DiaSemanaModel
            {
                Data = dia,
                NomeDiaCortado = dia.ToString("ddd", new System.Globalization.CultureInfo("pt-PT")).Substring(0, 3).ToUpper(),
                NumeroDia = dia.Day.ToString(),
                IsSelected = isHoje,
                CorFundo = isHoje ? "#1E40AF" : "Transparent",
                CorTexto = isHoje ? "White" : "#6B7280",
                CorPonto = isHoje ? "White" : "#1E40AF",
                TemTarefas = true
            });
        }
    }

    [RelayCommand]
    private async Task AbrirExecucaoTarefa(TarefaResumo tarefaClicada)
    {
        if (tarefaClicada == null) return;

        // VERIFICA SE JÁ ESTÁ CONCLUÍDA
        if (tarefaClicada.EstaConcluida)
        {
            await _notificationService.MostrarAvisoAsync("Esta tarefa já foi concluída e não pode ser alterada.");
            return; // Impede de abrir o popup!
        }

        // Passamos o serviço para a ViewModel da Modal
        var popupViewModel = new ExecucaoTarefaViewModel(tarefaClicada, _notificationService, _tarefaService);
        var popup = new ExecucaoTarefaPopup(popupViewModel);

        Application.Current.MainPage.ShowPopup(popup);
    }

    [RelayCommand]
    private async Task AbrirRegistoAdHoc()
    {
        // Navega para a página de criação de tarefa Ad-Hoc
        // Certifica-te de que a rota "RegistoAdHocView" está registada no teu AppShell
        await Shell.Current.GoToAsync("RegistoAdHocView");
    }

    [RelayCommand]
    private async Task AbrirNotas()
    {
        await Application.Current!.MainPage!.DisplayAlertAsync("Notas", "Funcionalidade de notas rápidas em breve.", "OK");
    }

    [RelayCommand]
    private async Task ConcluirTarefaAsync(TarefaResumo tarefa)
    {
        // Se a tarefa já está concluída ou é nula, não fazemos nada
        if (tarefa == null || tarefa.EstaConcluida) return;

        // Confirma o valor numérico de "Realizado" no teu Enum CareTaskStatus
        int valorStatusRealizado = 2;

        bool sucesso = await _tarefaService.AtualizarEstadoTarefaAsync(tarefa.Id, valorStatusRealizado, "Concluído via App Mobile");

        if (sucesso)
        {
            // Atualiza visualmente no telemóvel
            tarefa.EstaConcluida = true;
        }
    }

    [RelayCommand]
    private async Task RegistarTarefaAdHocAsync()
    {
        // 1. Validação básica
        if (string.IsNullOrWhiteSpace(TituloAdHoc))
        {
            await _notificationService.MostrarAvisoAsync("O título da tarefa é obrigatório.");
            return;
        }

        // 2. Criar o DTO
        var novaTarefa = new RegistoAdHocDto
        {
            // ATENÇÃO: Para testar, precisas de um Guid de um Utente que exista na tua BD.
            // Se ainda não tiveres a lógica para ir buscar o Utente atual, coloca o Guid de um Utente fixo que tenhas criado diretamente no SQL Server para testes.
            UtenteId = Guid.Parse("5d91ce8a-cf7c-4d8f-8e30-4cc2c444bff8"),

            Titulo = TituloAdHoc,
            Categoria = string.IsNullOrWhiteSpace(CategoriaAdHoc) ? "Geral" : CategoriaAdHoc,
            Notas = NotasAdHoc,

            // Regista a tarefa para o dia que estás a visualizar no calendário
            DataHora = DateTime.UtcNow
        };

        // 3. Enviar para a API
        bool sucesso = await _tarefaService.RegistarAdHocAsync(novaTarefa);

        if (sucesso)
        {
            // 4. Limpar o formulário do BottomSheet
            TituloAdHoc = string.Empty;
            CategoriaAdHoc = string.Empty;
            NotasAdHoc = string.Empty;

            // 5. Recarregar a lista do ecrã para a nova tarefa aparecer imediatamente!
            // Substitui "DataSelecionada" pela variável que guarda o dia atual no teu calendário
            await CarregarTarefasDaDataAsync(DateTime.Now);

            await _notificationService.MostrarSucessoAsync("Tarefa Ad-Hoc registada!");

            // Aqui podes adicionar o código para fechar o BottomSheet, se necessário.
        }
        else
        {
            await _notificationService.MostrarErroAsync("Não foi possível registar a tarefa.");
        }
    }
}