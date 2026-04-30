namespace BackendAPI.Models
{
    /// <summary>
    /// clase que representa una factura en el sistema de facturación y pagos. una factura es el documento
    /// que registra una transacción comercial entre la empresa y un cliente. contiene información crucial
    /// como el número de identificación, el cliente al que se le factura, la fecha del documento y el valor
    /// monetario de la operación.
    /// </summary>
    public class Factura
    {
        /// <summary>
        /// obtiene o establece el número de factura. es un identificador único que no puede repetirse
        /// en el sistema y sirve como clave primaria. cada factura debe tener un número diferente para poder
        /// ser identificada de manera única en el contexto del negocio.
        /// </summary>
        public string NumeroFactura { get; set; } = string.Empty;

        /// <summary>
        /// obtiene o establece el NIT del cliente al cual se le emite la factura. este campo establece una relación
        /// con la clase Cliente y permite saber a quién corresponde cada factura. es fundamental para poder hacer
        /// seguimiento de las transacciones de un cliente específico.
        /// </summary>
        public string NITcliente { get; set; } = string.Empty;

        /// <summary>
        /// obtiene o establece la fecha en que se emitió la factura. esta información es importante para auditoría,
        /// reportes financieros y para hacer seguimiento cronológico de las transacciones del negocio.
        /// se almacena como DateTime para permitir operaciones de búsqueda y filtrado por rangos de fechas.
        /// </summary>
        public DateTime Fecha { get; set; }

        /// <summary>
        /// obtiene o establece el valor total de la factura. es el monto monetario que el cliente debe pagar
        /// por los servicios o productos facturados. se utiliza para hacer comparaciones con los pagos realizados
        /// y determinar si la factura está totalmente saldada o tiene pendiente.
        /// </summary>
        public decimal Valor { get; set; }

        /// <summary>
        /// constructor por defecto de la clase Factura. inicializa las propiedades con valores por defecto
        /// para permitir la deserialización desde archivos XML.
        /// </summary>
        public Factura() { }

        /// <summary>
        /// constructor sobrecargado que inicializa una factura con todos los parámetros requeridos de una sola vez.
        /// es conveniente cuando se está creando una nueva factura desde el código de negocio.
        /// </summary>
        /// <param name="numeroFactura">el número identificador único de la factura</param>
        /// <param name="nitcliente">el NIT del cliente a quien va dirigida la factura</param>
        /// <param name="fecha">la fecha de emisión de la factura</param>
        /// <param name="valor">el monto total que el cliente debe pagar</param>
        public Factura(string numeroFactura, string nitcliente, DateTime fecha, decimal valor)
        {
            NumeroFactura = numeroFactura;
            NITcliente = nitcliente;
            Fecha = fecha;
            Valor = valor;
        }

        /// <summary>
        /// genera una representación en texto de la factura que facilita la depuración y los logs.
        /// retorna una cadena con los datos principales de la factura.
        /// </summary>
        /// <returns>una cadena con la información de la factura</returns>
        public override string ToString()
        {
            return $"Factura [Número: {NumeroFactura}, NIT Cliente: {NITcliente}, Fecha: {Fecha:dd/MM/yyyy}, Valor: {Valor:C}]";
        }
    }
}
