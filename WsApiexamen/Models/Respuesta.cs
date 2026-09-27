namespace WsApiexamen.Models
{
    public class Respuesta
    {
        public bool Exito { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public static Respuesta Correcto(string descripcion) => new() { Exito = true, Descripcion = descripcion };
        public static Respuesta Error(string descripcion) => new()
        {
            Exito = false,
            Descripcion = descripcion
        };
     }
}
