using CareConnect.Mobile.ViewModels.Auth;

namespace CareConnect.Mobile.Views.Auth;

public partial class ProfileSelectionView : ContentPage
{
	private string _selectedProfile = string.Empty;
	public ProfileSelectionView( ProfileSelectionViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

	private async void OnGestorTapped(object sender, TappedEventArgs e)
    {
        var vm = (ProfileSelectionViewModel)BindingContext;
        if (vm.SelectedProfile == "Gestor") return;
        
        vm.SelectedProfile = "Gestor"; // Informa o cérebro (ViewModel)

        // Reseta o outro cartão para o estado padrão
        CardCuidador.Stroke = Color.FromArgb("#E0E0E0"); 
        CardCuidador.BackgroundColor = Colors.White;
        CheckCuidador.IsVisible = false;
        CheckCuidador.Opacity = 0;

        // Lógica puramente visual (Animações)
        await CardGestor.ScaleTo(0.95, 100, Easing.CubicOut);
        CardGestor.Stroke = Color.FromArgb("#0052CC"); 
        CardGestor.BackgroundColor = Color.FromArgb("#F4F8FF");
        CheckGestor.Opacity = 0;
        CheckGestor.IsVisible = true;

        await Task.WhenAll(
            CardGestor.ScaleTo(1.0, 100, Easing.CubicIn),
            CheckGestor.FadeTo(1, 150)
        );
    }

    private async void OnCuidadorTapped(object sender, TappedEventArgs e)
    {
        var vm = (ProfileSelectionViewModel)BindingContext;
        if (vm.SelectedProfile == "Cuidador") return;

        vm.SelectedProfile = "Cuidador"; // Informa o cérebro (ViewModel)

        // Reseta o outro cartão para o estado padrão(Primeiro para nao ter conflito com a animação do outro cartão)
        CardGestor.Stroke = Color.FromArgb("#E0E0E0"); 
        CardGestor.BackgroundColor = Colors.White;
        CheckGestor.IsVisible = false;
        CheckGestor.Opacity = 0;

        // Lógica puramente visual (Animações)
        await CardCuidador.ScaleTo(0.95, 100, Easing.CubicOut);
        CardCuidador.Stroke = Color.FromArgb("#0052CC"); 
        CardCuidador.BackgroundColor = Color.FromArgb("#F4F8FF");
        CheckCuidador.Opacity = 0;
        CheckCuidador.IsVisible = true;

        await Task.WhenAll(
            CardCuidador.ScaleTo(1.0, 100, Easing.CubicIn),
            CheckCuidador.FadeTo(1, 150)
        );

    }

    // Interceta o botão de voltar do hardware (Android) e bloqueia a ação.
    protected override bool OnBackButtonPressed()
    {
        // Devolver 'true' cancela a navegação para trás.
        return true; 
    }
}