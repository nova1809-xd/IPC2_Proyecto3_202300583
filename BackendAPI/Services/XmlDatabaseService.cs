using System.Text.RegularExpressions;

namespace BackendAPI.Services
{
    /// <summary>
    /// servicio singleton que se encarga de toda la persistencia de datos en archivos XML.
    /// este servicio simula una base de datos mediante la lectura y escritura de archivos XML locales.
    /// utiliza expresiones regulares (regex) para parsear el contenido XML sin usar librerías nativas
    /// de serialización XML como XmlDocument o LINQ to XML, tal como lo especifica el proyecto.
    /// el singleton garantiza que solo existe una única instancia de este servicio en toda la aplicación,
    /// evitando problemas de concurrencia al acceder a los archivos XML.
    /// </summary>
    public class XmlDatabaseService
    {
        // instancia privada estática del singleton
        private static XmlDatabaseService? _instance;

        // objeto de sincronización para evitar condiciones de carrera en multihilo
        private static readonly object _lockObject = new object();

        // ruta base donde se almacenan todos los archivos XML de la base de datos
        private readonly string _basePath;

        // rutas de los archivos XML para cada entidad
        private readonly string _clientesPath;
        private readonly string _bancosPath;
        private readonly string _facturasPath;
        private readonly string _pagosPath;

        /// <summary>
        /// constructor privado del singleton. inicializa las rutas de los archivos XML en la carpeta del proyecto.
        /// el uso de constructor privado garantiza que solo esta clase puede crear instancias de sí misma.
        /// </summary>
        private XmlDatabaseService()
        {
            // obtiene la ruta base del directorio de la aplicación
            _basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database");

            // si la carpeta Database no existe, la crea
            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
            }

            // define las rutas específicas de cada archivo XML
            _clientesPath = Path.Combine(_basePath, "db_clientes.xml");
            _bancosPath = Path.Combine(_basePath, "db_bancos.xml");
            _facturasPath = Path.Combine(_basePath, "db_facturas.xml");
            _pagosPath = Path.Combine(_basePath, "db_pagos.xml");

            // inicializa los archivos XML si no existen
            InicializarArchivosXml();
        }

        /// <summary>
        /// obtiene la instancia única del singleton. implementa el patrón singleton con sincronización
        /// para garantizar que el getInstance se ejecute de manera thread-safe incluso en aplicaciones multihilo.
        /// </summary>
        /// <returns>la instancia única del servicio XmlDatabaseService</returns>
        public static XmlDatabaseService GetInstance()
        {
            if (_instance == null)
            {
                lock (_lockObject)
                {
                    if (_instance == null)
                    {
                        _instance = new XmlDatabaseService();
                    }
                }
            }
            return _instance;
        }

        /// <summary>
        /// inicializa los archivos XML creando las estructuras básicas si no existen.
        /// cada archivo tiene su raíz XML correspondiente para la entidad que almacena.
        /// </summary>
        private void InicializarArchivosXml()
        {
            // inicializa el archivo de clientes si no existe
            if (!File.Exists(_clientesPath))
            {
                File.WriteAllText(_clientesPath, "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<Clientes>\n</Clientes>");
            }

            // inicializa el archivo de bancos si no existe
            if (!File.Exists(_bancosPath))
            {
                File.WriteAllText(_bancosPath, "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<Bancos>\n</Bancos>");
            }

            // inicializa el archivo de facturas si no existe
            if (!File.Exists(_facturasPath))
            {
                File.WriteAllText(_facturasPath, "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<Facturas>\n</Facturas>");
            }

            // inicializa el archivo de pagos si no existe
            if (!File.Exists(_pagosPath))
            {
                File.WriteAllText(_pagosPath, "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<Pagos>\n</Pagos>");
            }
        }

        /// <summary>
        /// guarda una lista de clientes en el archivo XML db_clientes.xml. utiliza expresiones regulares
        /// para generar el contenido XML de manera manual sin depender de librerías de serialización.
        /// </summary>
        /// <param name="clientes">lista de objetos Cliente a guardar</param>
        public void GuardarClientes(List<Models.Cliente> clientes)
        {
            try
            {
                // construye el contenido XML manualmente
                string contenido = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<Clientes>\n";

                // itera sobre cada cliente para agregar su elemento XML
                foreach (var cliente in clientes)
                {
                    contenido += $"  <Cliente>\n" +
                                 $"    <NIT>{EscaparXml(cliente.NIT)}</NIT>\n" +
                                 $"    <Nombre>{EscaparXml(cliente.Nombre)}</Nombre>\n" +
                                 $"  </Cliente>\n";
                }

                contenido += "</Clientes>";

                // escribe el contenido en el archivo
                File.WriteAllText(_clientesPath, contenido);
            }
            catch (Exception ex)
            {
                throw new Exception($"error al guardar clientes en XML: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// carga la lista de clientes desde el archivo XML db_clientes.xml. utiliza expresiones regulares
        /// para extraer los datos del XML de manera manual.
        /// </summary>
        /// <returns>lista de objetos Cliente cargados desde el archivo XML</returns>
        public List<Models.Cliente> CargarClientes()
        {
            try
            {
                var clientes = new List<Models.Cliente>();

                // verifica que el archivo exista
                if (!File.Exists(_clientesPath))
                {
                    return clientes;
                }

                // lee el contenido del archivo
                string contenido = File.ReadAllText(_clientesPath);

                // usa regex para encontrar todos los elementos Cliente
                MatchCollection matches = Regex.Matches(contenido, @"<Cliente>.*?</Cliente>", RegexOptions.Singleline);

                foreach (Match match in matches)
                {
                    string clienteXml = match.Value;

                    // extrae el NIT usando regex
                    Match nitMatch = Regex.Match(clienteXml, @"<NIT>(.*?)</NIT>");
                    string nit = nitMatch.Success ? DesescaparXml(nitMatch.Groups[1].Value) : string.Empty;

                    // extrae el Nombre usando regex
                    Match nombreMatch = Regex.Match(clienteXml, @"<Nombre>(.*?)</Nombre>");
                    string nombre = nombreMatch.Success ? DesescaparXml(nombreMatch.Groups[1].Value) : string.Empty;

                    // crea el objeto Cliente y lo agrega a la lista
                    if (!string.IsNullOrEmpty(nit))
                    {
                        clientes.Add(new Models.Cliente(nit, nombre));
                    }
                }

                return clientes;
            }
            catch (Exception ex)
            {
                throw new Exception($"error al cargar clientes desde XML: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// guarda una lista de bancos en el archivo XML db_bancos.xml. utiliza expresiones regulares
        /// para generar el contenido XML de manera manual.
        /// </summary>
        /// <param name="bancos">lista de objetos Banco a guardar</param>
        public void GuardarBancos(List<Models.Banco> bancos)
        {
            try
            {
                // construye el contenido XML manualmente
                string contenido = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<Bancos>\n";

                // itera sobre cada banco para agregar su elemento XML
                foreach (var banco in bancos)
                {
                    contenido += $"  <Banco>\n" +
                                 $"    <Codigo>{banco.Codigo}</Codigo>\n" +
                                 $"    <Nombre>{EscaparXml(banco.Nombre)}</Nombre>\n" +
                                 $"  </Banco>\n";
                }

                contenido += "</Bancos>";

                // escribe el contenido en el archivo
                File.WriteAllText(_bancosPath, contenido);
            }
            catch (Exception ex)
            {
                throw new Exception($"error al guardar bancos en XML: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// carga la lista de bancos desde el archivo XML db_bancos.xml. utiliza expresiones regulares
        /// para extraer los datos del XML de manera manual.
        /// </summary>
        /// <returns>lista de objetos Banco cargados desde el archivo XML</returns>
        public List<Models.Banco> CargarBancos()
        {
            try
            {
                var bancos = new List<Models.Banco>();

                // verifica que el archivo exista
                if (!File.Exists(_bancosPath))
                {
                    return bancos;
                }

                // lee el contenido del archivo
                string contenido = File.ReadAllText(_bancosPath);

                // usa regex para encontrar todos los elementos Banco
                MatchCollection matches = Regex.Matches(contenido, @"<Banco>.*?</Banco>", RegexOptions.Singleline);

                foreach (Match match in matches)
                {
                    string bancoXml = match.Value;

                    // extrae el Código usando regex
                    Match codigoMatch = Regex.Match(bancoXml, @"<Codigo>(.*?)</Codigo>");
                    if (!codigoMatch.Success || !int.TryParse(codigoMatch.Groups[1].Value, out int codigo))
                    {
                        continue;
                    }

                    // extrae el Nombre usando regex
                    Match nombreMatch = Regex.Match(bancoXml, @"<Nombre>(.*?)</Nombre>");
                    string nombre = nombreMatch.Success ? DesescaparXml(nombreMatch.Groups[1].Value) : string.Empty;

                    // crea el objeto Banco y lo agrega a la lista
                    bancos.Add(new Models.Banco(codigo, nombre));
                }

                return bancos;
            }
            catch (Exception ex)
            {
                throw new Exception($"error al cargar bancos desde XML: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// guarda una lista de facturas en el archivo XML db_facturas.xml. utiliza expresiones regulares
        /// para generar el contenido XML de manera manual.
        /// </summary>
        /// <param name="facturas">lista de objetos Factura a guardar</param>
        public void GuardarFacturas(List<Models.Factura> facturas)
        {
            try
            {
                // construye el contenido XML manualmente
                string contenido = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<Facturas>\n";

                // itera sobre cada factura para agregar su elemento XML
                foreach (var factura in facturas)
                {
                    contenido += $"  <Factura>\n" +
                                 $"    <NumeroFactura>{EscaparXml(factura.NumeroFactura)}</NumeroFactura>\n" +
                                 $"    <NITcliente>{EscaparXml(factura.NITcliente)}</NITcliente>\n" +
                                 $"    <Fecha>{factura.Fecha:yyyy-MM-dd HH:mm:ss}</Fecha>\n" +
                                 $"    <Valor>{factura.Valor}</Valor>\n" +
                                 $"  </Factura>\n";
                }

                contenido += "</Facturas>";

                // escribe el contenido en el archivo
                File.WriteAllText(_facturasPath, contenido);
            }
            catch (Exception ex)
            {
                throw new Exception($"error al guardar facturas en XML: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// carga la lista de facturas desde el archivo XML db_facturas.xml. utiliza expresiones regulares
        /// para extraer los datos del XML de manera manual.
        /// </summary>
        /// <returns>lista de objetos Factura cargados desde el archivo XML</returns>
        public List<Models.Factura> CargarFacturas()
        {
            try
            {
                var facturas = new List<Models.Factura>();

                // verifica que el archivo exista
                if (!File.Exists(_facturasPath))
                {
                    return facturas;
                }

                // lee el contenido del archivo
                string contenido = File.ReadAllText(_facturasPath);

                // usa regex para encontrar todos los elementos Factura
                MatchCollection matches = Regex.Matches(contenido, @"<Factura>.*?</Factura>", RegexOptions.Singleline);

                foreach (Match match in matches)
                {
                    string facturaXml = match.Value;

                    // extrae el NumeroFactura usando regex
                    Match numeroMatch = Regex.Match(facturaXml, @"<NumeroFactura>(.*?)</NumeroFactura>");
                    string numero = numeroMatch.Success ? DesescaparXml(numeroMatch.Groups[1].Value) : string.Empty;

                    // extrae el NITcliente usando regex
                    Match nitMatch = Regex.Match(facturaXml, @"<NITcliente>(.*?)</NITcliente>");
                    string nit = nitMatch.Success ? DesescaparXml(nitMatch.Groups[1].Value) : string.Empty;

                    // extrae la Fecha usando regex
                    Match fechaMatch = Regex.Match(facturaXml, @"<Fecha>(.*?)</Fecha>");
                    DateTime fecha = DateTime.MinValue;
                    if (fechaMatch.Success && DateTime.TryParse(fechaMatch.Groups[1].Value, out DateTime fechaParsed))
                    {
                        fecha = fechaParsed;
                    }

                    // extrae el Valor usando regex
                    Match valorMatch = Regex.Match(facturaXml, @"<Valor>(.*?)</Valor>");
                    decimal valor = 0;
                    if (valorMatch.Success && decimal.TryParse(valorMatch.Groups[1].Value, out decimal valorParsed))
                    {
                        valor = valorParsed;
                    }

                    // crea el objeto Factura y lo agrega a la lista
                    if (!string.IsNullOrEmpty(numero) && !string.IsNullOrEmpty(nit))
                    {
                        facturas.Add(new Models.Factura(numero, nit, fecha, valor));
                    }
                }

                return facturas;
            }
            catch (Exception ex)
            {
                throw new Exception($"error al cargar facturas desde XML: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// guarda una lista de pagos en el archivo XML db_pagos.xml. utiliza expresiones regulares
        /// para generar el contenido XML de manera manual.
        /// </summary>
        /// <param name="pagos">lista de objetos Pago a guardar</param>
        public void GuardarPagos(List<Models.Pago> pagos)
        {
            try
            {
                // construye el contenido XML manualmente
                string contenido = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<Pagos>\n";

                // itera sobre cada pago para agregar su elemento XML
                foreach (var pago in pagos)
                {
                    contenido += $"  <Pago>\n" +
                                 $"    <CodigoBanco>{pago.CodigoBanco}</CodigoBanco>\n" +
                                 $"    <Fecha>{pago.Fecha:yyyy-MM-dd HH:mm:ss}</Fecha>\n" +
                                 $"    <NITcliente>{EscaparXml(pago.NITcliente)}</NITcliente>\n" +
                                 $"    <Valor>{pago.Valor}</Valor>\n" +
                                 $"  </Pago>\n";
                }

                contenido += "</Pagos>";

                // escribe el contenido en el archivo
                File.WriteAllText(_pagosPath, contenido);
            }
            catch (Exception ex)
            {
                throw new Exception($"error al guardar pagos en XML: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// carga la lista de pagos desde el archivo XML db_pagos.xml. utiliza expresiones regulares
        /// para extraer los datos del XML de manera manual.
        /// </summary>
        /// <returns>lista de objetos Pago cargados desde el archivo XML</returns>
        public List<Models.Pago> CargarPagos()
        {
            try
            {
                var pagos = new List<Models.Pago>();

                // verifica que el archivo exista
                if (!File.Exists(_pagosPath))
                {
                    return pagos;
                }

                // lee el contenido del archivo
                string contenido = File.ReadAllText(_pagosPath);

                // usa regex para encontrar todos los elementos Pago
                MatchCollection matches = Regex.Matches(contenido, @"<Pago>.*?</Pago>", RegexOptions.Singleline);

                foreach (Match match in matches)
                {
                    string pagoXml = match.Value;

                    // extrae el CodigoBanco usando regex
                    Match codigoMatch = Regex.Match(pagoXml, @"<CodigoBanco>(.*?)</CodigoBanco>");
                    if (!codigoMatch.Success || !int.TryParse(codigoMatch.Groups[1].Value, out int codigo))
                    {
                        continue;
                    }

                    // extrae la Fecha usando regex
                    Match fechaMatch = Regex.Match(pagoXml, @"<Fecha>(.*?)</Fecha>");
                    DateTime fecha = DateTime.MinValue;
                    if (fechaMatch.Success && DateTime.TryParse(fechaMatch.Groups[1].Value, out DateTime fechaParsed))
                    {
                        fecha = fechaParsed;
                    }

                    // extrae el NITcliente usando regex
                    Match nitMatch = Regex.Match(pagoXml, @"<NITcliente>(.*?)</NITcliente>");
                    string nit = nitMatch.Success ? DesescaparXml(nitMatch.Groups[1].Value) : string.Empty;

                    // extrae el Valor usando regex
                    Match valorMatch = Regex.Match(pagoXml, @"<Valor>(.*?)</Valor>");
                    decimal valor = 0;
                    if (valorMatch.Success && decimal.TryParse(valorMatch.Groups[1].Value, out decimal valorParsed))
                    {
                        valor = valorParsed;
                    }

                    // crea el objeto Pago y lo agrega a la lista
                    if (!string.IsNullOrEmpty(nit))
                    {
                        pagos.Add(new Models.Pago(codigo, fecha, nit, valor));
                    }
                }

                return pagos;
            }
            catch (Exception ex)
            {
                throw new Exception($"error al cargar pagos desde XML: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// escapa caracteres especiales de XML para evitar problemas con la estructura XML.
        /// convierte caracteres como &, <, >, ", ' a sus correspondientes entidades XML.
        /// </summary>
        /// <param name="texto">el texto a escapar</param>
        /// <returns>el texto escapado con entidades XML</returns>
        private string EscaparXml(string texto)
        {
            if (string.IsNullOrEmpty(texto))
            {
                return string.Empty;
            }

            // reemplaza caracteres especiales con sus entidades XML
            return texto
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&apos;");
        }

        /// <summary>
        /// desescapa caracteres especiales de XML para poder restaurar el texto original.
        /// convierte las entidades XML de regreso a sus caracteres correspondientes.
        /// </summary>
        /// <param name="texto">el texto con entidades XML a desescapar</param>
        /// <returns>el texto desescapado</returns>
        private string DesescaparXml(string texto)
        {
            if (string.IsNullOrEmpty(texto))
            {
                return string.Empty;
            }

            // reemplaza las entidades XML por sus caracteres originales
            return texto
                .Replace("&apos;", "'")
                .Replace("&quot;", "\"")
                .Replace("&gt;", ">")
                .Replace("&lt;", "<")
                .Replace("&amp;", "&");
        }
    }
}
