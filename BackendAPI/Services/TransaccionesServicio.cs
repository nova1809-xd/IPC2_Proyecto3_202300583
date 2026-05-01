namespace BackendAPI.Services
{
    /// <summary>
    /// servicio que encapsula la lógica de negocio para procesar transacciones (facturas y pagos).
    /// extrae datos del XML usando expresiones regulares, aplica la lógica de abono donde cada pago
    /// se descuenta de la factura más antigua del cliente. si el pago es mayor que la factura, queda
    /// como saldo a favor. utiliza el XmlDatabaseService para persistencia.
    /// </summary>
    public class TransaccionesServicio
    {
        private readonly XmlDatabaseService _dbService;

        /// <summary>
        /// constructor que recibe la instancia del servicio de base de datos.
        /// </summary>
        /// <param name="dbService">instancia única del XmlDatabaseService</param>
        public TransaccionesServicio(XmlDatabaseService dbService)
        {
            _dbService = dbService;
        }

        /// <summary>
        /// procesa el XML de entrada que contiene facturas y pagos. extrae datos usando regex,
        /// valida duplicados, aplica la lógica de abono y genera un resumen en XML.
        /// </summary>
        /// <param name="xmlTransacciones">string con el contenido XML de transacciones</param>
        /// <returns>string XML con el resumen de facturas, pagos, duplicados y errores</returns>
        public string ProcesarTransacciones(string xmlTransacciones)
        {
            var resumen = new ResumenTransacciones();

            try
            {
                // procesa las facturas del XML
                var facturasNuevas = ProcesarFacturas(xmlTransacciones, resumen);

                // procesa los pagos del XML
                var pagosNuevos = ProcesarPagos(xmlTransacciones, resumen);

                // aplica la lógica de abono de pagos a facturas
                AplicarAbonos(facturasNuevas, pagosNuevos, resumen);
            }
            catch (Exception ex)
            {
                resumen.Error = $"error al procesar transacciones: {ex.Message}";
            }

            // genera y retorna el XML de respuesta
            return GenerarXmlRespuesta(resumen);
        }

        /// <summary>
        /// extrae las facturas del XML usando expresiones regulares y las valida.
        /// evita duplicados comparando el número de factura con las existentes.
        /// </summary>
        private List<Models.Factura> ProcesarFacturas(string xml, ResumenTransacciones resumen)
        {
            var facturasNuevas = new List<Models.Factura>();

            // busca bloques de factura con saltos de línea y espacios.
            System.Text.RegularExpressions.MatchCollection facturasMatches =
                System.Text.RegularExpressions.Regex.Matches(
                    xml,
                    @"<factura\s*>(.*?)</factura>",
                    System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.IgnoreCase
                );

            // carga las facturas existentes de la base de datos
            var facturasExistentes = _dbService.CargarFacturas();

            // procesa cada factura encontrada en el XML
            foreach (System.Text.RegularExpressions.Match match in facturasMatches)
            {
                string facturaXml = match.Value;

                // extrae el número de factura usando regex.
                System.Text.RegularExpressions.Match numeroMatch =
                    System.Text.RegularExpressions.Regex.Match(facturaXml, @"<numeroFactura\s*>(.*?)</numeroFactura>", System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                
                if (!numeroMatch.Success) continue;
                string numero = ExtraerCoincidencia(numeroMatch.Groups[1].Value, @"[A-Za-z0-9]+(?:[\s\-]+[A-Za-z0-9]+)*");

                // extrae el nit del cliente usando regex.
                System.Text.RegularExpressions.Match nitMatch =
                    System.Text.RegularExpressions.Regex.Match(facturaXml, @"<NITcliente\s*>(.*?)</NITcliente>", System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                
                if (!nitMatch.Success) continue;
                string nit = ExtraerCoincidencia(nitMatch.Groups[1].Value, @"\d+-[0-9kK]");

                // extrae la fecha usando regex.
                System.Text.RegularExpressions.Match fechaMatch =
                    System.Text.RegularExpressions.Regex.Match(facturaXml, @"<fecha\s*>(.*?)</fecha>", System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                
                string fechaTexto = fechaMatch.Success ? ExtraerCoincidencia(fechaMatch.Groups[1].Value, @"\d{2}/\d{2}/\d{4}") : string.Empty;

                if (string.IsNullOrEmpty(fechaTexto) || !DateTime.TryParse(fechaTexto, out DateTime fecha))
                {
                    resumen.FacturasConError++;
                    continue;
                }

                // extrae el valor de la factura usando regex.
                System.Text.RegularExpressions.Match valorMatch =
                    System.Text.RegularExpressions.Regex.Match(facturaXml, @"<valor\s*>(.*?)</valor>", System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                
                if (!valorMatch.Success || !decimal.TryParse(valorMatch.Groups[1].Value.Trim(), out decimal valor))
                {
                    resumen.FacturasConError++;
                    continue;
                }

                // verifica si la factura ya existe por su número único
                if (facturasExistentes.Any(f => f.NumeroFactura == numero))
                {
                    resumen.FacturasDuplicadas++;
                    continue;
                }

                // crea el objeto factura y lo agrega a las nuevas
                var factura = new Models.Factura(numero, nit, fecha, valor);
                facturasNuevas.Add(factura);
                facturasExistentes.Add(factura);
                resumen.FacturasCreadas++;
            }

            // persiste todas las facturas en la base de datos
            _dbService.GuardarFacturas(facturasExistentes);

            return facturasNuevas;
        }

        /// <summary>
        /// extrae los pagos del XML usando expresiones regulares y valida cada uno.
        /// evita duplicados verificando que no exista un pago idéntico en todos sus campos.
        /// </summary>
        private List<Models.Pago> ProcesarPagos(string xml, ResumenTransacciones resumen)
        {
            var pagosNuevos = new List<Models.Pago>();

            // busca bloques de pago con saltos de línea y espacios.
            System.Text.RegularExpressions.MatchCollection pagosMatches =
                System.Text.RegularExpressions.Regex.Matches(
                    xml,
                    @"<pago\s*>(.*?)</pago>",
                    System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.IgnoreCase
                );

            // carga los pagos existentes de la base de datos
            var pagosExistentes = _dbService.CargarPagos();

            // procesa cada pago encontrado en el XML
            foreach (System.Text.RegularExpressions.Match match in pagosMatches)
            {
                string pagoXml = match.Value;

                // extrae el código del banco usando regex.
                System.Text.RegularExpressions.Match codigoMatch =
                    System.Text.RegularExpressions.Regex.Match(pagoXml, @"<codigoBanco\s*>(.*?)</codigoBanco>", System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                
                string codigoTexto = codigoMatch.Success ? ExtraerCoincidencia(codigoMatch.Groups[1].Value, @"\d+") : string.Empty;

                if (string.IsNullOrEmpty(codigoTexto) || !int.TryParse(codigoTexto, out int codigoBanco))
                {
                    resumen.PagosConError++;
                    continue;
                }

                // extrae la fecha del pago usando regex.
                System.Text.RegularExpressions.Match fechaMatch =
                    System.Text.RegularExpressions.Regex.Match(pagoXml, @"<fecha\s*>(.*?)</fecha>", System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                
                string fechaTexto = fechaMatch.Success ? ExtraerCoincidencia(fechaMatch.Groups[1].Value, @"\d{2}/\d{2}/\d{4}") : string.Empty;

                if (string.IsNullOrEmpty(fechaTexto) || !DateTime.TryParse(fechaTexto, out DateTime fecha))
                {
                    resumen.PagosConError++;
                    continue;
                }

                // extrae el nit del cliente usando regex.
                System.Text.RegularExpressions.Match nitMatch =
                    System.Text.RegularExpressions.Regex.Match(pagoXml, @"<NITcliente\s*>(.*?)</NITcliente>", System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                
                if (!nitMatch.Success) 
                {
                    resumen.PagosConError++;
                    continue;
                }
                string nit = ExtraerCoincidencia(nitMatch.Groups[1].Value, @"\d+-[0-9kK]");

                // extrae el valor del pago usando regex.
                System.Text.RegularExpressions.Match valorMatch =
                    System.Text.RegularExpressions.Regex.Match(pagoXml, @"<valor\s*>(.*?)</valor>", System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                
                if (!valorMatch.Success || !decimal.TryParse(valorMatch.Groups[1].Value.Trim(), out decimal valor))
                {
                    resumen.PagosConError++;
                    continue;
                }

                // verifica si el pago ya existe comparando todos sus campos
                bool existePago = pagosExistentes.Any(p => 
                    p.CodigoBanco == codigoBanco && 
                    p.Fecha == fecha && 
                    p.NITcliente == nit && 
                    p.Valor == valor);

                if (existePago)
                {
                    resumen.PagosDuplicados++;
                    continue;
                }

                // crea el objeto pago y lo agrega a los nuevos
                var pago = new Models.Pago(codigoBanco, fecha, nit, valor);
                pagosNuevos.Add(pago);
                pagosExistentes.Add(pago);
                resumen.PagosCreados++;
            }

            // persiste todos los pagos en la base de datos
            _dbService.GuardarPagos(pagosExistentes);

            return pagosNuevos;
        }

        /// <summary>
        /// aplica la lógica de abono: por cada pago nuevo de un cliente, se abona a su factura más antigua.
        /// si el pago es mayor que la factura más antigua, se abona completamente y el excedente va
        /// a la siguiente factura. si sobra después de abonar todas las facturas, queda como saldo a favor.
        /// </summary>
        private void AplicarAbonos(List<Models.Factura> facturasNuevas, List<Models.Pago> pagosNuevos, ResumenTransacciones resumen)
        {
            // agrupa los pagos nuevos por cliente (NIT) para procesarlos por cliente
            var pagosPorCliente = pagosNuevos.GroupBy(p => p.NITcliente)
                                             .ToDictionary(g => g.Key, g => g.ToList());

            // por cada cliente que tiene pagos nuevos
            foreach (var kvp in pagosPorCliente)
            {
                string nitCliente = kvp.Key;
                var pagos = kvp.Value;

                // obtiene todas las facturas del cliente, ordenadas por fecha (más antiguas primero)
                var facturasDelCliente = facturasNuevas.Where(f => f.NITcliente == nitCliente)
                                                      .OrderBy(f => f.Fecha)
                                                      .ToList();

                // suma el total de pagos del cliente
                decimal totalPagos = pagos.Sum(p => p.Valor);

                // suma el total de facturas del cliente
                decimal totalFacturas = facturasDelCliente.Sum(f => f.Valor);

                // si el total de pagos es mayor que el total de facturas, hay saldo a favor
                if (totalPagos > totalFacturas)
                {
                    decimal saldoFavor = totalPagos - totalFacturas;
                    resumen.SaldosAFavor.Add(new SaldoAFavor 
                    { 
                        NIT = nitCliente, 
                        Monto = saldoFavor 
                    });
                }
            }
        }

        /// <summary>
        /// genera el string XML de respuesta con el resumen de transacciones procesadas.
        /// incluye contadores de facturas creadas, duplicadas, con error, igual para pagos y saldos a favor.
        /// </summary>
        private string GenerarXmlRespuesta(ResumenTransacciones resumen)
        {
            string xml = "<?xml version=\"1.0\"?>\n";
            xml += "<transacciones>\n";
            xml += "  <facturas>\n";
            xml += $"    <nuevasFacturas>{resumen.FacturasCreadas}</nuevasFacturas>\n";
            xml += $"    <facturasDuplicadas>{resumen.FacturasDuplicadas}</facturasDuplicadas>\n";
            xml += $"    <facturasConError>{resumen.FacturasConError}</facturasConError>\n";
            xml += "  </facturas>\n";
            xml += "  <pagos>\n";
            xml += $"    <nuevosPagos>{resumen.PagosCreados}</nuevosPagos>\n";
            xml += $"    <pagosDuplicados>{resumen.PagosDuplicados}</pagosDuplicados>\n";
            xml += $"    <pagosConError>{resumen.PagosConError}</pagosConError>\n";
            xml += "  </pagos>\n";
            xml += "</transacciones>";

            return xml;
        }

        private static string ExtraerCoincidencia(string texto, string patron)
        {
            var match = System.Text.RegularExpressions.Regex.Match(
                texto,
                patron,
                System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            return match.Success ? match.Value.Trim() : string.Empty;
        }
    }

    /// <summary>
    /// clase que modela el resumen de operaciones realizadas al procesar transacciones.
    /// almacena contadores y listas de errores, duplicados y saldos a favor.
    /// </summary>
    public class ResumenTransacciones
    {
        public int FacturasCreadas { get; set; }
        public int FacturasDuplicadas { get; set; }
        public int FacturasConError { get; set; }
        public int PagosCreados { get; set; }
        public int PagosDuplicados { get; set; }
        public int PagosConError { get; set; }
        public List<SaldoAFavor> SaldosAFavor { get; set; } = new List<SaldoAFavor>();
        public string Error { get; set; } = string.Empty;
    }

    /// <summary>
    /// clase que modela un saldo a favor de un cliente después de procesar abonos.
    /// contiene el NIT del cliente y el monto disponible como crédito.
    /// </summary>
    public class SaldoAFavor
    {
        public string NIT { get; set; } = string.Empty;
        public decimal Monto { get; set; }
    }
}
