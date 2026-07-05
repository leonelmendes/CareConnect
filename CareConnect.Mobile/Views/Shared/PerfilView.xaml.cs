using CareConnect.Mobile.ViewModels.Shared;

namespace CareConnect.Mobile.Views.Shared;

public partial class PerfilView : ContentPage
{
    public PerfilView(PerfilViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}