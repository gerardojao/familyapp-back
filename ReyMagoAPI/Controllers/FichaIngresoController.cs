

using Microsoft.AspNetCore.Mvc;
using FamilyApp.Entities;


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
                        respuesta.Data.Add(new
                        {
                            item.NombreIngreso,
                            item.Importe
                        });
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

        // POST: api/FichaEgreso
        //[HttpPost]
        // public async Task<ActionResult> PostFichaEngreso(FichaEgreso fichaEgreso)
        // {

        //     Respuesta<object> respuesta = new();
        //     try
        //     {
        //         if (fichaEgreso != null)
        //         {
        //             string guidImagen = null;
        //             if (fichaEgreso.File != null)
        //             {
        //                 string ficherosImagenes = Path.Combine(_env.WebRootPath, "File");
        //                 guidImagen = Guid.NewGuid().ToString() + fichaEgreso.File.FileName;
        //                 string ruta = Path.Combine(ficherosImagenes, guidImagen);
        //                 await fichaEgreso.File.CopyToAsync(new FileStream(ruta, FileMode.Create));
        //             }
        //             FichaEgreso fEgreso = new();
        //             fEgreso.Foto = guidImagen;
        //             fEgreso.NombreEgreso = fichaEgreso.NombreEgreso;
        //             fEgreso.Fecha = fichaEgreso.Fecha;
        //             fEgreso.Mes = fichaEgreso.Mes;
        //             fEgreso.Importe = fichaEgreso.Importe;

        //             await _repository.CreateAsync(fEgreso);
        //             respuesta.Ok = 1;
        //             respuesta.Message = "Egreso registrado Satisfactoriamente";
        //         }

        //     }
        //     catch (Exception e)
        //     {
        //         respuesta.Ok = 0;
        //         respuesta.Message = e.Message + " " + e.InnerException;
        //         return Ok(respuesta);
        //     }
        //     return Ok(respuesta);
        // }


    }
}
