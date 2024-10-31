using PopIn_PaymentForm_.NET.Models;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text;
using JsonSerializer = System.Text.Json.JsonSerializer;
using Newtonsoft.Json;
using PopIn_PaymentForm_.NET.Models;

namespace PopIn_PaymentForm_.NET.Controllers
{
    public class PaymentController : Controller
    {
        public async Task<IActionResult> Index()
        {
            var nombre = HttpContext.Request.Form["txt_nombre"];
            var apellido = HttpContext.Request.Form["txt_apellido"];
            var correo = HttpContext.Request.Form["txt_correo"];
            var valor = HttpContext.Request.Form["txt_monto"];

            // Convierte el monto a céntimos
            int monto = ConvertToCents(valor);

            // Llama al método de la API y obtiene el token
            string valorjson = await Apis(nombre, apellido, correo, monto);
            var jsondeserialze = JsonSerializer.Deserialize<JsonElement>(valorjson);
            ViewBag.token = jsondeserialze.GetProperty("answer").GetProperty("formToken").GetString();

            return View();

        }

        // Método para convertir el monto a céntimos
        private int ConvertToCents(string valor)
        {
            if (decimal.TryParse(valor, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal montoDecimal))
            {
                return (int)(montoDecimal * 100);
            }
            return 0;
        }
        [HttpPost]
        public IActionResult respuesta()
        {
            var valor = HttpContext.Request.Form["kr-answer"];
            var jsondeserialze = JsonConvert.DeserializeObject<dynamic>(valor);

            ViewBag.Valor = jsondeserialze;

            return View();
        }

        [HttpPost]
        [ActionName("validate")]
        public string validate()
        {
            PaymentModel gm = new PaymentModel();
            var validate = HttpContext.Request;
            string key = "";
            var calculatedHash = "";

            if ("sha256_hmac" != validate.Form["kr_hash_algorithm"])
            {
                return JsonConvert.SerializeObject("false");
            }


            string krAnswer = validate.Form["kr_answer"].ToString().Replace("\\/", "/");
            if (validate.Form["kr_hash_key"] == "sha256_hmac")
            {
                key = gm.KEY_SHA256;
            }
            else if (validate.Form["kr_hash_key"] == "password")
            {
                key = gm.KEY_PASSWORD;
            }
            else
            {
                return JsonConvert.SerializeObject("false");
            }

            calculatedHash = hmac256(key, krAnswer);

            if (calculatedHash == validate.Form["kr_hash"]) return JsonConvert.SerializeObject("true");

            calculatedHash = hmac256(key, Decoder(krAnswer));

            if (calculatedHash == validate.Form["kr_hash"]) return JsonConvert.SerializeObject("true");

            return JsonConvert.SerializeObject("false");
        }

        public string Decoder(string value)
        {
            Encoding enc = new UTF8Encoding();
            byte[] bytes = enc.GetBytes(value);
            return enc.GetString(bytes);
        }

        public string hmac256(string key, string krAnswer)
        {

            ASCIIEncoding encoder = new ASCIIEncoding();
            Byte[] code = encoder.GetBytes(key);
            HMACSHA256 hmSha256 = new HMACSHA256(code);
            Byte[] hashMe = encoder.GetBytes(krAnswer);
            Byte[] hmBytes = hmSha256.ComputeHash(hashMe);
            return ToHexString(hmBytes);
        }

        public static string ToHexString(byte[] array)
        {
            StringBuilder hex = new StringBuilder(array.Length * 2);
            foreach (byte b in array)
            {
                hex.AppendFormat("{0:x2}", b);
            }
            return hex.ToString();
        }

        public string Encriptado()
        {
            var gm = new PaymentModel();
            string str = $"{gm.KEY_USER}:{gm.KEY_PASSWORD}";
            byte[] encbuff = Encoding.UTF8.GetBytes(str);
            return Convert.ToBase64String(encbuff);
        }

        public async Task<string> Apis(string nombre, string apellido, string correo, int monto)
        {
            var gm = new PaymentModel();
            string path = "api-payment/V4/Charge/CreatePayment";

            // Crear un objeto JSON que contiene los detalles del pago.
            var objjson = new
            {
                amount = monto,
                currency = "PEN",
                orderId = "MyOrderId-123456",
                customer = new
                {
                    email = correo,
                    billingDetails = new
                    {
                        firstName = nombre,
                        lastName = apellido,
                        phoneNumber = "933819747",
                        address = "av larco",
                        address2 = "av larco"
                    }
                }
            };

            var json = JsonSerializer.Serialize(objjson);

            var data = new StringContent(json, Encoding.UTF8, "application/json");

            using var client = new HttpClient();

            client.DefaultRequestHeaders.Add("Authorization", "Basic " + Encriptado());


            var response = await client.PostAsync(gm.URL_ENDPOINT + path, data);

            return await response.Content.ReadAsStringAsync();
        }


    }

}