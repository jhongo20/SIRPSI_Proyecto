using AutoMapper;
using DataAccess.Models.Companies;
using DataAccess.Models.Country;
using DataAccess.Models.Documents;
using DataAccess.Models.Estados;
using DataAccess.Models.Rols;
using DataAccess.Models.Status;
using DataAccess.Models.Users;
using SIRPSI.DTOs.Companies;
using SIRPSI.DTOs.Country;
using SIRPSI.DTOs.Document;
using SIRPSI.DTOs.Status;
using SIRPSI.DTOs.User;
using SIRPSI.DTOs.User.Roles;
using SIRPSI.DTOs.User.RolesUsuario;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SIRPSI.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            #region Usuario
            //Mapeo de la clase usuario
            CreateMap<AspNetUsers, UserCredentials>().ReverseMap();

            #endregion

            #region Roles

            CreateMap<Roles, ConsultarRoles>().ReverseMap();
            CreateMap<Roles, RegistrarRol>().ReverseMap();

            #endregion

            #region Roles de usuario

            CreateMap<UserRoles, ConsultarRolesUsuario>().ReverseMap();
            CreateMap<UserRoles, RegistrarRolesUsuario>().ReverseMap();
            CreateMap<UserRoles, EditarRolesUsuario>().ReverseMap();
            CreateMap<UserRoles, EliminarRolesUsuario>().ReverseMap();

            #endregion

            #region Estados

            CreateMap<Estados, ConsultarEstados>().ReverseMap();
            CreateMap<Estados, RegistrarEstados>().ReverseMap();
            CreateMap<Estados, EditarEstados>().ReverseMap();
            CreateMap<Estados, EliminarEstados>().ReverseMap();

            #endregion

            #region Empresas

            CreateMap<Empresas, ConsultarEmpresas>().ReverseMap();
            CreateMap<Empresas, RegistrarEmpresas>().ReverseMap();
            CreateMap<Empresas, EditarEmpresas>().ReverseMap();
            CreateMap<Empresas, EliminarEmpresas>().ReverseMap();

            CreateMap<TiposEmpresa, ConsultarTipoEmpresa>().ReverseMap();
            CreateMap<TiposEmpresa, RegistrarTipoEmpresa>().ReverseMap();
            CreateMap<TiposEmpresa, EditarTipoEmpresa>().ReverseMap();
            CreateMap<TiposEmpresa, EliminarTipoEmpresa>().ReverseMap();

            #endregion

            #region País
            CreateMap<Pais, ConsultarPaises>().ReverseMap();
            CreateMap<Pais, RegistrarPais>().ReverseMap();
            CreateMap<Pais, EditarPais>().ReverseMap();
            CreateMap<Pais, EliminarPais>().ReverseMap();
            #endregion

            #region Documentos
            CreateMap<TiposDocumento, ConsultarTiposDocumento>().ReverseMap();
            CreateMap<TiposDocumento, RegistrarTipoDocumento>().ReverseMap();
            CreateMap<TiposDocumento, EditarTipoDocumento>().ReverseMap();
            CreateMap<TiposDocumento, EliminarTipoDocumento>().ReverseMap();
            #endregion

        }
    }
}
