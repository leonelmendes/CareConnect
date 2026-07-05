using System;
using System.Collections.Generic;
using System.Text;

namespace CareConnect.Mobile.Models
{
    public class UploadAvatarResponse
    {
        public bool Sucesso { get; set; }
        public string AvatarUrl { get; set; } = string.Empty;
        public string Mensagem { get; set; } = string.Empty;
    }
}
