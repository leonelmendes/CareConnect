using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CareConnect.Mobile.Models;

public partial class DiaSemanaModel : ObservableObject
{
    public DateTime Data { get; set; }

    // Ex: "SEG", "TER"
    public string NomeDiaCortado { get; set; } = string.Empty;

    // Ex: "27", "28"
    public string NumeroDia { get; set; } = string.Empty;

    [ObservableProperty]
    private string _corFundo = "Transparent";

    [ObservableProperty]
    private string _corTexto = "#6B7280";

    [ObservableProperty]
    private string _corPonto = "#1E40AF";

    [ObservableProperty]
    private bool _isSelected;

    public bool TemTarefas { get; set; }
}