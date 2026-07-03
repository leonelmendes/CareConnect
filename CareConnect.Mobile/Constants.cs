namespace CareConnect.Mobile
{
    public class Constants
    {
        public static string BaseUrl
        {
            get
            {
#if ANDROID
            return "http://10.0.2.2:5116"; 
#else
            return "http://localhost:5116"; 
#endif
            }
        }

        public static string LoginUrl = $"{BaseUrl}/api/Users/sync-login";
        public static string RegisterUrl = $"{BaseUrl}/api/Users";
    }
}