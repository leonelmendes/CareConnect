using CareConnect.Mobile.ViewModels.Shared;

namespace CareConnect.Mobile.Views.Shared;

public partial class UtentesView : ContentPage
{
    public UtentesView(UtentesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}