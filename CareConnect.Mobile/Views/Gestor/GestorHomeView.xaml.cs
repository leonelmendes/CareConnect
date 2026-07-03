using CareConnect.Mobile.ViewModels.Gestor;

namespace CareConnect.Mobile.Views.Gestor;

public partial class GestorHomeView : ContentPage
{
	public GestorHomeView(GestorHomeViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}