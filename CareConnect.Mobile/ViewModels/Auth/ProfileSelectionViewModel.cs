using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CareConnect.Mobile.ViewModels.Auth
{
    public partial class ProfileSelectionViewModel : ObservableObject
    {
    [ObservableProperty]
    private string _selectedProfile = string.Empty;

    // O Toolkit gera automaticamente o "ContinueCommand" a partir disto
    [RelayCommand]
    private async Task ContinueAsync()
    {
        if (string.IsNullOrEmpty(SelectedProfile))
        {
            await App.Current.MainPage.DisplayAlert("Aviso", "Por favor, selecione um perfil para continuar.", "OK");
            return;
        }

        await Shell.Current.GoToAsync($"OnboardingView?Perfil={SelectedProfile}");
    }
    }
}