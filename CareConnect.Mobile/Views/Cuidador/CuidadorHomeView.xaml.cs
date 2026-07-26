using CareConnect.Mobile.ViewModels.Cuidador;

namespace CareConnect.Mobile.Views.Cuidador;

public partial class CuidadorHomeView : ContentPage
{
    private readonly CuidadorHomeViewModel _viewModel;

    public CuidadorHomeView(CuidadorHomeViewModel viewModel)
    {
        InitializeComponent();
        
        _viewModel = viewModel;
        
        BindingContext = _viewModel; 
    }

    // 2. Dispara a busca de dados sempre que o ecrã aparece
    protected override void OnAppearing()
    {
        base.OnAppearing();

        // O Toolkit gera "Command" no final do nome do teu método original
        if (_viewModel.CarregarDadosIniciaisCommand.CanExecute(null))
        {
            _viewModel.CarregarDadosIniciaisCommand.Execute(null);
        }
    }
}