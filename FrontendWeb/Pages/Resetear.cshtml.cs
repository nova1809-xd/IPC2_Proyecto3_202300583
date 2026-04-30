using FrontendWeb.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FrontendWeb.Pages
{
    /// <summary>
    /// modelo de página para limpiar los datos de la api.
    /// </summary>
    public class ResetearModel : PageModel
    {
        private readonly ApiService _apiService;

        /// <summary>
        /// constructor que recibe el servicio api.
        /// </summary>
        /// <param name="apiService">servicio para consumir la api local</param>
        public ResetearModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        /// <summary>
        /// mensaje de éxito mostrado en la página.
        /// </summary>
        public string? MensajeExito { get; set; }

        /// <summary>
        /// mensaje de error mostrado en la página.
        /// </summary>
        public string? MensajeError { get; set; }

        /// <summary>
        /// ejecuta la limpieza de datos.
        /// </summary>
        public async Task OnPostAsync()
        {
            try
            {
                // llama al endpoint que limpia la base local.
                string respuesta = await _apiService.LimpiarDatosAsync();
                MensajeExito = string.IsNullOrWhiteSpace(respuesta)
                    ? "datos limpiados correctamente."
                    : "datos limpiados correctamente.";
            }
            catch (Exception ex)
            {
                MensajeError = $"no se pudieron limpiar los datos: {ex.Message}";
            }
        }
    }
}
