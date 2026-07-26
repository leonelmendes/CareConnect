
using CareConnect.Mobile.ViewModels.Shared;

namespace CareConnect.Mobile.Views.Shared;

public partial class PerfilView : ContentPage
{
    private readonly PerfilViewModel _viewModel;

    public PerfilView(PerfilViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Garante que os dados estão atualizados sempre que a página é aberta
        _viewModel.CarregarDadosPerfil();
    }
}