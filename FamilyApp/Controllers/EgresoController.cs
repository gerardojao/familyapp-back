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

        // GET: api/egresosTotales por mes
        [HttpGet("totalesPorMes{fechaInicio}/{fechaFin}")]
        public async Task<ActionResult<IEnumerable<Egreso>>> GetEgresosPorMes( DateTime fechaInicio, DateTime fechaFin)
        {
            Respuesta<object> respuesta = new();
            try
            {
                var egre = await (from _egreso in _context.Egresos
                                  join _fEgreso in _context.FichaEgresos on _egreso.Id equals _fEgreso.NombreEgreso
                                  where _egreso.Id == _fEgreso.NombreEgreso && _fEgreso.Fecha >= fechaInicio && _fEgreso.Fecha <= fechaFin
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
