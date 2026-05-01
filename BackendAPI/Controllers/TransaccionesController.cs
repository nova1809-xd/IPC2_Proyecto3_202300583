using Microsoft.AspNetCore.Mvc;
using BackendAPI.Services;
using System.Globalization;
using System.Linq;

namespace BackendAPI.Controllers
{
    /// <summary>
    /// controlador que expone los endpoints para procesar transacciones de configuración, facturas y pagos.
    /// utiliza los servicios de negocio para aplicar la lógica del sistema de facturación.
    /// todos los datos se persisten en archivos XML locales usando el XmlDatabaseService.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TransaccionesController : ControllerBase
    {
        private readonly TransaccionesConfiguracionService _servicioConfiguracion;
        private readonly TransaccionesServicio _servicioTransacciones;

        /// <summary>
        /// constructor que recibe las instancias de los servicios através de inyección de dependencias.
        /// los servicios se registran en el Program.cs.
        /// </summary>
        public TransaccionesController(
            TransaccionesConfiguracionService servicioConfiguracion,
            TransaccionesServicio servicioTransacciones)
        {
            _servicioConfiguracion = servicioConfiguracion;
            _servicioTransacciones = servicioTransacciones;
        }

        /// <summary>
        /// endpoint que recibe un string XML con configuración de clientes y bancos.
        /// utiliza expresiones regulares para extraer los datos, valida duplicados y guarda
        /// todo en los archivos XML de la base de datos. retorna un resumen de operaciones en XML.
        /// 
        /// ejemplo de entrada XML:
        /// {@code
        /// <configuracion>
        ///   <Cliente>
        ///     <NIT>1234567890101</NIT>
        ///     <nombre>Empresa A</nombre>
        ///   </Cliente>
        ///   <Banco>
        ///     <codigo>001</codigo>
        ///     <nombre>Banco Guatemalteco</nombre>
        ///   </Banco>
        /// </configuracion>
        /// }
        /// </summary>
        /// <param name="xmlConfiguracion">string con el contenido XML de configuración</param>
        /// <returns>string XML con el resumen de clientes creados/actualizados y bancos</returns>
        [HttpPost("grabarConfiguracion")]
        public async Task<IActionResult> GrabarConfiguracion()
        {
            string xmlConfiguracion;

            using (var reader = new StreamReader(Request.Body))
            {
                xmlConfiguracion = await reader.ReadToEndAsync();
            }

            // valida que el XML no sea nulo o vacío
            if (string.IsNullOrWhiteSpace(xmlConfiguracion))
            {
                return BadRequest("el contenido XML no puede estar vacío");
            }

            // procesa la configuración usando el servicio
            string xmlRespuesta = _servicioConfiguracion.ProcesarConfiguracion(xmlConfiguracion);

            // retorna la respuesta en formato XML
            // se especifica el content type como application/xml
            return Content(xmlRespuesta, "application/xml");
        }

        /// <summary>
        /// endpoint que recibe un string XML con facturas y pagos.
        /// extrae los datos usando expresiones regulares, valida duplicados, aplica la lógica
        /// de abono (pagos a facturas más antiguas) y retorna un resumen en XML.
        /// 
        /// ejemplo de entrada XML:
        /// {@code
        /// <transacciones>
        ///   <Factura>
        ///     <NumeroFactura>FAC-001</NumeroFactura>
        ///     <NITcliente>1234567890101</NITcliente>
        ///     <Fecha>2026-04-29 10:00:00</Fecha>
        ///     <Valor>5000</Valor>
        ///   </Factura>
        ///   <Pago>
        ///     <CodigoBanco>001</CodigoBanco>
        ///     <Fecha>2026-04-29 14:00:00</Fecha>
        ///     <NITcliente>1234567890101</NITcliente>
        ///     <Valor>5000</Valor>
        ///   </Pago>
        /// </transacciones>
        /// }
        /// </summary>
        /// <param name="xmlTransacciones">string con el contenido XML de transacciones</param>
        /// <returns>string XML con el resumen de facturas, pagos, duplicados, errores y saldos a favor</returns>
        [HttpPost("grabarTransaccion")]
        public async Task<IActionResult> GrabarTransaccion()
        {
            string xmlTransacciones;

            using (var reader = new StreamReader(Request.Body))
            {
                xmlTransacciones = await reader.ReadToEndAsync();
            }

            // valida que el XML no sea nulo o vacío
            if (string.IsNullOrWhiteSpace(xmlTransacciones))
            {
                return BadRequest("el contenido XML no puede estar vacío");
            }

            // procesa las transacciones usando el servicio
            string xmlRespuesta = _servicioTransacciones.ProcesarTransacciones(xmlTransacciones);

            // retorna la respuesta en formato XML
            return Content(xmlRespuesta, "application/xml");
        }

        /// <summary>
        /// endpoint que borra todos los registros de la base de datos XML.
        /// elimina todas las facturas, pagos, clientes y bancos guardados.
        /// ¡usa con cuidado! esta operación es destructiva y no se puede deshacer fácilmente.
        /// 
        /// nota: la ruta tiene un error tipográfico intencional "limipiarDatos" (debería ser "limpiarDatos").
        /// se mantiene así según los requisitos del proyecto.
        /// </summary>
        /// <returns>mensaje confirmando que los datos fueron eliminados</returns>
        [HttpPost("/limpiarDatos")]
        [HttpPost("/limipiarDatos")]
        public IActionResult LimpiarDatos()
        {
            try
            {
                // obtiene la instancia singleton del servicio de base de datos
                var dbService = XmlDatabaseService.GetInstance();

                // borra todos los clientes
                dbService.GuardarClientes(new List<Models.Cliente>());

                // borra todos los bancos
                dbService.GuardarBancos(new List<Models.Banco>());

                // borra todas las facturas
                dbService.GuardarFacturas(new List<Models.Factura>());

                // borra todos los pagos
                dbService.GuardarPagos(new List<Models.Pago>());

                // retorna un mensaje de éxito en XML
                string respuesta = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                                   "<respuesta>\n" +
                                   "  <mensaje>todos los datos han sido eliminados exitosamente</mensaje>\n" +
                                   "</respuesta>";

                return Content(respuesta, "application/xml");
            }
            catch (Exception ex)
            {
                // en caso de error, retorna un mensaje de error
                string respuesta = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                                   "<respuesta>\n" +
                                   $"  <error>{System.Web.HttpUtility.HtmlEncode(ex.Message)}</error>\n" +
                                   "</respuesta>";

                return StatusCode(StatusCodes.Status500InternalServerError, respuesta);
            }
        }

        /// <summary>
        /// endpoint que devuelve el estado de cuenta de un cliente.
        /// por ahora esta es una estructura base que retorna 200 OK.
        /// en releases posteriores se implementará para retornar:
        /// - facturas pendientes
        /// - pagos realizados
        /// - saldos a favor
        /// </summary>
        /// <returns>código 200 OK</returns>
        [HttpGet("devolverEstadoCuenta")]
        public IActionResult DevolverEstadoCuenta([FromQuery] string? NIT)
        {
            try
            {
                var db = XmlDatabaseService.GetInstance();
                var clientes = db.CargarClientes().OrderBy(c => c.NIT, StringComparer.OrdinalIgnoreCase).ToList();

                if (!string.IsNullOrWhiteSpace(NIT))
                {
                    clientes = clientes
                        .Where(c => string.Equals(c.NIT, NIT, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                var facturas = db.CargarFacturas();
                var pagos = db.CargarPagos();
                var bancos = db.CargarBancos().ToDictionary(b => b.Codigo, b => b.Nombre);

                var estados = clientes
                    .Select(cliente => ConstruirEstadoCuenta(cliente, facturas, pagos, bancos))
                    .ToList();

                return new JsonResult(new { clientes = estados });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"error: {ex.Message}");
            }
        }

        private static EstadoCuentaClienteDto ConstruirEstadoCuenta(
            Models.Cliente cliente,
            List<Models.Factura> facturas,
            List<Models.Pago> pagos,
            Dictionary<int, string> bancos)
        {
            var facturasCliente = facturas.Where(f => string.Equals(f.NITcliente, cliente.NIT, StringComparison.OrdinalIgnoreCase)).ToList();
            var pagosCliente = pagos.Where(p => string.Equals(p.NITcliente, cliente.NIT, StringComparison.OrdinalIgnoreCase)).ToList();

            var transacciones = new List<EstadoCuentaTransaccionDto>();

            foreach (var factura in facturasCliente)
            {
                transacciones.Add(new EstadoCuentaTransaccionDto
                {
                    Tipo = "Factura",
                    NumeroFactura = factura.NumeroFactura,
                    Fecha = factura.Fecha,
                    Valor = factura.Valor
                });
            }

            foreach (var pago in pagosCliente)
            {
                string nombreBanco = bancos.TryGetValue(pago.CodigoBanco, out var bancoNombre) ? bancoNombre : string.Empty;

                transacciones.Add(new EstadoCuentaTransaccionDto
                {
                    Tipo = "Pago",
                    CodigoBanco = pago.CodigoBanco,
                    BancoNombre = nombreBanco,
                    Fecha = pago.Fecha,
                    Valor = pago.Valor
                });
            }

            return new EstadoCuentaClienteDto
            {
                NIT = cliente.NIT,
                Nombre = cliente.Nombre,
                Saldo = facturasCliente.Sum(x => x.Valor) - pagosCliente.Sum(x => x.Valor),
                Transacciones = transacciones.OrderByDescending(t => t.Fecha).ToList()
            };
        }

        /// <summary>
        /// endpoint que devuelve un resumen de pagos realizados.
        /// por ahora esta es una estructura base que retorna 200 OK.
        /// en releases posteriores se implementará para retornar:
        /// - pagos por cliente
        /// - pagos por banco
        /// - pagos por rango de fechas
        /// </summary>
        /// <returns>código 200 OK</returns>
        [HttpGet("devolverResumenPagos")]
        public IActionResult DevolverResumenPagos([FromQuery] int? mes, [FromQuery] int? anio)
        {
            if (!mes.HasValue || !anio.HasValue)
            {
                return BadRequest("se requieren los parámetros 'mes' y 'anio' en la query");
            }

            try
            {
                // construye la lista de meses: mes solicitado y los dos anteriores
                var meses = new List<DateTime>();
                var fechaBase = new DateTime(anio.Value, mes.Value, 1);
                for (int i = 0; i < 3; i++)
                {
                    meses.Add(fechaBase.AddMonths(-i));
                }

                var db = XmlDatabaseService.GetInstance();
                var pagos = db.CargarPagos();
                var bancos = db.CargarBancos().ToDictionary(b => b.Codigo, b => b.Nombre);

                // obtiene lista única de códigos de banco presentes en los pagos
                var codigosBancos = pagos.Select(p => p.CodigoBanco).Distinct().ToList();

                var bancosReporte = new List<object>();

                foreach (var codigo in codigosBancos)
                {
                    var nombre = bancos.ContainsKey(codigo) ? bancos[codigo] : string.Empty;
                    var totales = new List<decimal>();

                    foreach (var m in meses)
                    {
                        decimal total = pagos.Where(p => p.CodigoBanco == codigo && p.Fecha.Year == m.Year && p.Fecha.Month == m.Month)
                                             .Sum(p => p.Valor);
                        totales.Add(total);
                    }

                    bancosReporte.Add(new
                    {
                        Codigo = codigo,
                        Nombre = nombre,
                        Totales = totales
                    });
                }

                var etiquetas = meses.Select(d => d.ToString("yyyy-MM", CultureInfo.InvariantCulture)).ToList();

                var resultado = new
                {
                    Meses = etiquetas,
                    Bancos = bancosReporte
                };

                return new JsonResult(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"error: {ex.Message}");
            }
        }
    }

    public class EstadoCuentaClienteDto
    {
        public string NIT { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public decimal Saldo { get; set; }
        public List<EstadoCuentaTransaccionDto> Transacciones { get; set; } = new List<EstadoCuentaTransaccionDto>();
    }

    public class EstadoCuentaTransaccionDto
    {
        public string Tipo { get; set; } = string.Empty;
        public string NumeroFactura { get; set; } = string.Empty;
        public int CodigoBanco { get; set; }
        public string BancoNombre { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public decimal Valor { get; set; }
    }
}
