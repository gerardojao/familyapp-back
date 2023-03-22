using System;
using Microsoft.AspNetCore.Mvc;
using FamilyApp.Entities;
using FamilyApp.Data;
using Microsoft.EntityFrameworkCore;

namespace FamilyApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IngresoController : ControllerBase
    {

       
        private readonly dbContext _context;

        public IngresoController(dbContext context)
        {
            _context = context;
        }

        // GET: api/SolicitudIngreso
        [HttpGet]
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
                                  }).ToListAsync();

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
        
    }
}
