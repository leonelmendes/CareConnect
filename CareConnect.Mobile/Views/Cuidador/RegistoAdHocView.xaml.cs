using CareConnect.Mobile.ViewModels.Cuidador;

namespace CareConnect.Mobile.Views.Cuidador;

public partial class RegistoAdHocView : ContentPage
{
    public RegistoAdHocView(RegistoAdHocViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}