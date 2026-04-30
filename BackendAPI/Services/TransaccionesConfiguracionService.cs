namespace BackendAPI.Services
{
    /// <summary>
    /// servicio que encapsula la lógica de negocio para procesar la configuración de clientes y bancos.
    /// recibe un string XML, utiliza expresiones regulares para extraer los datos, valida duplicados
    /// y persiste la información usando el XmlDatabaseService. retorna un resumen detallado en formato XML.
    /// </summary>
    public class TransaccionesConfiguracionService
    {
        private readonly XmlDatabaseService _dbService;

        /// <summary>
        /// constructor que recibe la instancia del servicio de base de datos.
        /// </summary>
        /// <param name="dbService">instancia única del XmlDatabaseService</param>
        public TransaccionesConfiguracionService(XmlDatabaseService dbService)
        {
            _dbService = dbService;
        }

        /// <summary>
        /// procesa el XML de entrada que contiene clientes y bancos. extrae los datos usando regex,
        /// valida duplicados y guarda sin repetición. genera un string XML con el resumen de las operaciones.
        /// </summary>
        /// <param name="xmlConfiguracion">string con el contenido XML de configuración</param>
        /// <returns>string XML con el resumen de clientes creados/actualizados y bancos creados/actualizados</returns>
        public string ProcesarConfiguracion(string xmlConfiguracion)
        {
            var resumen = new ResumenConfiguracion();

            try
            {
                // procesa los clientes del XML
                ProcesarClientes(xmlConfiguracion, resumen);

                // procesa los bancos del XML
                ProcesarBancos(xmlConfiguracion, resumen);
            }
            catch (Exception ex)
            {
                resumen.Error = $"error al procesar configuración: {ex.Message}";
            }

            // genera y retorna el XML de respuesta
            return GenerarXmlRespuesta(resumen);
        }

        /// <summary>
        /// extrae y procesa los clientes del XML usando expresiones regulares.
        /// valida que no existan duplicados por NIT y actualiza si es necesario.
        /// </summary>
        private void ProcesarClientes(string xml, ResumenConfiguracion resumen)
        {
            // expresión regular que busca todas las etiquetas <Cliente>...</Cliente>
            // <Cliente>.*?</Cliente> usa:
            // - <Cliente> y </Cliente> como delimitadores literales
            // - .*? para capturar cualquier contenido entre ellos (no-codicioso, se detiene en el primer </Cliente>)
            // - RegexOptions.Singleline permite que . incluya saltos de línea (\n)
            System.Text.RegularExpressions.MatchCollection clientesMatches = 
                System.Text.RegularExpressions.Regex.Matches(
                    xml, 
                    @"<Cliente>.*?</Cliente>", 
                    System.Text.RegularExpressions.RegexOptions.Singleline
                );

            // carga los clientes existentes de la base de datos
            var clientesExistentes = _dbService.CargarClientes();

            // procesa cada cliente encontrado en el XML
            foreach (System.Text.RegularExpressions.Match match in clientesMatches)
            {
                string clienteXml = match.Value;

                // extrae el NIT del cliente usando regex
                // <NIT>(.*?)</NIT> captura todo lo que aparece entre <NIT> y </NIT>
                // el grupo de captura (.*?) se accede con Groups[1]
                // Groups[0] sería el match completo incluyendo las etiquetas
                System.Text.RegularExpressions.Match nitMatch = 
                    System.Text.RegularExpressions.Regex.Match(clienteXml, @"<NIT>(.*?)</NIT>");
                
                if (!nitMatch.Success) continue;
                string nit = nitMatch.Groups[1].Value.Trim();

                // extrae el nombre del cliente usando regex
                // <nombre>(.*?)</nombre> busca el contenido dentro de las etiquetas nombre
                // tiene que ser exactamente con minúsculas según el formato esperado del XML
                System.Text.RegularExpressions.Match nombreMatch = 
                    System.Text.RegularExpressions.Regex.Match(clienteXml, @"<nombre>(.*?)</nombre>");
                
                if (!nombreMatch.Success) continue;
                string nombre = nombreMatch.Groups[1].Value.Trim();

                // valida que ambos campos no estén vacíos
                if (string.IsNullOrEmpty(nit) || string.IsNullOrEmpty(nombre))
                    continue;

                // busca si ya existe un cliente con este NIT
                var clienteExistente = clientesExistentes.FirstOrDefault(c => c.NIT == nit);

                if (clienteExistente != null)
                {
                    // si existe, actualiza el nombre
                    clienteExistente.Nombre = nombre;
                    resumen.ClientesActualizados++;
                }
                else
                {
                    // si no existe, lo agrega como nuevo
                    clientesExistentes.Add(new Models.Cliente(nit, nombre));
                    resumen.ClientesCreados++;
                }
            }

            // persiste todos los clientes en la base de datos
            _dbService.GuardarClientes(clientesExistentes);
        }

        /// <summary>
        /// extrae y procesa los bancos del XML usando expresiones regulares.
        /// valida que no existan duplicados por Código.
        /// </summary>
        private void ProcesarBancos(string xml, ResumenConfiguracion resumen)
        {
            // expresión regular que busca todas las etiquetas <Banco>...</Banco>
            // similar a clientes, captura bloques completos de bancos
            System.Text.RegularExpressions.MatchCollection bancosMatches = 
                System.Text.RegularExpressions.Regex.Matches(
                    xml, 
                    @"<Banco>.*?</Banco>", 
                    System.Text.RegularExpressions.RegexOptions.Singleline
                );

            // carga los bancos existentes de la base de datos
            var bancosExistentes = _dbService.CargarBancos();

            // procesa cada banco encontrado en el XML
            foreach (System.Text.RegularExpressions.Match match in bancosMatches)
            {
                string bancoXml = match.Value;

                // extrae el código del banco usando regex
                // <codigo>(.*?)</codigo> captura el número del banco
                // se intenta parsear a int para validar que sea un número válido
                System.Text.RegularExpressions.Match codigoMatch = 
                    System.Text.RegularExpressions.Regex.Match(bancoXml, @"<codigo>(.*?)</codigo>");
                
                if (!codigoMatch.Success || !int.TryParse(codigoMatch.Groups[1].Value.Trim(), out int codigo))
                    continue;

                // extrae el nombre del banco usando regex
                // <nombre>(.*?)</nombre> captura el nombre o razón social del banco
                System.Text.RegularExpressions.Match nombreMatch = 
                    System.Text.RegularExpressions.Regex.Match(bancoXml, @"<nombre>(.*?)</nombre>");
                
                if (!nombreMatch.Success) continue;
                string nombre = nombreMatch.Groups[1].Value.Trim();

                if (string.IsNullOrEmpty(nombre))
                    continue;

                // busca si ya existe un banco con este código
                var bancoExistente = bancosExistentes.FirstOrDefault(b => b.Codigo == codigo);

                if (bancoExistente != null)
                {
                    // actualiza el nombre del banco existente
                    bancoExistente.Nombre = nombre;
                    resumen.BancosActualizados++;
                }
                else
                {
                    // agrega el banco como nuevo
                    bancosExistentes.Add(new Models.Banco(codigo, nombre));
                    resumen.BancosCreados++;
                }
            }

            // persiste todos los bancos en la base de datos
            _dbService.GuardarBancos(bancosExistentes);
        }

        /// <summary>
        /// genera el string XML de respuesta con el resumen de las operaciones realizadas.
        /// la estructura sigue el formato solicitado con etiquetas de clientes y bancos.
        /// </summary>
        private string GenerarXmlRespuesta(ResumenConfiguracion resumen)
        {
            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n";
            xml += "<respuesta>\n";
            xml += "  <clientes>\n";
            xml += $"    <creados>{resumen.ClientesCreados}</creados>\n";
            xml += $"    <actualizados>{resumen.ClientesActualizados}</actualizados>\n";
            xml += "  </clientes>\n";
            xml += "  <bancos>\n";
            xml += $"    <creados>{resumen.BancosCreados}</creados>\n";
            xml += $"    <actualizados>{resumen.BancosActualizados}</actualizados>\n";
            xml += "  </bancos>\n";

            if (!string.IsNullOrEmpty(resumen.Error))
            {
                xml += $"  <error>{System.Web.HttpUtility.HtmlEncode(resumen.Error)}</error>\n";
            }

            xml += "</respuesta>";

            return xml;
        }
    }

    /// <summary>
    /// clase que modela el resumen de operaciones realizadas al procesar configuración.
    /// almacena contadores de clientes y bancos creados/actualizados.
    /// </summary>
    public class ResumenConfiguracion
    {
        public int ClientesCreados { get; set; }
        public int ClientesActualizados { get; set; }
        public int BancosCreados { get; set; }
        public int BancosActualizados { get; set; }
        public string Error { get; set; } = string.Empty;
    }
}
