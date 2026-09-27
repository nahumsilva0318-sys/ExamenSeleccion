using Microsoft.EntityFrameworkCore;
using System.Data;
using WsApiexamen.Data;
using WsApiexamen.Models;

namespace WsApiexamen.Services
{
    public class ExamenService
    {
        private const int LongitudMaxima = 255;
        private readonly BdiExamenContext _db;

        public ExamenService(BdiExamenContext db) => _db = db;

        public async Task<Respuesta> AgregarExamen(int id, string? nombre, string? descripcion)
        {
            var error = Validar(id, nombre, descripcion);
            if (error is not null) return Respuesta.Error(error);
            await using var transaccion = await _db.Database.BeginTransactionAsync();
            try
            {
                if (await _db.tblExamen.AnyAsync(e => e.Id == id))
                {
                    await transaccion.RollbackAsync();
                    return Respuesta.Error($"Ya existe un registro con el Id {id}.");
                }

                _db.tblExamen.Add(new TblExamen
                {
                    Id = id,
                    Nombre = nombre!.Trim(),
                    Descripcion = descripcion?.Trim()
                });
                await _db.SaveChangesAsync();
                await transaccion.CommitAsync();
                return Respuesta.Correcto("Registro insertado satisfactoriamente");
            }
            catch (Exception ex)
            {
                await transaccion.RollbackAsync();
                return Respuesta.Error("Error al insertar: " + MensajeReal(ex));
            }
        }

        public async Task<Respuesta> ActualizarExamen(int id, string? nombre, string? descripcion)
        {
            var error = Validar(id, nombre, descripcion);
            if (error is not null) return Respuesta.Error(error);
            await using var transaccion = await _db.Database.BeginTransactionAsync();
            try
            {
                var examen = await _db.tblExamen.FirstOrDefaultAsync(e => e.Id == id);
                if (examen is null)
                {
                    await transaccion.RollbackAsync();
                    return Respuesta.Error($"No existe un registro con el Id {id}.");
                }
                examen.Nombre = nombre!.Trim();
                examen.Descripcion = descripcion?.Trim();
                await _db.SaveChangesAsync();
                await transaccion.CommitAsync();
                return Respuesta.Correcto("Registro actualizado satisfactoriamente");
            }
            catch (Exception ex)
            {
                await transaccion.RollbackAsync();
                return Respuesta.Error("Error al actualizar: " + MensajeReal(ex));
            }
        }

        public async Task<Respuesta> EliminarExamen(int id)
        {
            if (id <= 0) return Respuesta.Error("El Id debe ser mayor a cero.");
            await using var transaccion = await _db.Database.BeginTransactionAsync();
            try
            {
                var examen = await _db.tblExamen.FirstOrDefaultAsync(e => e.Id == id);
                if (examen is null)
                {
                    await transaccion.RollbackAsync();
                    return Respuesta.Error($"No existe un registro con el Id {id}.");
                }
                _db.tblExamen.Remove(examen);
                await _db.SaveChangesAsync();
                await transaccion.CommitAsync();
                return Respuesta.Correcto("Registro eliminado satisfactoriamente");
            }
            catch (Exception ex)
            {
                await transaccion.RollbackAsync();
                return Respuesta.Error("Error al eliminar: " + MensajeReal(ex));
            }
        }

        public async Task<List<ExamenDto>> ConsultarExamen(int? id, string? nombre, string? descripcion)
        {
            // Transacción de solo lectura 
            await using var transaccion = await _db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
            var consulta = _db.tblExamen.AsNoTracking().AsQueryable();

            if (id.HasValue)
                consulta = consulta.Where(e => e.Id == id.Value);
            if (!string.IsNullOrWhiteSpace(nombre))
            {
                var filtroNombre = nombre.Trim();
                consulta = consulta.Where(e => e.Nombre != null && e.Nombre.Contains(filtroNombre));
            }
            if (!string.IsNullOrWhiteSpace(descripcion))
            {
                var filtroDescripcion = descripcion.Trim();
                consulta = consulta.Where(e => e.Descripcion != null && e.Descripcion.Contains(filtroDescripcion));
            }
            var resultado = await consulta
            .OrderBy(e => e.Id)
            .Select(e => new ExamenDto { Id = e.Id, Nombre = e.Nombre, Descripcion = e.Descripcion })
            .ToListAsync();
            await transaccion.CommitAsync();
            return resultado;
        }

        private static string? Validar(int id, string? nombre, string? descripcion)
        {
            if (id <= 0) return "El Id debe ser mayor a cero.";
            if (string.IsNullOrWhiteSpace(nombre)) return "El Nombre es obligatorio.";
            if (nombre.Trim().Length > LongitudMaxima) return $"El Nombre no puede exceder {LongitudMaxima} caracteres.";
            if (descripcion is not null && descripcion.Trim().Length > LongitudMaxima)
                return $"La Descripción no puede exceder {LongitudMaxima} caracteres.";
            return null;
        }

        private static string MensajeReal(Exception ex) => ex.InnerException?.Message ?? ex.Message;
    }



}
