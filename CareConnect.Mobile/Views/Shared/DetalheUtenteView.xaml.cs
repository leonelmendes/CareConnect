using CareConnect.Mobile.ViewModels.Shared;

namespace CareConnect.Mobile.Views.Shared;

public partial class DetalheUtenteView : ContentPage
{
    public DetalheUtenteView(DetalheUtenteViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}