namespace BackendAPI.Models
{
    /// <summary>
    /// clase que representa un cliente en el sistema de facturación. cada cliente es identificado
    /// de manera única por su NIT (número de identificación tributaria), que es alfanumérico y funciona
    /// como la clave primaria. el cliente tiene un nombre que es información fundamental para poder identificarlo
    /// en los reportes y en las facturas generadas.
    /// </summary>
    public class Cliente
    {
        /// <summary>
        /// obtiene o establece el NIT del cliente. es un identificador alfanumérico único que cumple la función
        /// de clave primaria para este cliente. es obligatorio y no puede cambiar durante la vida útil del cliente
        /// en el sistema.
        /// </summary>
        public string NIT { get; set; } = string.Empty;

        /// <summary>
        /// obtiene o establece el nombre del cliente. es la información básica que identifica de manera legible
        /// al cliente en el sistema. se utiliza principalmente en los reportes y en las facturas para dar claridad
        /// sobre quién es el receptor de los servicios o productos.
        /// </summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// constructor por defecto de la clase Cliente. inicializa las propiedades con valores vacíos
        /// para permitir la deserialización desde XML y la creación de instancias mediante reflection.
        /// </summary>
        public Cliente() { }

        /// <summary>
        /// constructor sobrecargado que inicializa un cliente con su NIT y nombre de manera directa.
        /// es útil cuando se están creando nuevos clientes desde el código del negocio.
        /// </summary>
        /// <param name="nit">el identificador tributario del cliente</param>
        /// <param name="nombre">el nombre o razón social del cliente</param>
        public Cliente(string nit, string nombre)
        {
            NIT = nit;
            Nombre = nombre;
        }

        /// <summary>
        /// genera una representación en texto del cliente que es útil para depuración y logging.
        /// retorna una cadena con el NIT y el nombre para poder identificar al cliente de manera rápida.
        /// </summary>
        /// <returns>una cadena que contiene la información básica del cliente</returns>
        public override string ToString()
        {
            return $"Cliente [NIT: {NIT}, Nombre: {Nombre}]";
        }
    }
}
