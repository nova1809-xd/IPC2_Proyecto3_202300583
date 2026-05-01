using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FrontendWeb.Services;
using System.Text.Json;

namespace FrontendWeb.Pages
{
    public class EstadoCuentaModel : PageModel
    {
        private readonly ApiService _apiService;

        public EstadoCuentaModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        [BindProperty]
        public string? NIT { get; set; }

        public List<ClienteEstadoView> Clientes { get; set; } = new List<ClienteEstadoView>();
        public string Mensaje { get; set; } = string.Empty;

        public async Task OnPostAsync()
        {
            try
            {
                var json = await _apiService.GetEstadoCuentaAsync(NIT);
                if (string.IsNullOrWhiteSpace(json))
                {
                    Mensaje = "respuesta vacía del servidor";
                    return;
                }

                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (TryGetProperty(root, out JsonElement clientesEl, "clientes", "Clientes") && clientesEl.ValueKind == JsonValueKind.Array)
                {
                    foreach (var clienteEl in clientesEl.EnumerateArray())
                    {
                        var cliente = new ClienteEstadoView
                        {
                            NIT = GetStringProperty(clienteEl, "NIT", "nit"),
                            Nombre = GetStringProperty(clienteEl, "Nombre", "nombre"),
                            Saldo = GetDecimalProperty(clienteEl, "Saldo", "saldo")
                        };

                        if (TryGetProperty(clienteEl, out JsonElement transEl, "Transacciones", "transacciones") && transEl.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var item in transEl.EnumerateArray())
                            {
                                cliente.Transacciones.Add(new TransaccionView
                                {
                                    Tipo = GetStringProperty(item, "tipo", "Tipo"),
                                    Fecha = GetDateTimeProperty(item, "fecha", "Fecha"),
                                    Valor = GetDecimalProperty(item, "valor", "Valor"),
                                    NumeroFactura = GetStringProperty(item, "numeroFactura", "NumeroFactura"),
                                    CodigoBanco = GetIntProperty(item, "codigoBanco", "CodigoBanco"),
                                    BancoNombre = GetStringProperty(item, "bancoNombre", "BancoNombre")
                                });
                            }
                        }

                        Clientes.Add(cliente);
                    }
                }

                if (!Clientes.Any())
                {
                    Mensaje = "no se encontraron clientes para mostrar";
                }
            }
            catch (Exception ex)
            {
                Mensaje = $"error al consultar el estado de cuenta: {ex.Message}";
            }
        }

        public class ClienteEstadoView
        {
            public string NIT { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
            public decimal Saldo { get; set; }
            public List<TransaccionView> Transacciones { get; set; } = new List<TransaccionView>();
        }

        public class TransaccionView
        {
            public string Tipo { get; set; } = string.Empty;
            public DateTime Fecha { get; set; }
            public decimal Valor { get; set; }
            public string NumeroFactura { get; set; } = string.Empty;
            public int CodigoBanco { get; set; }
            public string BancoNombre { get; set; } = string.Empty;
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

        private static decimal GetDecimalProperty(JsonElement element, params string[] names)
        {
            return TryGetProperty(element, out var value, names) ? value.GetDecimal() : 0m;
        }

        private static int GetIntProperty(JsonElement element, params string[] names)
        {
            return TryGetProperty(element, out var value, names) ? value.GetInt32() : 0;
        }

        private static DateTime GetDateTimeProperty(JsonElement element, params string[] names)
        {
            return TryGetProperty(element, out var value, names) ? value.GetDateTime() : DateTime.MinValue;
        }
    }
}
