

using Microsoft.AspNetCore.Mvc;
using FamilyApp.Models;


using FamilyApp.Data;
using Microsoft.EntityFrameworkCore;

namespace FamilyApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FichaIngresoController : ControllerBase
    {

        private readonly IRepository _repository;
        private readonly IWebHostEnvironment _env;


        public FichaIngresoController(IRepository repository, IWebHostEnvironment env)
        {
           _repository = repository;
            _env = env;
     
        }

        // GET: api/SolicitudFichaIngresos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FichaIngreso>>> GetAllIngresos()
        {
            Respuesta<object> respuesta = new();
            try
            {
                var fIngresos = await _repository.SelectAll<FichaIngreso>();
                if (fIngresos != null)
                {
                    foreach (var item in fIngresos)
                    {
                        respuesta.Data.Add(item);
                    }
                    respuesta.Ok = 1;
                    respuesta.Message = "Ingresos Registrados";
                }
                else
                {
                    respuesta.Ok = 0;
                    respuesta.Message = "No hay ingresos registrados";
                }

            }
            catch (Exception e)
            {
                respuesta.Ok = 0;
                respuesta.Message = e.Message + " " + e.InnerException;
            }
            return Ok(respuesta);
        }

        //POST: api/FichaEgreso
       [HttpPost("Create")]
        public async Task<ActionResult> PostFichaIngreso(FichaIngreso fichaIngreso)
        {
            Respuesta<object> respuesta = new();
            try
            {
                await _repository.CreateAsync(fichaIngreso);
                respuesta.Ok = 1;
                respuesta.Message = "Success";
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
