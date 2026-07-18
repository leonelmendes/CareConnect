using CareConnect.Mobile.ViewModels.Gestor;

namespace CareConnect.Mobile.Views.Gestor;

public partial class GestorHomeView : ContentPage
{
    public GestorHomeView(GestorHomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        if (BindingContext is GestorHomeViewModel vm)
        {
            // Dispara a busca de dados na API. O Shimmer vai ativar/desativar
            // automaticamente baseado no estado da variável 'IsLoading'.
            //await Task.Delay(5000); // Pequeno delay para garantir que a UI está pronta antes de iniciar a carga de dados
            vm.CarregarDadosHomeCommand.Execute(null);
        }
    }
}