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
        
        if (BindingContext is UtentesViewModel vm)
        {
            // Executa em segundo plano sem travar a abertura da tela!
            vm.CarregarUtentesAsyncCommand.Execute(null);
        }
    }
}