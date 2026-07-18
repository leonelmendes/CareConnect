using CareConnect.Mobile.ViewModels.Gestor;

namespace CareConnect.Mobile.Views.Gestor;

public partial class DetalhePlanoView : ContentPage
{
	public DetalhePlanoView(DetalhePlanoViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}