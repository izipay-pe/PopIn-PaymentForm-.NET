namespace PopIn_PaymentForm_.NET.Models
{
    public class PaymentModel
    {
        //Usuario
        private const string usuario = "~~CHANGE_ME_USER~~";

        //Clave API REST de TEST o PRODUCCIÓN
        private const string contraseña = "~~CHANGE_ME_KEY~~";

        //Clave JavaScript de TEST o PRODUCCIÓN
        private const string claveJS = "~~CHANGE_ME_PUBLIC_KEY~~";

        //Clave HMAC-SHA-256 de TEST o PRODUCCIÓN
        private const string claveSHA256 = "~~CHANGE_ME_KEY_HMAC-SHA-256~~";

        //URL de servidor de IZIPAY
        private const string servidorAPI = "https://api.micuentaweb.pe/";


        public string KEY_USER => usuario;
        public string KEY_PASSWORD => contraseña;
        public string KEY_JS => claveJS;
        public string KEY_SHA256 => claveSHA256;
        public string URL_ENDPOINT => servidorAPI;
    }
}