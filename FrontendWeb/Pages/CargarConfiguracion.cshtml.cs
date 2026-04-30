using FrontendWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FrontendWeb.Pages
{
    /// <summary>
    /// modelo de página para cargar la configuración desde un archivo xml.
    /// aquí se lee el archivo como texto y se manda a la api.
    /// </summary>
    public class CargarConfiguracionModel : PageModel
    {
        private readonly ApiService _apiService;

        /// <summary>
        /// constructor que recibe el servicio api por inyección.
        /// </summary>
        /// <param name="apiService">servicio para consumir la api local</param>
        public CargarConfiguracionModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        /// <summary>
        /// archivo subido por el usuario.
        /// </summary>
        [BindProperty]
        public IFormFile? ArchivoConfiguracion { get; set; }

        /// <summary>
        /// respuesta xml devuelta por el backend.
        /// </summary>
        public string? RespuestaXml { get; set; }

        /// <summary>
        /// maneja el post de la página.
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            if (ArchivoConfiguracion == null || ArchivoConfiguracion.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "sube un archivo xml válido.");
                return Page();
            }

            // lee el archivo como string para mandarlo por post a la api.
            using var reader = new StreamReader(ArchivoConfiguracion.OpenReadStream());
            string xml = await reader.ReadToEndAsync();

            RespuestaXml = await _apiService.GrabarConfiguracionAsync(xml);
            return Page();
        }
    }
}
