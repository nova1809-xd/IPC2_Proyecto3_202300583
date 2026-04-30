namespace BackendAPI.Models
{
    /// <summary>
    /// clase que modela un pago realizado por un cliente en el sistema de facturación.
    /// cada pago representa una transacción que reduce el saldo pendiente de un cliente.
    /// la clase almacena información del banco que procesó la transacción, la fecha, el cliente
    /// y el monto del pago realizado.
    /// </summary>
    public class Pago
    {
        /// <summary>
        /// obtiene o establece el código del banco a través del cual se realizó el pago.
        /// es una llave foránea que hace referencia a la clase Banco. permite identificar
        /// qué institución financiera procesó la transacción.
        /// </summary>
        public int CodigoBanco { get; set; }

        /// <summary>
        /// obtiene o establece la fecha en que se realizó el pago. es importante para auditoría,
        /// para poder hacer reconciliación de pagos y para determinar el orden cronológico
        /// de las transacciones de un cliente.
        /// </summary>
        public DateTime Fecha { get; set; }

        /// <summary>
        /// obtiene o establece el NIT del cliente que realizó el pago. es una llave foránea que
        /// hace referencia a la clase Cliente. permite saber a qué cliente corresponde cada pago
        /// y es fundamental para actualizar el saldo pendiente del cliente.
        /// </summary>
        public string NITcliente { get; set; } = string.Empty;

        /// <summary>
        /// obtiene o establece el monto del pago realizado. es la cantidad de dinero que el cliente
        /// pagó en esta transacción específica. este valor se compara con el valor de las facturas
        /// para determinar si existe un saldo pendiente o un exceso de pago.
        /// </summary>
        public decimal Valor { get; set; }

        /// <summary>
        /// constructor por defecto de la clase Pago. inicializa las propiedades con valores por defecto
        /// para permitir la deserialización desde archivos XML.
        /// </summary>
        public Pago() { }

        /// <summary>
        /// constructor sobrecargado que inicializa un pago con todos sus parámetros requeridos.
        /// es práctico cuando se estamos registrando un nuevo pago en el sistema.
        /// </summary>
        /// <param name="codigoBanco">el código del banco que procesó el pago</param>
        /// <param name="fecha">la fecha en que se realizó el pago</param>
        /// <param name="nitcliente">el NIT del cliente que realizó el pago</param>
        /// <param name="valor">el monto del pago realizado</param>
        public Pago(int codigoBanco, DateTime fecha, string nitcliente, decimal valor)
        {
            CodigoBanco = codigoBanco;
            Fecha = fecha;
            NITcliente = nitcliente;
            Valor = valor;
        }

        /// <summary>
        /// genera una representación en texto del pago que facilita la depuración y los logs del sistema.
        /// retorna una cadena con los datos principales del pago.
        /// </summary>
        /// <returns>una cadena con la información del pago</returns>
        public override string ToString()
        {
            return $"Pago [Banco: {CodigoBanco}, Fecha: {Fecha:dd/MM/yyyy}, NIT Cliente: {NITcliente}, Valor: {Valor:C}]";
        }
    }
}
