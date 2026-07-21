using CommunityToolkit.Mvvm.Messaging.Messages;
using CareConnect.Shared.Models;

namespace CareConnect.Mobile.Messages;

// Esta mensagem vai carregar o Utente atualizado
public class PatientUpdatedMessage : ValueChangedMessage<Patient>
{
    public PatientUpdatedMessage(Patient patient) : base(patient) { }
}