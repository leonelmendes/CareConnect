using CareConnect.Mobile.ViewModels.Gestor;

namespace CareConnect.Mobile.Views.Gestor;

public partial class CriarPlanoCuidadoView : ContentPage
{
	public CriarPlanoCuidadoView(CriarPlanoCuidadoViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}