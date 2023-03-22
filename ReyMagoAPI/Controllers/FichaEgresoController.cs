

using Microsoft.AspNetCore.Mvc;
using FamilyApp.Entities;


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
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<FichaEgreso>>> GetAllEgresos()
        //{
        //    Respuesta<object> respuesta = new();
        //    try
        //    {
        //        var fEgresos = await _repository.SelectAll<FichaEgreso>();
        //        if (fEgresos != null)
        //        {
        //            foreach (var item in fEgresos)
        //            {
        //                respuesta.Data.Add(new
        //                {                        
        //                   item.NombreEgreso,
        //                   item.Importe
        //                });
        //            }
        //            respuesta.Ok = 1;
        //            respuesta.Message = "Success";
        //        }
        //        else
        //        {
        //            respuesta.Ok = 0;
        //            respuesta.Message = "There are no rol yet";
        //        }
                
        //    }
        //    catch (Exception e)
        //    {
        //        respuesta.Ok = 0;
        //        respuesta.Message = e.Message + " " + e.InnerException;
        //    }
        //    return Ok(respuesta);
        //}

       // POST: api/FichaEgreso
       [HttpPost]
        public async Task<ActionResult> PostFichaEngreso(FichaEgreso fichaEgreso)
        {

            Respuesta<object> respuesta = new();
            try
            {
                if (fichaEgreso != null)
                {
                    string guidImagen = null;
                    if (fichaEgreso.File != null)
                    {
                        string ficherosImagenes = Path.Combine(_env.WebRootPath, "File");
                        guidImagen = Guid.NewGuid().ToString() + fichaEgreso.File.FileName;
                        string ruta = Path.Combine(ficherosImagenes, guidImagen);
                        await fichaEgreso.File.CopyToAsync(new FileStream(ruta, FileMode.Create));
                    }
                    FichaEgreso fEgreso = new();
                    fEgreso.Foto = guidImagen;
                    fEgreso.NombreEgreso = fichaEgreso.NombreEgreso;
                    fEgreso.Fecha = fichaEgreso.Fecha;
                    fEgreso.Mes = fichaEgreso.Mes;
                    fEgreso.Importe = fichaEgreso.Importe;

                    await _repository.CreateAsync(fEgreso);
                    respuesta.Ok = 1;
                    respuesta.Message = "Egreso registrado Satisfactoriamente";
                }

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
