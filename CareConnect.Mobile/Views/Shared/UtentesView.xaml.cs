using CareConnect.Mobile.ViewModels.Shared;

namespace CareConnect.Mobile.Views.Shared;

public partial class UtentesView : ContentPage
{
    public UtentesView(UtentesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Sempre que a página abrir, dispara o carregamento da API
        if (BindingContext is UtentesViewModel vm)
        {
            if (vm.ListaUtentes.Count == 0)
            {
                vm.AtualizarListaCommand.Execute(null);
            }
        }
    }
}