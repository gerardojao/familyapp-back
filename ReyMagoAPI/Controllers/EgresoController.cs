using System;using Microsoft.AspNetCore.Mvc;



using FamilyApp.Entities;
using FamilyApp.Data;
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

        // GET: api/SolicitudIngreso
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Egreso>>> GetAllEgresos()
        {
            Respuesta<object> respuesta = new();
            try
            {
                //var egresos = await _repository.SelectAll<Egreso>();
                //if (egresos != null)
                //{
                //    foreach (var item in egresos)
                //    {
                //        respuesta.Data.Add(new
                //        {
                //            item.Id,
                //            item.Nombre
                //        });
                //    }
                //    respuesta.Ok = 1;
                //    respuesta.Message = "Egresos cargados en sistema";
                //}
                //else
                //{
                //    respuesta.Ok = 0;
                //    respuesta.Message = "No se encuenta ningun egreso";
                //}
                var egre = await (from _egreso in _context.Egresos
                                  join _fEgreso in _context.FichaEgresos on _egreso.Id equals _fEgreso.NombreEgreso
                                  where _egreso.Id == _fEgreso.NombreEgreso                              
                                  select new
                                  {
                                      _egreso.Nombre,
                                      _fEgreso.Importe
                                  }).ToListAsync();

                if (egre != null)
                {
                    var egreT = from e in egre
                                group e by e.Nombre into totals
                                select new
                                {
                                    Cuenta_Egreso = totals.Key,
                                    Total = totals.Sum(e=>e.Importe)
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
        
    }
}
