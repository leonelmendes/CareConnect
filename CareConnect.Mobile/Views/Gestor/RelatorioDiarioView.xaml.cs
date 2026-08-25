using CareConnect.Mobile.ViewModels.Gestor;

namespace CareConnect.Mobile.Views.Gestor;

public partial class RelatorioDiarioView : ContentPage
{
	public RelatorioDiarioView(RelatorioDiarioViewModel viewModel)
	{
		InitializeComponent();

		BindingContext = viewModel;
	}
}