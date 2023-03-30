using System;
using Microsoft.AspNetCore.Mvc;

using FamilyApp.Data;
using Microsoft.EntityFrameworkCore;
using FamilyApp.Models;

namespace FamilyApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IngresoController : ControllerBase
    {
        private readonly IRepository _repository;
        private readonly IWebHostEnvironment _env;
        private readonly dbContext _context;

        public IngresoController(IRepository repository, IWebHostEnvironment env, dbContext context)
        {
            _repository = repository;
            _env = env;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Ingreso>>> GetIngresos()
        {
            Respuesta<object> respuesta = new();
            try
            {
                var ingresos = await _repository.SelectAll<Ingreso>();
                if (ingresos != null)
                {
                    foreach (var item in ingresos)
                    {
                        respuesta.Data.Add(new
                        {
                            item.Id,
                            item.NombreIngreso
                        });
                    }
                    respuesta.Ok = 1;
                    respuesta.Message = "Ingresos Registrados";
                }
                else
                {
                    respuesta.Ok = 0;
                    respuesta.Message = "No se encuenta ningun ingreso";
                }
            }
            catch (Exception e)
            {
                respuesta.Ok = 0;
                respuesta.Message = e.Message + " " + e.InnerException;
            }
            return Ok(respuesta);

        }
    

        // GET: api/SolicitudIngreso
        [HttpGet("totales")]
        public async Task<ActionResult<IEnumerable<Ingreso>>> GetAllIngresos()
        {
            Respuesta<object> respuesta = new();
            try
            {                
                var ingre = await (from _ingreso in _context.Ingresos
                                  join _fIngreso in _context.FichaIngresos on _ingreso.Id equals _fIngreso.NombreIngreso
                                  where _ingreso.Id == _fIngreso.NombreIngreso                          
                                  select new
                                  {
                                      _ingreso.NombreIngreso,
                                      _fIngreso.Importe
                                  }).OrderByDescending(x => x.Importe).ToListAsync();

                if (ingre != null)
                {
                    var ingreT = from i in ingre
                                group i by i.NombreIngreso into totals
                                select new
                                {
                                    Cuenta_Ingreso = totals.Key,
                                    Total = totals.Sum(e=>e.Importe)
                                };
                    respuesta.Data.Add(ingreT);
                    respuesta.Ok = 1;
                    respuesta.Message = "Ingresos e Importe";
                }
                else
                {
                    respuesta.Ok = 0;
                    respuesta.Message = "No hay Ingresos";
                }
            }
            catch (Exception e)
            {
                respuesta.Ok = 0;
                respuesta.Message = e.Message + " " + e.InnerException;
            }
            return Ok(respuesta);
        }

        // GET: api/SolicitudIngreso
        [HttpGet("totalesPorMes/{fechaInicio}/{fechaFin}")]
        public async Task<ActionResult<IEnumerable<Ingreso>>> GetIngresosPorMes(DateTime fechaInicio, DateTime fechaFin)
        {
            Respuesta<object> respuesta = new();
            try
            {
                var ingre = await (from _ingreso in _context.Ingresos
                                   join _fIngreso in _context.FichaIngresos on _ingreso.Id equals _fIngreso.NombreIngreso
                                   where _ingreso.Id == _fIngreso.NombreIngreso && _fIngreso.Fecha >= fechaInicio && _fIngreso.Fecha <= fechaFin
                                   select new
                                   {
                                       _ingreso.NombreIngreso,
                                       _fIngreso.Importe
                                   }).OrderByDescending(x => x.Importe).ToListAsync();

                if (ingre != null)
                {
                    var ingreT = from i in ingre
                                 group i by i.NombreIngreso into totals
                                 select new
                                 {
                                     Cuenta_Ingreso = totals.Key,
                                     Total = totals.Sum(e => e.Importe)
                                 };
                    respuesta.Data.Add(ingreT);
                    respuesta.Ok = 1;
                    respuesta.Message = "Ingresos e Importe";
                }
                else
                {
                    respuesta.Ok = 0;
                    respuesta.Message = "No hay Ingresos";
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
        public async Task<ActionResult> PostIngreso(Ingreso ingreso)
        {

            Respuesta<object> respuesta = new();
            try
            {

                await _repository.CreateAsync(ingreso);
                respuesta.Ok = 1;
                respuesta.Message = "Ingreso registrado satisfactoriamente";


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
