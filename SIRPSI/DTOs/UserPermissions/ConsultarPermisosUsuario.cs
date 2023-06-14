namespace SIRPSI.DTOs.UserPermissions
{
    public class ConsultarPermisosUsuario
    {
        public string Id { get; set; } = null!;
        public string? Vista { get; set; }
        public string IdUsuario { get; set; } = null!;
        public string IdEmpresa { get; set; } = null!;
        public bool Consulta { get; set; }
        public bool Registrar { get; set; }
        public bool Actualizar { get; set; }
        public bool Eliminar { get; set; }
        public bool Reportes { get; set; }
        public string? IdEstado { get; set; }
    }
}
