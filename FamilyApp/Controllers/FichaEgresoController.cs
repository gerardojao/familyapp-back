

using Microsoft.AspNetCore.Mvc;
using FamilyApp.Models;


using FamilyApp.Data;
using Microsoft.EntityFrameworkCore;

namespace FamilyApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FichaEgresoController : ControllerBase
    {

        private readonly IRepository _repository;
        private readonly IWebHostEnvironment _env;


        public FichaEgresoController(IRepository repository, IWebHostEnvironment env)
        {
           _repository = repository;
            _env = env;
     
        }

        //// GET: api/SolicitudFichaEgresos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FichaEgreso>>> GetAllEgresos()
        {
            Respuesta<object> respuesta = new();
            try
            {
                var fEgresos = await _repository.SelectAll<FichaEgreso>();
                if (fEgresos != null)
                {
                    foreach (var item in fEgresos)
                    {
                        respuesta.Data.Add(item);
                    }
                    respuesta.Ok = 1;
                    respuesta.Message = "Egresos registrado";
                }
                else
                {
                    respuesta.Ok = 0;
                    respuesta.Message = "No hay egresos registradosThere are no rol yet";
                }

            }
            catch (Exception e)
            {
                respuesta.Ok = 0;
                respuesta.Message = e.Message + " " + e.InnerException;
            }
            return Ok(respuesta);
        }

       
        // POST: api/FichaEgreso
        [HttpPost]
        public async Task<ActionResult> PostFichaEngreso(FichaEgreso fichaEgreso)
        {

            Respuesta<object> respuesta = new();
            try
            {
               
                    await _repository.CreateAsync(fichaEgreso);
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
