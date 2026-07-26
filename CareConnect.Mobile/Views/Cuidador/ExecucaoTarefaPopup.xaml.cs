using CommunityToolkit.Maui.Views;
using CareConnect.Mobile.ViewModels.Cuidador;

namespace CareConnect.Mobile.Views.Cuidador;

public partial class ExecucaoTarefaPopup : Popup
{
    public ExecucaoTarefaPopup(ExecucaoTarefaViewModel viewModel)
    {
        InitializeComponent();

        // Passa para a ViewModel a capacidade de fechar este Popup XAML
        viewModel.FecharPopupAcao = () =>
        {
            this.CloseAsync();
        };

        BindingContext = viewModel;
    }
}