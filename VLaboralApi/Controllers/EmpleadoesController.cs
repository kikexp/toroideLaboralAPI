﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json.Linq;
using VLaboralApi.ClasesAuxiliares;
using VLaboralApi.Models;
using VLaboralApi.Models.Ubicacion;
using VLaboralApi.Services;
using VLaboralApi.ViewModels.Empleados;
using VLaboralApi.ViewModels.Filtros;

namespace VLaboralApi.Controllers
{
    public class EmpleadoesController : ApiController
    {
        private VLaboral_Context db = new VLaboral_Context();

        // GET: api/Empleadoes
        public IQueryable<Empleado> GetEmpleadoes()
        {
            return db.Empleadoes;
        }

        
        [ResponseType(typeof(Empleado))]
        public IHttpActionResult GetEmpleado(int tipoIdentificacion, string valor )
        {

            var empresaId = Utiles.GetEmpresaId(User.Identity.GetUserId());
            Empleado empleado = null;
            if (empresaId != null)
            {
                empleado = db.Empleadoes
                    .FirstOrDefault(e => e.EmpresaId == empresaId &
                                         e.IdentificacionesEmpleado
                                             .Any(ie => ie.TipoIdentificacionEmpleadoId == tipoIdentificacion & ie.Valor == valor));
            }
            return Ok(empleado);
        }

        // GET: api/Empleadoes/5
        [ResponseType(typeof(Empleado))]
        public IHttpActionResult GetEmpleado(int id)
        {
            Empleado empleado = db.Empleadoes.Find(id);
            if (empleado == null)
            {
                return NotFound();
            }

            return Ok(empleado);
        }

         // PUT: api/Empleadoes/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutEmpleado(int id, Empleado empleado)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != empleado.Id)
            {
                return BadRequest();
            }

            db.Entry(empleado).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmpleadoExists(id))
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

        // POST: api/Empleadoes
        [ResponseType(typeof(Empleado))]
        public IHttpActionResult PostEmpleado(EmpleadoVM empleadoVm)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (Utiles.GetEmpresaId(User.Identity.GetUserId()) == null)
                {
                    return BadRequest("Error, el usuario no tiene EmpresaId");
                }

                //if (db.Empresas.Find(empleadoVm.EmpresaId) == null) return BadRequest("Verificar EmpresaId");

                var profesional = new Profesional();
                Empleado empleado;

                if (empleadoVm.ProfesionalId == null)
                {
                    GuardarProfesional(empleadoVm, profesional);
                    empleado = GuardarEmpleado(empleadoVm, profesional);
                    CargarExperienciasLaborales(empleadoVm, profesional);
                }
                else
                {
                    profesional = db.Profesionals.Find(empleadoVm.ProfesionalId);

                    if (profesional == null) return BadRequest("Verificar ProfesionalId");

                    empleado = GuardarEmpleado(empleadoVm, profesional);
                    CargarExperienciasLaborales(empleadoVm, profesional);
                }
                return Ok(empleado);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
          
           
        }

        private void GuardarProfesional(EmpleadoVM empleadoVm, Profesional profesional)
        {
            profesional.Apellido = empleadoVm.Apellido;
            profesional.Nombre = empleadoVm.Nombre;
            profesional.FechaNac = empleadoVm.FechaNac;
            profesional.Nacionalidad = empleadoVm.Nacionalidad;
            if (empleadoVm.Domicilio != null)
            {
                profesional.Domicilio = new Domicilio()
                {
                    PlaceId = empleadoVm.Domicilio.PlaceId,
                    Calle = empleadoVm.Domicilio.Calle,
                    Nro = empleadoVm.Domicilio.Nro,
                    Piso = empleadoVm.Domicilio.Piso,
                    Dpto = empleadoVm.Domicilio.Dpto,
                    CodigoPostal = empleadoVm.Domicilio.CodigoPostal
                };
                if (empleadoVm.Domicilio.Location != null) profesional.Domicilio.Location = empleadoVm.Domicilio.Location;
                if (empleadoVm.Domicilio.CiudadId != null && db.Ciudades.Any(c => c.Id == empleadoVm.Domicilio.CiudadId))
                    profesional.Domicilio.CiudadId = empleadoVm.Domicilio.CiudadId;
            }
            profesional.Sexo = empleadoVm.Sexo.ToString();
            db.Profesionals.Add(profesional);
            db.SaveChanges();

            foreach (var identificacion in empleadoVm.IdentificacionesEmpleado)
            {
                var identificacionProfesional = new IdentificacionProfesional()
                {
                    Valor = identificacion.Valor,
                    TipoIdentificacionProfesionalId = identificacion.TipoIdentificacionEmpleadoId,
                    ProfesionalId = profesional.Id
                };
                db.IdentificacionesProfesional.Add(identificacionProfesional);
            }
            db.SaveChanges();

           // CargarExperienciasLaborales(empleadoVm, profesional);
        }

        private Empleado GuardarEmpleado(EmpleadoVM empleadoVm, Profesional profesional )
        {
            var empleado = new Empleado
            {
                Apellido = empleadoVm.Apellido,
                Nombre = empleadoVm.Nombre,
                FechaNac = empleadoVm.FechaNac,
                Nacionalidad = empleadoVm.Nacionalidad,
                Sexo = empleadoVm.Sexo,
                ProfesionalId = profesional.Id,
                EmpresaId =  Utiles.GetEmpresaId(User.Identity.GetUserId()),
                Legajo =  empleadoVm.Legajo,
                FechaInicioVigencia = empleadoVm.FechaInicioVigencia,
                FechaFinVigencia = empleadoVm.FechaFinVigencia
            };

            if (empleadoVm.Domicilio != null)
            {
                empleado.Domicilio = new Domicilio()
                {
                    PlaceId = empleadoVm.Domicilio.PlaceId,
                    Calle = empleadoVm.Domicilio.Calle,
                    Nro = empleadoVm.Domicilio.Nro,
                    Piso = empleadoVm.Domicilio.Piso,
                    Dpto = empleadoVm.Domicilio.Dpto,
                    CodigoPostal = empleadoVm.Domicilio.CodigoPostal
                };
                if (empleadoVm.Domicilio.CiudadId != null && db.Ciudades.Any(c => c.Id == empleadoVm.Domicilio.CiudadId))
                    empleado.Domicilio.CiudadId = empleadoVm.Domicilio.CiudadId;
            }

            db.Empleadoes.Add(empleado);
            db.SaveChanges();

             foreach (var identificacion in empleadoVm.IdentificacionesEmpleado)
            {
                var identificacionEmpleado = new IdentificacionEmpleado()
                {
                    Valor = identificacion.Valor,
                    TipoIdentificacionEmpleadoId =  identificacion.TipoIdentificacionEmpleadoId,
                    EmpleadoId = empleado.Id
                };
                db.IdentificacionesEmpleado.Add(identificacionEmpleado);
            }
             db.SaveChanges();
            return empleado;
        }

        private void CargarExperienciasLaborales(EmpleadoVM empleadoVm, Profesional profesional)
        {
            if (empleadoVm.ExperienciasLaborales == null) return;

            foreach (var experiencia in empleadoVm.ExperienciasLaborales)
            {
                var experienciaLaboral = new ExperienciaLaboral
                {
                    ProfesionalId = profesional.Id,
                    Descripcion = experiencia.Descripcion,
                    EmpresaId = Utiles.GetEmpresaId(User.Identity.GetUserId()),
                    FechaCreacion = DateTime.Now.Date,
                    PeriodoDesde = experiencia.PeriodoDesde,
                    PeriodoHasta = experiencia.PeriodoHasta,
                    Puesto = experiencia.Puesto,
                    Ubicacion = experiencia.Ubicacion,
                    idUsuarioCreacion = User.Identity.GetUserId()
                };
                db.ExperienciaLaborals.Add(experienciaLaboral);
            }
            db.SaveChanges();
        }

        // DELETE: api/Empleadoes/5
        [ResponseType(typeof(Empleado))]
        public IHttpActionResult DeleteEmpleado(int id)
        {
            Empleado empleado = db.Empleadoes.Find(id);
            if (empleado == null)
            {
                return NotFound();
            }

            db.Empleadoes.Remove(empleado);
            db.SaveChanges();

            return Ok(empleado);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool EmpleadoExists(int id)
        {
            return db.Empleadoes.Count(e => e.Id == id) > 0;
        }

        [HttpPost]
        [Route("api/Empleados/QueryOptions")]
        public IHttpActionResult QueryOptions(EmpleadosOptionsBindingModel options)
        {
            dynamic filters = new JObject();

            if (options != null && options.Filters != null)
            {
                 if (User == null)
                {
                   return BadRequest("Debe ser una empresa para poder consultar el listado de empleados.");
                }

                var empresaId = Utiles.GetEmpresaId(User.Identity.GetUserId());
           
                if (options.Filters.Contains(EmpleadosFilterOptions.Sexo))
                {
                    filters.Sexo = Enum.GetNames(typeof(Sexo));  
                }
                if (options.Filters.Contains(EmpleadosFilterOptions.Estado))
                {
                    filters.Estado = Enum.GetNames(typeof(EstadoEmpleado));  
                }
                if (options.Filters.Contains(EmpleadosFilterOptions.Ubicaciones))
                {
                    filters.Ubicaciones =  JArray.FromObject(db.Ciudades.Where(c => c.Domicilios.Any(d => d.Empleados.Any(e => e.EmpresaId == empresaId))).Select(c => new ValorFiltroViewModel()
                    {
                        Id = c.Id,
                        Valor = c.Id.ToString(),
                        Descripcion = c.Nombre,
                        Cantidad = db.Empleadoes.Count(e => e.Domicilio.CiudadId == c.Id && e.EmpresaId == empresaId)
                    }).ToList());
                }
                if (options.Filters.Contains(EmpleadosFilterOptions.Rubros))
                {
                    filters.Rubros = JArray.FromObject(db.SubRubros
                        .Where(s => s.Profesionales.Any(p=> p.Empleados.Any(e=> e.EmpresaId == empresaId)))
                                                    .Select(a => new ValorFiltroViewModel()
                                                    {
                                                        Id = a.Id,
                                                        Valor = a.Id.ToString(),
                                                        Descripcion = a.Nombre,
                                                        Cantidad = db.Profesionals
                                                                .Count(p => p.Subrubros.Any(s => s.Id == a.Id))
                                                    })
                                                    .ToList());
                }
                if (options.Filters.Contains(EmpleadosFilterOptions.Valoraciones))
                {
                    var valoracionProfesional = new List<ValorFiltroViewModel>();
                    for (var i = 1; i < 6; i++)
                    {
                        valoracionProfesional.Add(new ValorFiltroViewModel()
                        {
                            Id = i,
                            Valor = i.ToString(),
                            Descripcion = i.ToString(),
                            Cantidad = db.Profesionals.Where(p=> p.Empleados.Any(e=> e.EmpresaId == empresaId)).Count(p => p.ValoracionPromedio >= i)
                        });
                    }
                    filters.Valoraciones = JArray.FromObject(valoracionProfesional);
                }
            }


            return Ok(new
            {
                options = new
                {
                    selectableFilters = filters,
                    allFilterTypes = Enum.GetNames(typeof(EmpleadosFilterOptions)),
                    orderByOptions = Enum.GetNames(typeof(EmpleadosOrderByOptions)),
                },
                query = new
                {
                    orderBy = "",
                    searchText = "",
                    Sexos = new List<string>(),
                    Estados = new List<string>(),
                    Ubicaciones = new List<string>(),
                }
            });
        }

        [HttpPost]
        [Route("api/Empleados/Search")]
        public IHttpActionResult Search(EmpleadosQueryBindingModel queryOptions)
        {
            if (queryOptions == null)
            {
                return BadRequest("no query options provided");
            }

            if (User == null)
            {
                return BadRequest("Debe ser una empresa para poder consultar el listado de empleados.");
            }
            var empresaId = Utiles.GetEmpresaId(User.Identity.GetUserId());

            //create the initial query...
            var query = db.Empleadoes.Where(e=> e.EmpresaId == empresaId);

            //for each query option if it has values add it to the query
            if (!string.IsNullOrEmpty(queryOptions.SearchText))
            {
                query = query.Where(p => p.Nombre.Contains(queryOptions.SearchText) || p.Apellido.Contains(queryOptions.SearchText) || p.Legajo.Contains(queryOptions.SearchText));
            }

            if (queryOptions.Ubicaciones != null && queryOptions.Ubicaciones.Any())
            {
                query = query.Where(e => queryOptions.Ubicaciones.Contains((int) e.Domicilio.CiudadId));
            }

            if (queryOptions.Sexos != null && queryOptions.Sexos.Any())
            {
                query = query.Where(e => queryOptions.Sexos.Contains(e.Sexo));

            }

            if (queryOptions.Rubros != null && queryOptions.Rubros.Any())
            {
                query = query.Where(e => e.Profesional.Subrubros.Any(s => queryOptions.Rubros.Contains(s.Id)));
            }

            if (queryOptions.Valoraciones != null && queryOptions.Valoraciones.Any())
            {
                queryOptions.Valoraciones.Sort();
                var valMin = queryOptions.Valoraciones.FirstOrDefault();
                query = query.Where(e => e.Profesional.ValoracionPromedio >= valMin);
            }

            var activos = false;
            var baja = false;
            foreach (var estado in queryOptions.Estados)
            {
                switch (estado)
                {
                    case EstadoEmpleado.Activo:
                        {
                            activos = true;
                            break;
                        }
                    case EstadoEmpleado.Baja:
                        {
                            baja = true;
                            break;
                        }
                }
            }
            if (activos && !baja)
            {
                query = query.Where(e => e.FechaFinVigencia == null);
            }
            if (baja && !activos)
            {
                query = query.Where(e => e.FechaFinVigencia != null);
            }

            query = CreateOrderByExpression(query, queryOptions.OrderBy);

            var data = Utiles.Paginate(new PaginateQueryParameters(queryOptions.Page, queryOptions.Rows), query);
            return Ok(data);
        }

        private static IQueryable<Empleado> CreateOrderByExpression(IQueryable<Empleado> query, EmpleadosOrderByOptions orderByoption)
        {
            switch (orderByoption)
            {
                case EmpleadosOrderByOptions.Nombre:
                    query = query.OrderBy(p => p.Apellido).ThenBy(p=> p.Nombre);
                    break;
                case EmpleadosOrderByOptions.FechaInicioRelacionLaboral:
                    query = query.OrderBy(p => p.FechaInicioVigencia);
                    break;
                case EmpleadosOrderByOptions.Legajo:
                    query = query.OrderBy(p => p.Legajo);
                    break;
                default:
                    query = query.OrderBy(p => p.Apellido).ThenBy(p => p.Nombre);
                    break;
            }

            return query;
        }
    }
}