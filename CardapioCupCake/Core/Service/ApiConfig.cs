namespace CardapioCupCake.Core.Service
{
    /// <summary>
    /// Endereço da API. Em Debug o app fala com a API rodando no seu PC;
    /// em Release (APK dos testadores) fala com a API publicada no Render.
    /// </summary>
    public static class ApiConfig
    {
        // TROQUE pela URL que o Render gerar no deploy (mantenha a barra no final).
        public const string UrlProducao = "https://cupcake-store.onrender.com/";

        // O emulador Android enxerga o "localhost" do PC pelo IP 10.0.2.2.
        public const string UrlLocalAndroid = "http://10.0.2.2:5080/";
        public const string UrlLocal = "http://localhost:5080/";

        // Mude para false se quiser testar o build Debug contra a API do Render.
        public const bool UsarApiLocalEmDebug = true;

        public static string BaseUrl
        {
            get
            {
#if DEBUG
                if (UsarApiLocalEmDebug)
                    return DeviceInfo.Platform == DevicePlatform.Android ? UrlLocalAndroid : UrlLocal;
#endif
                return UrlProducao;
            }
        }
    }
}
