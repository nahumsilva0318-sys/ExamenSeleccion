using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace apiexamen
{
    public class clsExamen
    {
        private const int LongitudMaxima = 255;
        private readonly string _cadenaConexion;
        private readonly string _urlWebService;
        private readonly HttpClient _http;
        
        public TipoConexion Modo { get; set; }
        public clsExamen(string cadenaConexion, string urlWebService,
        HttpClient? httpClient = null,
        TipoConexion modo = TipoConexion.ProcedimientosAlmacenados)
        {
            _cadenaConexion = cadenaConexion ?? string.Empty;
            _urlWebService = (urlWebService ?? string.Empty).TrimEnd('/');
            _http = httpClient ?? new HttpClient();
            Modo = modo;
        }
        // ============================ Métodos públicos ============================
        public async Task<Resultado> AgregarExamen(int id, string? nombre, string? descripcion)
        {
            var error = ValidarConfiguracion()
            ?? ValidarId(id)
            ?? ValidarTexto(nombre, "Nombre", obligatorio: true)
            ?? ValidarTexto(descripcion, "Descripción", obligatorio: false);
            if (error is not null) return Resultado.Error(error, 1);
            nombre = Limpiar(nombre);
            descripcion = Limpiar(descripcion);
            return Modo == TipoConexion.WebService
            ? await LlamarWebService(HttpMethod.Post, "Agregar",
            new Examen { Id = id, Nombre = nombre, Descripcion = descripcion })
            : await EjecutarSpTransaccional("dbo.spAgregar",
            p => ParametrosExamen(p, id, nombre, descripcion));
        }
        public async Task<Resultado> ActualizarExamen(int id, string? nombre, string? descripcion)
        {
            var error = ValidarConfiguracion()
            ?? ValidarId(id)
            ?? ValidarTexto(nombre, "Nombre", obligatorio: true)
            ?? ValidarTexto(descripcion, "Descripción", obligatorio: false);
            if (error is not null) return Resultado.Error(error, 1);
            nombre = Limpiar(nombre);
            descripcion = Limpiar(descripcion);
            return Modo == TipoConexion.WebService
            ? await LlamarWebService(HttpMethod.Put, "Actualizar",
            new Examen { Id = id, Nombre = nombre, Descripcion = descripcion })
            : await EjecutarSpTransaccional("dbo.spActualizar",
            p => ParametrosExamen(p, id, nombre, descripcion));
        }
        public async Task<Resultado> EliminarExamen(int id)
        {
            var error = ValidarConfiguracion() ?? ValidarId(id);
            if (error is not null) return Resultado.Error(error, 1);
            return Modo == TipoConexion.WebService
            ? await LlamarWebService(HttpMethod.Delete, $"Eliminar/{id}", null)
            : await EjecutarSpTransaccional("dbo.spEliminar",
            p => p.Add("@Id", SqlDbType.Int).Value = id);
        }
        public async Task<ResultadoConsulta> ConsultarExamen(int? id, string? nombre, string? descripcion)
        {
            var error = ValidarConfiguracion()
            ?? (id.HasValue ? ValidarId(id.Value) : null)
            ?? ValidarTexto(nombre, "Nombre", obligatorio: false)
            ?? ValidarTexto(descripcion, "Descripción", obligatorio: false);
            if (error is not null) return new ResultadoConsulta { Exito = false, Codigo = 1, Descripcion = error };
            nombre = Limpiar(nombre);
            descripcion = Limpiar(descripcion);
            return Modo == TipoConexion.WebService
            ? await ConsultarPorWebService(id, nombre, descripcion) : await ConsultarPorSp(id, nombre, descripcion);
        }

        // ============================ Procedimientos Almacenados ============================
        private async Task<Resultado> EjecutarSpTransaccional(string nombreSp, Action<SqlParameterCollection> agregarParametros)
        {
            try
            {
                await using var conexion = new SqlConnection(_cadenaConexion);
                await conexion.OpenAsync();
                // Transacción ADO.NET: si el SP devuelve error se hace Rollback
                using var transaccion = conexion.BeginTransaction();
                try
                {
                    int codigo;
                    string descripcion;
                    await using (var comando = new SqlCommand(nombreSp, conexion, transaccion))
                    {
                        comando.CommandType = CommandType.StoredProcedure;
                        agregarParametros(comando.Parameters);
                        await using var lector = await comando.ExecuteReaderAsync();
                        if (!await lector.ReadAsync())
                            throw new InvalidOperationException($"El procedimiento {nombreSp} no devolvió resultado.");
                        codigo = Convert.ToInt32(lector["CodigoRetorno"]);
                        descripcion = Convert.ToString(lector["DescripcionRetorno"]) ?? string.Empty;
                    }
                    if (codigo == 0)
                    {
                        await transaccion.CommitAsync();
                        return Resultado.Correcto(descripcion);
                    }
                    await transaccion.RollbackAsync();
                    return Resultado.Error(descripcion, codigo);
                }
                catch
                {
                    try { await transaccion.RollbackAsync(); } catch { /* ya estaba revertida */ }
                    throw;
                }
            }
            catch (SqlException ex)
            {
                return Resultado.Error("Error de base de datos: " + ex.Message, ex.Number);
            }
            catch (Exception ex)
            {
                return Resultado.Error("Error inesperado: " + ex.Message);
            }
        }

        private async Task<ResultadoConsulta> ConsultarPorSp(int? id, string? nombre, string? descripcion)
        {
            try
            {
                var lista = new List<Examen>();
                await using var conexion = new SqlConnection(_cadenaConexion);
                await using var comando = new SqlCommand("dbo.spConsultar", conexion)
                {
                    CommandType = CommandType.StoredProcedure
                };
                comando.Parameters.Add("@Nombre", SqlDbType.VarChar, LongitudMaxima).Value = (object?)nombre ?? DBNull.Value;
                comando.Parameters.Add("@Descripcion", SqlDbType.VarChar, LongitudMaxima).Value = (object?)descripcion ?? DBNull.Value;
                await conexion.OpenAsync();
                await using var lector = await comando.ExecuteReaderAsync();
                while (await lector.ReadAsync())
                {
                    lista.Add(new Examen
                    {
                        Id = lector.GetInt32(0),
                        Nombre = lector.IsDBNull(1) ? null : lector.GetString(1),
                        Descripcion = lector.IsDBNull(2) ? null : lector.GetString(2)
                    });

                }
                
                // para filtrar por id, el sp no lo contiene, se filtra de la lista por el valor del id
                if (id.HasValue)
                    lista = lista.Where(e => e.Id == id.Value).ToList();
                return new ResultadoConsulta
                {
                    Exito = true,
                    Descripcion = $"Consulta realizada: {lista.Count} registro(s) encontrado(s).",
                    Datos = lista
                };
            }
            catch (Exception ex)
            {
                return new ResultadoConsulta { Exito = false, Codigo = -1, Descripcion = "Error al consultar: " + ex.Message };
            }


        }

        private static void ParametrosExamen(SqlParameterCollection p, int id, string? nombre, string? descripcion)
        {
            p.Add("@Id", SqlDbType.Int).Value = id;
            p.Add("@Nombre", SqlDbType.VarChar, LongitudMaxima).Value = (object?)nombre ?? DBNull.Value;
            p.Add("@Descripcion", SqlDbType.VarChar, LongitudMaxima).Value = (object?)descripcion ?? DBNull.Value;
        }

        // ============================ WebSrvice ============================
        private async Task<Resultado> LlamarWebService(HttpMethod metodo, string accion, Examen? cuerpo)
        {
            try
            {
                using var peticion = new HttpRequestMessage(metodo, $"{_urlWebService}/api/Examen/{accion}");
                if (cuerpo is not null)
                    peticion.Content = JsonContent.Create(cuerpo);
                using var respuesta = await _http.SendAsync(peticion);
                return await LeerResultado(respuesta);
            }
            catch (HttpRequestException ex)
            {
                return Resultado.Error($"No fue posible comunicarse con el Web Service ({_urlWebService}): {ex.Message}", -2);
            }
            catch (Exception ex)
            {
                return Resultado.Error("Error al llamar al Web Service: " + ex.Message);
            }
        }
        private async Task<ResultadoConsulta> ConsultarPorWebService(int? id, string? nombre, string? descripcion)
        {
            try
            {
                var filtros = new List<string>();
                if (id.HasValue) filtros.Add($"id={id.Value}");
                if (nombre is not null) filtros.Add($"nombre={Uri.EscapeDataString(nombre)}");
                if (descripcion is not null) filtros.Add($"descripcion={Uri.EscapeDataString(descripcion)}");
                var url = $"{_urlWebService}/api/Examen/Consultar";
                if (filtros.Count > 0) url += "?" + string.Join("&", filtros);
                using var respuesta = await _http.GetAsync(url);
                if (respuesta.IsSuccessStatusCode)
                {
                    var datos = await respuesta.Content.ReadFromJsonAsync<List<Examen>>() ?? new List<Examen>();
                    return new ResultadoConsulta
                    {
                        Exito = true,
                        Descripcion = $"Consulta realizada: {datos.Count} registro(s) encontrado(s).",
                        Datos = datos
                    };
                }
                var error = await LeerResultado(respuesta);
                return new ResultadoConsulta { Exito = false, Codigo = error.Codigo, Descripcion = error.Descripcion };
            }
            catch (HttpRequestException ex)
            {
                return new ResultadoConsulta
                {

                    Exito = false,
                    Codigo = -2,
                    Descripcion = $"No fue posible comunicarse con el Web Service ({_urlWebService}): {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                return new ResultadoConsulta { Exito = false, Codigo = -1, Descripcion = "Error al consultar: " + ex.Message };
            }
        }

        private static async Task<Resultado> LeerResultado(HttpResponseMessage respuesta)
        {
            try
            {
                var r = await respuesta.Content.ReadFromJsonAsync<Resultado>();
                if (r is not null && !string.IsNullOrEmpty(r.Descripcion))
                {
                    if (!r.Exito && r.Codigo == 0) r.Codigo = (int)respuesta.StatusCode;
                    return r;
                }
            }
            catch (JsonException) { } // en caso de que la respuesta no sea un JSON
            catch (NotSupportedException) { } //content-type no soportado
            return respuesta.IsSuccessStatusCode
            ? Resultado.Correcto("Operación realizada.")
            : Resultado.Error($"El Web Service respondió {(int)respuesta.StatusCode} ({respuesta.ReasonPhrase}).",
            (int)respuesta.StatusCode);
        }

        // ============================ Validaciones ============================
        private string? ValidarConfiguracion()
        {
            if (Modo == TipoConexion.ProcedimientosAlmacenados && string.IsNullOrWhiteSpace(_cadenaConexion))
                return "No se configuró la cadena de conexión a la base de datos.";
            if (Modo == TipoConexion.WebService && !Uri.TryCreate(_urlWebService, UriKind.Absolute, out _))
                return "No se configuró una URL válida para el Web Service.";
            return null;
        }
        private static string? ValidarId(int id) =>
        id <= 0 ? "El Id es obligatorio y debe ser un número entero mayor a cero." : null;
        private static string? ValidarTexto(string? valor, string campo, bool obligatorio)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return obligatorio ? $"El campo {campo} es obligatorio." : null;
            if (valor.Trim().Length > LongitudMaxima)
                return $"El campo {campo} no puede exceder {LongitudMaxima} caracteres.";
            return null;
        }
        private static string? Limpiar(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();


    }
}
