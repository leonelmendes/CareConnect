using CareConnect.Mobile.ViewModels.Gestor;

namespace CareConnect.Mobile.Views.Gestor;

public partial class SelecaoUtenteRelatorioView : ContentPage
{
	public SelecaoUtenteRelatorioView(SelecaoUtenteRelatorioViewModel viewModel)
	{
		InitializeComponent();

		BindingContext = viewModel;
	}
}