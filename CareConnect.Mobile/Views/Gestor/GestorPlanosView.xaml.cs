using CareConnect.Mobile.ViewModels.Gestor;

namespace CareConnect.Mobile.Views.Gestor;

public partial class GestorPlanosView : ContentPage
{
	public GestorPlanosView(GestorPlanosViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}

	protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is GestorPlanosViewModel vm)
        {
            vm.CarregarPlanosCommand.Execute(null);
        }
    }
}