using System.Net.Http.Headers;

namespace FrontendWeb.Services
{
    /// <summary>
    /// servicio que centraliza las llamadas a la api local.
    /// se inyecta con httpclient para evitar crear conexiones a mano en cada página.
    /// </summary>
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        /// <summary>
        /// constructor que recibe el httpclient configurado por inyección de dependencias.
        /// </summary>
        /// <param name="httpClient">cliente http apuntando a la api local</param>
        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// envía el xml de configuración al backend.
        /// </summary>
        /// <param name="xml">contenido xml leído desde el archivo</param>
        /// <returns>respuesta xml devuelta por la api</returns>
        public async Task<string> GrabarConfiguracionAsync(string xml)
        {
            using var content = new StringContent(xml);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/xml");
            using var response = await _httpClient.PostAsync("api/Transacciones/grabarConfiguracion", content);
            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// envía el xml de transacciones al backend.
        /// </summary>
        /// <param name="xml">contenido xml leído desde el archivo</param>
        /// <returns>respuesta xml devuelta por la api</returns>
        public async Task<string> GrabarTransaccionAsync(string xml)
        {
            using var content = new StringContent(xml);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/xml");
            using var response = await _httpClient.PostAsync("api/Transacciones/grabarTransaccion", content);
            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// llama al endpoint de limpieza de datos.
        /// </summary>
        /// <returns>respuesta xml devuelta por la api</returns>
        public async Task<string> LimpiarDatosAsync()
        {
            using var response = await _httpClient.PostAsync("limpiarDatos", null);
            return await response.Content.ReadAsStringAsync();
        }
    }
}
