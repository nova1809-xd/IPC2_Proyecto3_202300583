using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FrontendWeb.Services;
using System.Text.Json;

namespace FrontendWeb.Pages
{
    public class ConsultarIngresosModel : PageModel
    {
        private readonly ApiService _apiService;

        public ConsultarIngresosModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        [BindProperty]
        public string MesAnio { get; set; } = string.Empty; // formato yyyy-MM

        public List<string>? Meses { get; set; }
        public List<BancoReporte>? Bancos { get; set; }
        public string Mensaje { get; set; } = string.Empty;

        public async Task OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(MesAnio) || MesAnio.Length < 7)
            {
                Mensaje = "seleccione un mes válido";
                return;
            }

            try
            {
                var parts = MesAnio.Split('-');
                int anio = int.Parse(parts[0]);
                int mes = int.Parse(parts[1]);

                var json = await _apiService.GetResumenPagosAsync(mes, anio);
                if (string.IsNullOrWhiteSpace(json))
                {
                    Mensaje = "respuesta vacía del servidor";
                    return;
                }

                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                Meses = TryGetProperty(root, out var mesEl, "Meses", "meses") 
                    ? mesEl.EnumerateArray().Select(x => x.GetString() ?? string.Empty).ToList()
                    : new List<string>();

                Bancos = new List<BancoReporte>();
                if (TryGetProperty(root, out var bancosEl, "Bancos", "bancos"))
                {
                    foreach (var b in bancosEl.EnumerateArray())
                    {
                        int codigo = GetIntProperty(b, "Codigo", "codigo");
                        string nombre = GetStringProperty(b, "Nombre", "nombre");
                        var totales = TryGetProperty(b, out var totalesEl, "Totales", "totales")
                            ? totalesEl.EnumerateArray().Select(x => x.GetDecimal()).ToList()
                            : new List<decimal>();

                        Bancos.Add(new BancoReporte { Codigo = codigo, Nombre = nombre, Totales = totales });
                    }
                }

                if (Bancos.Count == 0)
                {
                    Mensaje = "no se encontraron pagos en el periodo solicitado";
                }
            }
            catch (Exception ex)
            {
                Mensaje = $"error al consultar ingresos: {ex.Message}";
            }
        }

        public class BancoReporte
        {
            public int Codigo { get; set; }
            public string Nombre { get; set; } = string.Empty;
            public List<decimal> Totales { get; set; } = new List<decimal>();
        }

        private static bool TryGetProperty(JsonElement element, out JsonElement value, params string[] names)
        {
            foreach (var name in names)
            {
                if (element.TryGetProperty(name, out value))
                {
                    return true;
                }
            }

            value = default;
            return false;
        }

        private static string GetStringProperty(JsonElement element, params string[] names)
        {
            return TryGetProperty(element, out var value, names) ? (value.GetString() ?? string.Empty) : string.Empty;
        }

        private static int GetIntProperty(JsonElement element, params string[] names)
        {
            return TryGetProperty(element, out var value, names) ? value.GetInt32() : 0;
        }
    }
}
