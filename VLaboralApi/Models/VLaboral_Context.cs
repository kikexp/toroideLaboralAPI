﻿using Microsoft.AspNet.Identity.EntityFramework;
using VlaboralApi.Infrastructure;

namespace VLaboralApi.Models
{
    public class VLaboral_Context : IdentityDbContext<ApplicationUser> // DbContext
    {
        public VLaboral_Context() : base("VLaboral_Context", throwIfV1Schema: false)
        {
            this.Configuration.LazyLoadingEnabled = false;
            this.Configuration.ProxyCreationEnabled = false;
        }

        #region Definicion de Tablas DbSet
        public System.Data.Entity.DbSet<VLaboralApi.Models.Empleado> Empleadoes { get; set; }

        public System.Data.Entity.DbSet<VLaboralApi.Models.TipoIdentificacionEmpleado> TipoIdentificacionEmpleado { get; set; }
        public System.Data.Entity.DbSet<VLaboralApi.Models.IdentificacionEmpleado> IdentificacionesEmpleado { get; set; }

        public System.Data.Entity.DbSet<VLaboralApi.Models.BlobUploadModel> BlobUploadModels { get; set; }
        public System.Data.Entity.DbSet<VLaboralApi.Models.Oferta> Ofertas { get; set; }

        public System.Data.Entity.DbSet<VLaboralApi.Models.Rubro> Rubroes { get; set; }

        public System.Data.Entity.DbSet<VLaboralApi.Models.SubRubro> SubRubros { get; set; }

        public System.Data.Entity.DbSet<VLaboralApi.Models.TipoDisponibilidad> TipoDisponibilidads { get; set; }

        public System.Data.Entity.DbSet<VLaboralApi.Models.TipoContrato> TipoContratoes { get; set; }

        public System.Data.Entity.DbSet<VLaboralApi.Models.TipoIdentificacionProfesional> TiposIdentificacionesProfesionales { get; set; }

        public System.Data.Entity.DbSet<VLaboralApi.Models.IdentificacionProfesional> IdentificacionesProfesional { get; set; }

        public System.Data.Entity.DbSet<VLaboralApi.Models.TipoIdentificacionEmpresa> TiposIdentificacionesEmpresas { get; set; }
        public System.Data.Entity.DbSet<VLaboralApi.Models.IdentificacionEmpresa> IdentificacionesEmpresa { get; set; }

        public System.Data.Entity.DbSet<VLaboralApi.Models.Profesional> Profesionals { get; set; }

        public System.Data.Entity.DbSet<VLaboralApi.Models.Habilidad> Habilidads { get; set; }

        public System.Data.Entity.DbSet<VLaboralApi.Models.Requisito> Requisitos { get; set; }
        public System.Data.Entity.DbSet<VLaboralApi.Models.ValoresRequisito> ValoresRequisitos { get; set; }
        
        public System.Data.Entity.DbSet<VLaboralApi.Models.TipoRequisito> TipoRequisitoes { get; set; }
        public System.Data.Entity.DbSet<VLaboralApi.Models.ValoresTipoRequisito> ValoresTipoRequisitos { get; set; }

        public System.Data.Entity.DbSet<VLaboralApi.Models.Empresa> Empresas { get; set; }
        public System.Data.Entity.DbSet<VLaboralApi.Models.Puesto> Puestos { get; set; }

        public System.Data.Entity.DbSet<VLaboralApi.Models.TipoEtapa> TiposEtapas { get; set; }

        public System.Data.Entity.DbSet<VLaboralApi.Models.Postulacion> Postulacions { get; set; }

        public System.Data.Entity.DbSet<VLaboralApi.Models.PuestoEtapaOferta> PuestoEtapaOfertas { get; set; }

        public System.Data.Entity.DbSet<VLaboralApi.Models.EtapaOferta> EtapasOfertas { get; set; }


        #endregion

        public static VLaboral_Context Create()
        {
            return new VLaboral_Context();
        }

        public System.Data.Entity.DbSet<VLaboralApi.Models.TipoNivelEstudio> TipoNivelEstudios { get; set; }

        public System.Data.Entity.DbSet<VLaboralApi.Models.Estudio> Estudios { get; set; }

        public System.Data.Entity.DbSet<VLaboralApi.Models.Educacion> Educacions { get; set; }

        public System.Data.Entity.DbSet<VLaboralApi.Models.Curso_Certificacion> Cursos { get; set; }

        public System.Data.Entity.DbSet<VLaboralApi.Models.ExperienciaLaboral> ExperienciaLaborals { get; set; }

        public System.Data.Entity.DbSet<VLaboralApi.Models.Idioma> Idiomas { get; set; }

        public System.Data.Entity.DbSet<VLaboralApi.Models.IdiomaConocido> IdiomaConocidoes { get; set; }

        public System.Data.Entity.DbSet<VLaboralApi.Models.CompetenciaIdioma> CompetenciaIdiomas { get; set; }

        public System.Data.Entity.DbSet<VLaboralApi.Models.Notificacion> Notificaciones { get; set; }

        public System.Data.Entity.DbSet<VLaboralApi.Models.TipoNotificacion> TipoNotificaciones { get; set; }

        public System.Data.Entity.DbSet<VLaboralApi.Models.VerificacionExperienciaLaboral> VerificacionExperienciaLaborals { get; set; }

        public System.Data.Entity.DbSet<VLaboralApi.Models.Ubicacion.Pais> Paises { get; set; }

        public System.Data.Entity.DbSet<VLaboralApi.Models.Ubicacion.Provincia> Provincias { get; set; }

        public System.Data.Entity.DbSet<VLaboralApi.Models.Ubicacion.Ciudad> Ciudades { get; set; }

        public System.Data.Entity.DbSet<VLaboralApi.Models.Ubicacion.Domicilio> Domicilios { get; set; }

       
    }


}

