namespace BackendAPI.Models
{
    /// <summary>
    /// clase que modela los bancos disponibles en el sistema. cada banco tiene un código numérico único
    /// que lo identifica y un nombre que permite conocer la institución financiera. esta clase es fundamental
    /// para poder registrar los pagos que realizan los clientes, ya que cada pago debe estar asociado a un banco específico.
    /// </summary>
    public class Banco
    {
        /// <summary>
        /// obtiene o establece el código numérico del banco. es un identificador único que no puede repetirse
        /// en el sistema y funciona como clave primaria. este código es lo que se utiliza para relacionar los pagos
        /// con el banco que los procesó.
        /// </summary>
        public int Codigo { get; set; }

        /// <summary>
        /// obtiene o establece el nombre del banco. es información descriptiva que permite identificar de manera
        /// legible la institución financiera. es útil en reportes y consultas para saber qué banco procesó un pago.
        /// </summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// constructor por defecto de la clase Banco. inicializa las propiedades con valores por defecto
        /// para permitir la deserialización desde XML.
        /// </summary>
        public Banco() { }

        /// <summary>
        /// constructor sobrecargado que inicializa un banco con su código y nombre de forma directa.
        /// es útil cuando estamos registrando nuevos bancos en el sistema.
        /// </summary>
        /// <param name="codigo">el código numérico único del banco</param>
        /// <param name="nombre">el nombre de la institución financiera</param>
        public Banco(int codigo, string nombre)
        {
            Codigo = codigo;
            Nombre = nombre;
        }

        /// <summary>
        /// genera una representación en texto del banco que facilita la depuración y los logs del sistema.
        /// retorna una cadena que muestra tanto el código como el nombre del banco.
        /// </summary>
        /// <returns>una cadena con la información del banco</returns>
        public override string ToString()
        {
            return $"Banco [Código: {Codigo}, Nombre: {Nombre}]";
        }
    }
}
