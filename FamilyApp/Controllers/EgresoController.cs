using FamilyApp.Data;
using FamilyApp.DTOs.Egresos;
using FamilyApp.DTOs.Ingresos;
using FamilyApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FamilyApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EgresoController : ControllerBase
    {
        private readonly IRepository _repository;//private readonly IMapper _mapper;
        private readonly dbContext _context;

        public EgresoController(IRepository repository, dbContext context)
        {
            _repository = repository;
            _context = context;
        }

        //Egresos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Egreso>>> GetEgresos()
        {
            Respuesta<object> respuesta = new();
            try
            {
                var egresos = await _repository.SelectAll<Egreso>();
                if (egresos != null)
                {
                    foreach (var item in egresos)
                    {
                        respuesta.Data.Add(new
                        {
                            item.Id,
                            item.Nombre
                        });
                    }
                    respuesta.Ok = 1;
                    respuesta.Message = "Egresos cargados en sistema";
                }
                else
                {
                    respuesta.Ok = 0;
                    respuesta.Message = "No se encuenta ningun egreso";
                }
            }
            catch (Exception e)
            {
                respuesta.Ok = 0;
                respuesta.Message = e.Message + " " + e.InnerException;
            }
            return Ok(respuesta);

        }


        // GET: api/egresosTotales
        [HttpGet("totales")]
        public async Task<ActionResult<IEnumerable<Egreso>>> GetAllEgresos()
        {
            var respuesta = new Respuesta<object>();
            try
            {
                var egreT = await (from e in _context.Egresos.AsNoTracking()
                                   join f in _context.FichaEgresos.AsNoTracking() on e.Id equals f.NombreEgreso
                                   where !f.Eliminado
                                   group f by e.Nombre into g
                                   select new
                                   {
                                       Cuenta_Egreso = g.Key,
                                       Total = g.Sum(x => x.Importe)
                                   })
                                  .OrderByDescending(x => x.Total)
                                  .ToListAsync();

                respuesta.Data.Add(egreT);
                respuesta.Ok = 1;
                respuesta.Message = "Egresos e Importe";
                return Ok(respuesta);
            }
            catch (Exception e)
            {
                respuesta.Ok = 0;
                respuesta.Message = e.Message + " " + e.InnerException;
                return Ok(respuesta);
            }
        }


        // GET: api/Egreso/detalle?fechaInicio=2025-09-01&fechaFin=2025-09-26&tipoId=1
        //[HttpGet("detalle")]
        //public async Task<ActionResult> GetEgresosDetalle(
        //    [FromQuery] DateTime? fechaInicio,
        //    [FromQuery] DateTime? fechaFin,
        //    [FromQuery] int? tipoId)
        //{
        //    var respuesta = new Respuesta<object>();
        //    try
        //    {
        //        DateTime? fi = fechaInicio?.Date;
        //        DateTime? ffExcl = fechaFin?.Date.AddDays(1);

        //        var query = from f in _context.FichaEgresos.AsNoTracking()
        //                    join e in _context.Egresos.AsNoTracking()
        //                         on f.NombreEgreso equals e.Id
        //                    where (!fi.HasValue || (f.Fecha.HasValue && f.Fecha.Value >= fi.Value))
        //                       && (!ffExcl.HasValue || (f.Fecha.HasValue && f.Fecha.Value < ffExcl.Value))
        //                       && (!tipoId.HasValue || f.NombreEgreso == tipoId.Value)
        //                    orderby f.Fecha descending, f.Id descending
        //                    select new
        //                    {
        //                        id = f.Id,
        //                        fecha = f.Fecha,
        //                        mes = f.Mes,
        //                        tipoId = e.Id,
        //                        tipo = e.Nombre,
        //                        descripcion = f.Descripcion,
        //                        importe = f.Importe
        //                    };

        //        var detalles = await query.ToListAsync();

        //        respuesta.Data.Add(detalles);
        //        respuesta.Ok = 1;
        //        respuesta.Message = "Detalle de egresos";
        //        return Ok(respuesta);
        //    }
        //    catch (Exception e)
        //    {
        //        respuesta.Ok = 0;
        //        respuesta.Message = e.Message + " " + e.InnerException;
        //        return Ok(respuesta);
        //    }
        //}

        [HttpGet("detalle")]
        public async Task<ActionResult> GetEgresosDetalle(
    [FromQuery] DateTime? fechaInicio,
    [FromQuery] DateTime? fechaFin,
    [FromQuery] int? tipoId)
        {
            var respuesta = new Respuesta<object>();

            try
            {
                // Validación simple
                if (fechaInicio.HasValue && fechaFin.HasValue && fechaFin < fechaInicio)
                    return BadRequest(new { message = "La fecha fin no puede ser menor que la fecha inicio." });

                // Normalizamos a día (inicio inclusivo, fin exclusivo)
                DateTime? fi = fechaInicio?.Date;
                DateTime? ffExcl = fechaFin?.Date.AddDays(1);

                // Base query (filtra soft delete)
                var fq = _context.FichaEgresos
                    .AsNoTracking()
                    .Where(f => !f.Eliminado)
                    .AsQueryable();

                if (fi.HasValue)
                    fq = fq.Where(f => f.Fecha >= fi.Value);

                if (ffExcl.HasValue)
                    fq = fq.Where(f => f.Fecha < ffExcl.Value);

                if (tipoId.HasValue)
                    fq = fq.Where(f => f.NombreEgreso == tipoId.Value);

                var detalles = await fq
                    .Join(_context.Egresos.AsNoTracking(),
                          f => f.NombreEgreso,
                          e => e.Id,
                          (f, e) => new { f, e })
                    .OrderByDescending(x => x.f.Fecha ?? DateTime.MinValue) // orden estable aunque haya NULL
                    .ThenByDescending(x => x.f.Id)
                    .Select(x => new
                    {
                        id = x.f.Id,
                        fecha = x.f.Fecha,
                        mes = x.f.Mes,
                        tipoId = x.e.Id,
                        tipo = x.e.Nombre,
                        descripcion = x.f.Descripcion,
                        importe = x.f.Importe
                    })
                    .ToListAsync();

                respuesta.Data.Add(detalles);
                respuesta.Ok = 1;
                respuesta.Message = "Detalle de egresos";
                return Ok(respuesta);
            }
            catch (Exception e)
            {
                respuesta.Ok = 0;
                respuesta.Message = e.Message + (e.InnerException != null ? " " + e.InnerException.Message : "");
                return Ok(respuesta);
            }
        }



        //[HttpGet("totalesPorMes")]
        //public async Task<ActionResult> GetEgresosPorMesQuery([FromQuery] DateTime fechaInicio,
        //                                                      [FromQuery] DateTime fechaFin)
        //{
        //    var respuesta = new Respuesta<object>();
        //    try
        //    {
        //        var fi = fechaInicio.Date;
        //        var ffExcl = fechaFin.Date.AddDays(1); // [fi, ffExcl)

        //        var egre = await (from e in _context.Egresos.AsNoTracking()
        //                          join f in _context.FichaEgresos.AsNoTracking()
        //                               on e.Id equals f.NombreEgreso
        //                          where f.Fecha.HasValue
        //                             && f.Fecha.Value >= fi
        //                             && f.Fecha.Value < ffExcl
        //                          select new { e.Nombre, f.Importe })
        //                         .OrderByDescending(x => x.Importe)
        //                         .ToListAsync();

        //        var egreT = egre.GroupBy(x => x.Nombre)
        //                        .Select(g => new 
        //                        { Cuenta_Egreso = g.Key, 
        //                            Total = g.Sum(x => x.Importe) 
        //                        });

        //        respuesta.Data.Add(egreT);
        //        respuesta.Ok = 1;
        //        respuesta.Message = "Egresos e Importe";
        //        return Ok(respuesta);
        //    }
        //    catch (Exception ex)
        //    {
        //        respuesta.Ok = 0; respuesta.Message = ex.Message;
        //        return Ok(respuesta);
        //    }
        //}

        [HttpGet("totalesPorMes")]
        public async Task<ActionResult> GetEgresosPorMesQuery([FromQuery] DateTime fechaInicio,
                                                      [FromQuery] DateTime fechaFin)
        {
            var respuesta = new Respuesta<object>();
            try
            {
                var fi = fechaInicio.Date;
                var ffExcl = fechaFin.Date.AddDays(1);

                var egreT = await (from e in _context.Egresos.AsNoTracking()
                                   join f in _context.FichaEgresos.AsNoTracking() on e.Id equals f.NombreEgreso
                                   where !f.Eliminado
                                      && f.Fecha.HasValue
                                      && f.Fecha.Value >= fi
                                      && f.Fecha.Value < ffExcl
                                   group f by e.Nombre into g
                                   select new { Cuenta_Egreso = g.Key, Total = g.Sum(x => x.Importe) })
                                  .OrderByDescending(x => x.Total)
                                  .ToListAsync();

                respuesta.Data.Add(egreT);
                respuesta.Ok = 1;
                respuesta.Message = "Egresos e Importe";
                return Ok(respuesta);
            }
            catch (Exception ex)
            {
                respuesta.Ok = 0; respuesta.Message = ex.Message;
                return Ok(respuesta);
            }
        }


        // POST: api/Egreso
        [HttpPost]
        public async Task<ActionResult> PostEgreso(Egreso egreso)
        {

            Respuesta<object> respuesta = new();
            try
            {

                await _repository.CreateAsync(egreso);
                respuesta.Ok = 1;
                respuesta.Message = "Egreso registrado satisfactoriamente";


            }
            catch (Exception e)
            {
                respuesta.Ok = 0;
                respuesta.Message = e.Message + " " + e.InnerException;
                return Ok(respuesta);
            }
            return Ok(respuesta);
        }

        [HttpPut("detalle/{id:int}")]
        public async Task<ActionResult> PutDetalleEgreso(int id, [FromBody] FichaEgresoUpdateDTO dto)
        {
            var respuesta = new Respuesta<object>();
            try
            {
                var fe = await _context.FichaEgresos.FirstOrDefaultAsync(x => x.Id == id && !x.Eliminado);
                if (fe == null)
                {
                    respuesta.Ok = 0;
                    respuesta.Message = "No existe el detalle o fue eliminado.";
                    return NotFound(respuesta);
                }

                if (dto.Fecha.HasValue) fe.Fecha = dto.Fecha;
                if (!string.IsNullOrWhiteSpace(dto.Mes)) fe.Mes = dto.Mes;
                if (dto.NombreEgreso.HasValue) fe.NombreEgreso = dto.NombreEgreso.Value;
                if (dto.Descripcion != null) fe.Descripcion = dto.Descripcion; // permite null
                if (dto.Importe.HasValue) fe.Importe = dto.Importe.Value;
                if (dto.Foto != null) fe.Foto = dto.Foto;

                await _context.SaveChangesAsync();

                respuesta.Ok = 1;
                respuesta.Message = "Detalle de egreso actualizado correctamente.";
                respuesta.Data.Add(new { fe.Id });
                return Ok(respuesta);
            }
            catch (Exception e)
            {
                respuesta.Ok = 0;
                respuesta.Message = e.Message + (e.InnerException != null ? " " + e.InnerException.Message : "");
                return Ok(respuesta);
            }
        }

        [HttpDelete("detalle/{id:int}")]
        public async Task<ActionResult> DeleteDetalleEgresoSoft(int id)
        {
            var respuesta = new Respuesta<object>();
            try
            {
                var fe = await _context.FichaEgresos.FirstOrDefaultAsync(x => x.Id == id && !x.Eliminado);
                if (fe == null)
                {
                    respuesta.Ok = 0;
                    respuesta.Message = "No existe el detalle o ya está eliminado.";
                    return NotFound(respuesta);
                }

                fe.Eliminado = true;
                fe.FechaEliminacion = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                respuesta.Ok = 1;
                respuesta.Message = "Detalle de egreso eliminado (lógico) correctamente.";
                respuesta.Data.Add(new { fe.Id, fe.Eliminado, fe.FechaEliminacion });
                return Ok(respuesta);
            }
            catch (Exception e)
            {
                respuesta.Ok = 0;
                respuesta.Message = e.Message + (e.InnerException != null ? " " + e.InnerException.Message : "");
                return Ok(respuesta);
            }
        }


    }
}
