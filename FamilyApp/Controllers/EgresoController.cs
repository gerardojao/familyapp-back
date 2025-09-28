using FamilyApp.Data;

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
            Respuesta<object> respuesta = new();
            try
            {
                var egre = await (from _egreso in _context.Egresos
                                  join _fEgreso in _context.FichaEgresos on _egreso.Id equals _fEgreso.NombreEgreso
                                  where _egreso.Id == _fEgreso.NombreEgreso
                                  select new
                                  {
                                      _egreso.Nombre,
                                      _fEgreso.Importe
                                  }).OrderByDescending(x => x.Importe).ToListAsync();

                if (egre != null)
                {
                    var egreT = from e in egre
                                group e by e.Nombre into totals
                                select new
                                {
                                    Cuenta_Egreso = totals.Key,
                                    Total = totals.Sum(e => e.Importe)
                                };

                    respuesta.Data.Add(egreT);
                    respuesta.Ok = 1;
                    respuesta.Message = "Egresos e Importe";
                }
                else
                {
                    respuesta.Ok = 0;
                    respuesta.Message = "No hay Egresos";
                }
            }
            catch (Exception e)
            {
                respuesta.Ok = 0;
                respuesta.Message = e.Message + " " + e.InnerException;
            }
            return Ok(respuesta);
        }

        // GET: api/Egreso/detalle?fechaInicio=2025-09-01&fechaFin=2025-09-26&tipoId=1
        [HttpGet("detalle")]
        public async Task<ActionResult> GetEgresosDetalle(
            [FromQuery] DateTime? fechaInicio,
            [FromQuery] DateTime? fechaFin,
            [FromQuery] int? tipoId)
        {
            var respuesta = new Respuesta<object>();
            try
            {
                DateTime? fi = fechaInicio?.Date;
                DateTime? ffExcl = fechaFin?.Date.AddDays(1);

                var query = from f in _context.FichaEgresos.AsNoTracking()
                            join e in _context.Egresos.AsNoTracking()
                                 on f.NombreEgreso equals e.Id
                            where (!fi.HasValue || (f.Fecha.HasValue && f.Fecha.Value >= fi.Value))
                               && (!ffExcl.HasValue || (f.Fecha.HasValue && f.Fecha.Value < ffExcl.Value))
                               && (!tipoId.HasValue || f.NombreEgreso == tipoId.Value)
                            orderby f.Fecha descending, f.Id descending
                            select new
                            {
                                id = f.Id,
                                fecha = f.Fecha,
                                mes = f.Mes,
                                tipoId = e.Id,
                                tipo = e.Nombre,
                                descripcion = f.Descripcion,
                                importe = f.Importe
                            };

                var detalles = await query.ToListAsync();

                respuesta.Data.Add(detalles);
                respuesta.Ok = 1;
                respuesta.Message = "Detalle de egresos";
                return Ok(respuesta);
            }
            catch (Exception e)
            {
                respuesta.Ok = 0;
                respuesta.Message = e.Message + " " + e.InnerException;
                return Ok(respuesta);
            }
        }


        [HttpGet("totalesPorMes")]
        public async Task<ActionResult> GetEgresosPorMesQuery([FromQuery] DateTime fechaInicio,
                                                              [FromQuery] DateTime fechaFin)
        {
            var respuesta = new Respuesta<object>();
            try
            {
                var fi = fechaInicio.Date;
                var ffExcl = fechaFin.Date.AddDays(1); // [fi, ffExcl)

                var egre = await (from e in _context.Egresos.AsNoTracking()
                                  join f in _context.FichaEgresos.AsNoTracking()
                                       on e.Id equals f.NombreEgreso
                                  where f.Fecha.HasValue
                                     && f.Fecha.Value >= fi
                                     && f.Fecha.Value < ffExcl
                                  select new { e.Nombre, f.Importe })
                                 .OrderByDescending(x => x.Importe)
                                 .ToListAsync();

                var egreT = egre.GroupBy(x => x.Nombre)
                                .Select(g => new 
                                { Cuenta_Egreso = g.Key, 
                                    Total = g.Sum(x => x.Importe) 
                                });

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
    }
}
