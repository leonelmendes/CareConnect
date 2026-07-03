using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CareConnect.Mobile.Models;
using CareConnect.Mobile.Shells;

namespace CareConnect.Mobile.ViewModels.Auth;

[QueryProperty(nameof(Perfil), "Perfil")]
public partial class OnboardingViewModel : ObservableObject
{
    [ObservableProperty]
    private string _perfil = string.Empty;

    [ObservableProperty]
    private int _position;

    [ObservableProperty]
    private OnboardingItem _currentItem = new();

    public ObservableCollection<OnboardingItem> Slides { get; } = new();

    partial void OnPerfilChanged(string value)
    {
        CarregarSlides(value);
    }

    partial void OnPositionChanged(int value)
    {
        if (Slides.Count > 0 && value >= 0 && value < Slides.Count)
        {
            CurrentItem = Slides[value];
        }
    }

    private void CarregarSlides(string perfilSelecionado)
    {
        Slides.Clear();

        if (perfilSelecionado == "Gestor")
        {
            Slides.Add(new OnboardingItem { ImageSource = "onboarding_gestor_1", TitleBlack = "Organize os", TitleBlue = "Seus Utentes", Description = "Centralize e gerir os perfis dos seus utentes de forma simples e segura.", ButtonText = "Vamos começar" });
            Slides.Add(new OnboardingItem { ImageSource = "onboarding_gestor_2", TitleBlack = "Crie planos", TitleBlue = "de Cuidados", Description = "Monte planos personalizados de forma simples e organize rotinas com facilidade.", ButtonText = "Continuar" });
            Slides.Add(new OnboardingItem { ImageSource = "onboarding_gestor_3", TitleBlack = "Acompanhe em", TitleBlue = "Tempo Real", Description = "Monitore atendimentos, atividades e alertas em tempo real e garanta uma gestão eficiente.", ButtonText = "Começar agora" });
        }
        else if (perfilSelecionado == "Cuidador")
        {
            Slides.Add(new OnboardingItem { ImageSource = "onboarding_cuidador_1", TitleBlack = "A sua agenda", TitleBlue = "Diária", Description = "Veja suas tarefas e compromissos de hoje em um só lugar.", ButtonText = "Próximo" });
            Slides.Add(new OnboardingItem { ImageSource = "onboarding_cuidador_2", TitleBlack = "Registe com", TitleBlue = "Facilidade", Description = "Registe ações de cuidado, adicione notas e anexe fotos como comprovação em poucos passos.", ButtonText = "Próximo" });
            Slides.Add(new OnboardingItem { ImageSource = "onboarding_cuidador_3", TitleBlack = "Mantenha todos", TitleBlue = "Informados", Description = "Compartilhe relatórios em tempo real e mantenha familiares sempre atualizados sobre o cuidado.", ButtonText = "Começar agora" });
        }

        Position = 0;
    }

    [RelayCommand]
    private void Next()
    {
        // Se ainda houver slides, avança a posição
        if (Position < Slides.Count - 1)
        {
            Position++;
        }
        else
        {
            // Chegámos ao último slide e o utilizador clicou em "Começar agora"
            // Vamos trocar a raiz da aplicação com base no perfil escolhido!
            
            if (Perfil == "Gestor")
            {
                // Injeta o Shell com a TabBar do Gestor (Dashboard, Utentes, Planos)
                Application.Current.MainPage = new GestorShell();
            }
            else if (Perfil == "Cuidador")
            {
                // Injeta o Shell com a TabBar do Cuidador (Agenda Diária, Tarefas)
                Application.Current.MainPage = new CuidadorShell();
            }
            else
            {
                // Fallback de segurança caso algo falhe
                App.Current.MainPage.DisplayAlert("Erro", "Perfil inválido detetado.", "OK");
            }
        }
    }
}