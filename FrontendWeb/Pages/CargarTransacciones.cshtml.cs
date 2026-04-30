using FrontendWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FrontendWeb.Pages
{
    /// <summary>
    /// modelo de página para cargar transacciones desde un archivo xml.
    /// se lee el contenido completo del archivo y se manda a la api.
    /// </summary>
    public class CargarTransaccionesModel : PageModel
    {
        private readonly ApiService _apiService;

        /// <summary>
        /// constructor que recibe el servicio api.
        /// </summary>
        /// <param name="apiService">servicio para consumir la api local</param>
        public CargarTransaccionesModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        /// <summary>
        /// archivo subido por el usuario.
        /// </summary>
        [BindProperty]
        public IFormFile? ArchivoTransacciones { get; set; }

        /// <summary>
        /// respuesta xml devuelta por la api.
        /// </summary>
        public string? RespuestaXml { get; set; }

        /// <summary>
        /// procesa el archivo subido.
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            if (ArchivoTransacciones == null || ArchivoTransacciones.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "sube un archivo xml válido.");
                return Page();
            }

            // lee el archivo como texto para mandarlo por post a la api.
            using var reader = new StreamReader(ArchivoTransacciones.OpenReadStream());
            string xml = await reader.ReadToEndAsync();

            RespuestaXml = await _apiService.GrabarTransaccionAsync(xml);
            return Page();
        }
    }
}
