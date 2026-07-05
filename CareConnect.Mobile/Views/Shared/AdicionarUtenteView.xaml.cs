using CareConnect.Mobile.ViewModels.Gestor;

namespace CareConnect.Mobile.Views.Shared;

public partial class AdicionarUtenteView : ContentPage
{
    public AdicionarUtenteView(AdicionarUtenteViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}