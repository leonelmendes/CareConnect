using CareConnect.Mobile.ViewModels.Auth;

namespace CareConnect.Mobile.Views.Auth;

public partial class OnboardingView : ContentPage
{
	public OnboardingView(OnboardingViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
    
	// Interceta o botão de voltar do hardware (Android) e bloqueia a ação.
    protected override bool OnBackButtonPressed()
    {
        // Devolver 'true' cancela a navegação para trás.
        return true; 
    }
}