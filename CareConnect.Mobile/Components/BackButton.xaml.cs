namespace CareConnect.Mobile.Components;

public partial class BackButton : ContentView
{
    public BackButton()
    {
        InitializeComponent();
    }

    private async void OnBackTapped(object sender, TappedEventArgs e)
    {
        // Animação suave de clique (opcional, mas dá um toque premium)
        await this.ScaleTo(0.90, 100, Easing.CubicOut);
        await this.ScaleTo(1.0, 100, Easing.CubicIn);

        // Volta um passo atrás na navegação
        await Shell.Current.GoToAsync("..");
    }
}