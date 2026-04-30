using Microsoft.AspNetCore.Mvc;
using BackendAPI.Services;

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
        public IActionResult GrabarConfiguracion([FromBody] string xmlConfiguracion)
        {
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
        public IActionResult GrabarTransaccion([FromBody] string xmlTransacciones)
        {
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
        public IActionResult DevolverEstadoCuenta()
        {
            // estructura base vacía por ahora
            // aquí irá la lógica para generar el estado de cuenta de un cliente
            return Ok("estado de cuenta - por implementar en futuro release");
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
        public IActionResult DevolverResumenPagos()
        {
            // estructura base vacía por ahora
            // aquí irá la lógica para generar un resumen de pagos según criterios
            return Ok("resumen de pagos - por implementar en futuro release");
        }
    }
}
