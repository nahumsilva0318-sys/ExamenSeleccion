using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apiexamen
{
    // Indica como se va a guardar la información
    public enum TipoConexion
    {
        ProcedimientosAlmacenados = 1, // directo a los SPs
        WebService = 2 // Llama a WsApiexamen
    }
    public class Examen
    {
        public int Id { get; set; }
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
    }
   
    public class Resultado
    {
        public bool Exito { get; set; }
        public int Codigo { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public static Resultado Correcto(string descripcion) =>
        new() { Exito = true, Codigo = 0, Descripcion = descripcion };
        public static Resultado Error(string descripcion, int codigo = -1) =>
        new() { Exito = false, Codigo = codigo, Descripcion = descripcion };
    }
    
    public class ResultadoConsulta : Resultado
    {
        public List<Examen> Datos { get; set; } = new();
    }

}
