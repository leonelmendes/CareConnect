using CareConnect.Mobile.ViewModels.Cuidador;

namespace CareConnect.Mobile.Views.Cuidador;

public partial class TarefasView : ContentPage
{
	public TarefasView(TarefasViewModel viewModel)
	{
		InitializeComponent();

		BindingContext = viewModel;
	}
}