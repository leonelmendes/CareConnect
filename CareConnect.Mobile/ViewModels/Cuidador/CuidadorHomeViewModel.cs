using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CareConnect.Mobile.Models;
using CareConnect.Mobile.Services;

namespace CareConnect.Mobile.ViewModels.Cuidador;

public partial class CuidadorHomeViewModel : ObservableObject
{
    private readonly TarefaService _tarefaService;

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

    public CuidadorHomeViewModel(TarefaService tarefaService)
    {
        _tarefaService = tarefaService;
    }

    [RelayCommand]
    private async Task CarregarDadosIniciaisAsync()
    {
        if (IsBusy) return;

        IsBusy = true; // Inicia o estado de carregamento e mostra o spinner

        await Task.Delay(50); // Dá tempo à UI para respirar

        GerarCalendarioSemanal(DateTime.Today);
        await CarregarTarefasDaDataAsync(DataSelecionada);

        IsBusy = false; // Termina o carregamento
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
            // Executamos a busca de dados de forma assíncrona
            var tarefasDaApi = await _tarefaService.ObterTarefasHojeAsync();
            var listaTemporaria = new ObservableCollection<TarefaResumo>();

            if (tarefasDaApi != null && tarefasDaApi.Any(t => t.DataHora.Date == data.Date))
            {
                foreach (var tarefa in tarefasDaApi.Where(t => t.DataHora.Date == data.Date))
                {
                    listaTemporaria.Add(tarefa);
                }
            }
            else if (data.Date == DateTime.Today.Date)
            {
                // MOCK DATA: Apenas para HOJE mostramos os cartões de teste
                listaTemporaria.Add(new TarefaResumo { Id = Guid.NewGuid(), Titulo = "Medicação da Manhã", Categoria = "Medicação", NomeUtente = "João Silva", DataHora = DateTime.Today.AddHours(8), EstaConcluida = true });
                listaTemporaria.Add(new TarefaResumo { Id = Guid.NewGuid(), Titulo = "Higiene Pessoal", Categoria = "Higiene", NomeUtente = "Maria Oliveira", DataHora = DateTime.Today.AddHours(9).AddMinutes(30), EstaConcluida = false });
                listaTemporaria.Add(new TarefaResumo { Id = Guid.NewGuid(), Titulo = "Alimentação (Almoço)", Categoria = "Alimentação", NomeUtente = "Carlos Santos", DataHora = DateTime.Today.AddHours(12).AddMinutes(30), EstaConcluida = false });
            }

            // ATUALIZA A INTERFACE DE UMA SÓ VEZ! Sem "Clear" e sem "Add" na lista visível.
            // É isto que resolve o teu congelamento de 5 segundos.
            ProximasTarefas = listaTemporaria;
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
    private async Task AbrirExecucaoTarefaAsync(TarefaResumo tarefaClicada)
    {
        if (tarefaClicada == null) return;

        var parametros = new Dictionary<string, object>
        {
            { "TarefaAtual", tarefaClicada }
        };

        await Shell.Current.GoToAsync("ExecucaoTarefaModal", parametros);
    }
}