﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Security;
using Antlr.Runtime;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using VlaboralApi.Infrastructure;
using VLaboralApi.Hubs;
using VLaboralApi.Models;
using VLaboralApi.Services;

namespace VLaboralApi.Controllers
{
    //public abstract class ApiControllerWithHub<THub> : ApiController
    //  where THub : IHub
    //{
    //    Lazy<IHubContext> hub = new Lazy<IHubContext>(
    //        () => GlobalHost.ConnectionManager.GetHubContext<THub>()
    //    );

    //    protected IHubContext Hub
    //    {
    //        get { return hub.Value; }
    //    }
    //}

    //public class NotificacionesController : ApiControllerWithHub<NotificacionesHub>

    public class NotificacionesController : ApiController
    {
        private VLaboral_Context db = new VLaboral_Context();

        // GET: api/Notificaciones/5
        //[ResponseType(typeof(Notificacion))]
        public IHttpActionResult GetNotificacion(int id, string tipoNotificacion)
        {
            switch (tipoNotificacion)
            {
                case "EXP":
                    return Ok(db.Notificaciones.OfType<NotificacionExperiencia>().Include(n=> n.ExperienciaLaboral.Empresa).FirstOrDefault(n => n.Id == id));
                case "EXPVER":
                    return Ok(db.Notificaciones.OfType<NotificacionExperiencia>().Include(n => n.ExperienciaLaboral.Empresa).FirstOrDefault(n => n.Id == id));
                case "POS":
                    return Ok(db.Notificaciones.OfType<NotificacionPostulacion>().Include(n => n.Postulacion.PuestoEtapaOferta.EtapaOferta.Oferta).FirstOrDefault(n => n.Id == id));
                case "ETAP":
                    return Ok(db.Notificaciones.OfType<NotificacionPostulacion>().Include(n => n.Postulacion.PuestoEtapaOferta.EtapaOferta.Oferta).FirstOrDefault(n => n.Id == id));
                default:
                    return BadRequest("No se han encontrado notificaciones que respondan a los parámetros ingresados.");
            }
        }

        private int? GetReceptorId(string tipoReceptor)
        {
            var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new VLaboral_Context()));
            var usuarioId = User.Identity.GetUserId();
            switch (tipoReceptor)
            {
                case "profesional":
                    return 
                        Convert.ToInt32(manager.GetClaims(usuarioId).FirstOrDefault(r => r.Type == "profesionalId").Value);
                case "empresa":
                    return Convert.ToInt32(manager.GetClaims(usuarioId).FirstOrDefault(r => r.Type == "empresaId").Value);
                case "administracion":
                    return null;
            }
            return null;
        }

        private string GetTipoReceptor()
        {
            var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new VLaboral_Context()));
            var usuarioId = User.Identity.GetUserId();
            var appUsertype = manager.GetClaims(usuarioId).FirstOrDefault(r => r.Type == "app_usertype");
            return appUsertype == null ? null : appUsertype.Value;
        }

        // GET: api/Notificaciones
        //[Authorize]
        // [ResponseType(typeof(List<Notificacion>))]
        [ResponseType(typeof(CustomPaginateResult<Oferta>))]
        public IHttpActionResult GetNotificacionesRecibidas(int page, int rows)
        {
            var tipoReceptor = GetTipoReceptor();
            if (tipoReceptor == null) return null;

            var receptorId = GetReceptorId(tipoReceptor);
            if (receptorId == null) return null;

            var totalRows = db.Notificaciones.Count(n => n.ReceptorId == receptorId
                                && n.FechaPublicacion <= DateTime.Now
                                && (n.FechaVencimiento >= DateTime.Now || n.FechaVencimiento == null)
                                && n.TipoNotificacion.TipoReceptor == tipoReceptor);

            var totalPages = (int)Math.Ceiling((double)totalRows / rows);

            var results = db.Notificaciones
                .Where(n => n.ReceptorId == receptorId
                                && n.FechaPublicacion <= DateTime.Now
                                && (n.FechaVencimiento >= DateTime.Now || n.FechaVencimiento == null)
                                && n.TipoNotificacion.TipoReceptor == tipoReceptor)
                                .Include(n => n.TipoNotificacion)
                                .OrderByDescending(n => n.FechaPublicacion)
                .Skip((page - 1) * rows) //SLuna: -1 Para manejar indice(1) en pagina
                .Take(rows)
                .ToList();

            var result = new CustomPaginateResult<Notificacion>()
            {
                PageSize = rows,
                TotalRows = totalRows,
                TotalPages = totalPages,
                CurrentPage = page,
                Results = results
            };

            return Ok(result);
        }

        // GET: api/Notificaciones
        //[Authorize]
        //public IHttpActionResult GetNotificacionesEnviadas()
        //{

        //    var claimsIdentity = User.Identity as ClaimsIdentity;
        //    if (claimsIdentity == null) return BadRequest();

        //    int? emisorId = null;

        //    var appUsertype = claimsIdentity.Claims.FirstOrDefault(r => r.Type == "app_usertype");
        //    if (appUsertype == null) return BadRequest();

        //    var tipoEmisor = appUsertype.Value;
        //    switch (tipoEmisor)
        //    {
        //        case "profesional":
        //            emisorId =
        //                Convert.ToInt32(claimsIdentity.Claims.FirstOrDefault(r => r.Type == "profesionalId").Value);
        //            break;

        //        case "empresa":
        //            emisorId = Convert.ToInt32(claimsIdentity.Claims.FirstOrDefault(r => r.Type == "empresaId").Value);
        //            break;
        //        case "administracion":
        //            break;
        //    }

        //    if (emisorId == null) return BadRequest();

        //    var resultado = db.Notificaciones
        //            .Where(n => n.EmisorId == emisorId
        //                        && n.FechaPublicacion <= DateTime.Now
        //                        && (n.FechaVencimiento >= DateTime.Now || n.FechaVencimiento == null)
        //                        && n.TipoNotificacion.TipoEmisor == tipoEmisor)
        //            .Include(n => n.TipoNotificacion)
        //            .OrderByDescending(n => n.FechaPublicacion);

        //    return Ok(resultado.ToList());
        //}

     

        // PUT: api/Notificaciones/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutNotificacion(int id, Notificacion notificacion)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != notificacion.Id)
            {
                return BadRequest();
            }

            notificacion.FechaLectura = DateTime.Now;
            db.Entry(notificacion).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NotificacionExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        private void SendNotificacion(string who, Notificacion notificacion)
        {



        }

        // POST: api/Notificaciones
        [ResponseType(typeof(Notificacion))]
        public IHttpActionResult PostNotificacionEtapaAprobada(int postulacionId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var postulacion = db.Postulacions
                                    .Include(p => p.PuestoEtapaOferta.EtapaOferta.Oferta)
                        .FirstOrDefault(p => p.Id == postulacionId);


            if (postulacion == null) return BadRequest(ModelState);

            var tipoNotificacion = db.TipoNotificaciones.FirstOrDefault(tn => tn.Valor == "ETAP");

            if (tipoNotificacion == null) return BadRequest(ModelState);
            {
                var notificacion = new NotificacionPostulacion()
                {
                    PostulacionId = postulacion.Id,
                    // EtapaOfertaId = postulacion.PuestoEtapaOferta.EtapaOfertaId,
                    FechaCreacion = DateTime.Now,
                    FechaPublicacion = DateTime.Now,
                    Mensaje = tipoNotificacion.Mensaje, // "Este mensaje hay que sacarlo de la bd. Por ahora lo hardcodeo aqui",
                    Titulo = tipoNotificacion.Titulo, //"El título lo podemos sacar de la clase directamente o desde la bd. Prefiero desde la bd.",
                    TipoNotificacionId = tipoNotificacion.Id,
                    ReceptorId = postulacion.ProfesionalId,
                    EmisorId = postulacion.PuestoEtapaOferta.EtapaOferta.Oferta.EmpresaId
                };

                db.Notificaciones.Add(notificacion);
                db.SaveChanges();

                return Ok();
                //  return CreatedAtRoute("DefaultApi", new { id = notificacion.Id }, notificacion);
            }
        }

        // POST: api/Notificaciones
        [ResponseType(typeof(Notificacion))]
        public IHttpActionResult PostNotificacion(Notificacion notificacion)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }



            //Existe Emisor

            //Existe Receptor

            //Existe tipo



            db.Notificaciones.Add(notificacion);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = notificacion.Id }, notificacion);
        }

        // DELETE: api/Notificaciones/5
        [ResponseType(typeof(Notificacion))]
        public IHttpActionResult DeleteNotificacion(int id)
        {
            Notificacion notificacion = db.Notificaciones.Find(id);
            if (notificacion == null)
            {
                return NotFound();
            }

            db.Notificaciones.Remove(notificacion);
            db.SaveChanges();

            return Ok(notificacion);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool NotificacionExists(int id)
        {
            return db.Notificaciones.Count(e => e.Id == id) > 0;
        }
    }


}