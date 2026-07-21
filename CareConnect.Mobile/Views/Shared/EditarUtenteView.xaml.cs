using CareConnect.Mobile.ViewModels.Shared;

namespace CareConnect.Mobile.Views.Shared;

public partial class EditarUtenteView : ContentPage
{
    public EditarUtenteView(EditarUtenteViewModel viewModel)
    {
        InitializeComponent();
        
        // Liga a View à ViewModel recebida por Injeção de Dependência
        BindingContext = viewModel;
    }
}