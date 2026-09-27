using Microsoft.AspNetCore.Mvc;
using WsApiexamen.Models;
using WsApiexamen.Services;

namespace WsApiexamen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExamenController : ControllerBase
    {
        private readonly ExamenService _servicio;
        public ExamenController(ExamenService servicio) => _servicio = servicio;

        [HttpPost("Agregar")]
        public async Task<ActionResult<Respuesta>> AgregarExamen([FromBody] ExamenDto examen)
        {
            var r = await _servicio.AgregarExamen(examen.Id, examen.Nombre, examen.Descripcion);
            return r.Exito ? Ok(r) : BadRequest(r);
        }

        [HttpPut("Actualizar")]
        public async Task<ActionResult<Respuesta>> ActualizarExamen([FromBody] ExamenDto examen)
        {
            var r = await _servicio.ActualizarExamen(examen.Id, examen.Nombre, examen.Descripcion);
            return r.Exito ? Ok(r) : BadRequest(r);
        }

        [HttpDelete("Eliminar/{id:int}")]
        public async Task<ActionResult<Respuesta>> EliminarExamen(int id)
        {
            var r = await _servicio.EliminarExamen(id);
            return r.Exito ? Ok(r) : BadRequest(r);
        }

        [HttpGet("Consultar")]
        public async Task<ActionResult<List<ExamenDto>>> ConsultarExamen(
        [FromQuery] int? id, [FromQuery] string? nombre, [FromQuery] string? descripcion)
        {
            try
            {
                return Ok(await _servicio.ConsultarExamen(id, nombre, descripcion));
            }
            catch (Exception ex)
            {
                return BadRequest(Respuesta.Error("Error al consultar: " + (ex.InnerException?.Message ?? ex.Message)));
            }
        }
    }
}
