using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SIRPSI.ModelsJ;

public partial class SirpsiContext : DbContext
{
    public SirpsiContext()
    {
    }

    public SirpsiContext(DbContextOptions<SirpsiContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AspNetRole> AspNetRoles { get; set; }

    public virtual DbSet<AspNetRoleClaim> AspNetRoleClaims { get; set; }

    public virtual DbSet<AspNetUser> AspNetUsers { get; set; }

    public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }

    public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }

    public virtual DbSet<AspNetUserRole> AspNetUserRoles { get; set; }

    public virtual DbSet<AspNetUserRole1> AspNetUserRoles1 { get; set; }

    public virtual DbSet<AspNetUserToken> AspNetUserTokens { get; set; }

    public virtual DbSet<Empresa> Empresas { get; set; }

    public virtual DbSet<Estado> Estados { get; set; }

    public virtual DbSet<Pai> Pais { get; set; }

    public virtual DbSet<TiposDocumento> TiposDocumentos { get; set; }

    public virtual DbSet<TiposEmpresa> TiposEmpresas { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=SIRPSI;Trusted_Connection=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AspNetRole>(entity =>
        {
            entity.HasIndex(e => e.NormalizedName, "RoleNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedName] IS NOT NULL)");

            entity.Property(e => e.Name).HasMaxLength(256);
            entity.Property(e => e.NormalizedName).HasMaxLength(256);
        });

        modelBuilder.Entity<AspNetRoleClaim>(entity =>
        {
            entity.HasIndex(e => e.RoleId, "IX_AspNetRoleClaims_RoleId");

            entity.HasOne(d => d.Role).WithMany(p => p.AspNetRoleClaims).HasForeignKey(d => d.RoleId);
        });

        modelBuilder.Entity<AspNetUser>(entity =>
        {
            entity.HasIndex(e => e.NormalizedEmail, "EmailIndex");

            entity.HasIndex(e => e.NormalizedUserName, "UserNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedUserName] IS NOT NULL)");

            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
            entity.Property(e => e.NormalizedUserName).HasMaxLength(256);
            entity.Property(e => e.UserName).HasMaxLength(256);
        });

        modelBuilder.Entity<AspNetUserClaim>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_AspNetUserClaims_UserId");

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserClaims).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserLogin>(entity =>
        {
            entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

            entity.HasIndex(e => e.UserId, "IX_AspNetUserLogins_UserId");

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserLogins).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserRole>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RoleId });

            entity.HasIndex(e => e.RoleId, "IX_AspNetUserRoles_RoleId");

            entity.HasOne(d => d.Role).WithMany(p => p.AspNetUserRoles).HasForeignKey(d => d.RoleId);

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserRoles).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserRole1>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AspNetUs__3214EC07D5925D52");

            entity.ToTable("AspNetUserRoles", "sirpsi");

            entity.Property(e => e.FechaModifico).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");
            entity.Property(e => e.IdEstado).HasMaxLength(450);
            entity.Property(e => e.RoleId).HasMaxLength(450);
            entity.Property(e => e.UserId).HasMaxLength(450);
            entity.Property(e => e.UsuarioModifico).HasMaxLength(450);
            entity.Property(e => e.UsuarioRegistro).HasMaxLength(450);

            entity.HasOne(d => d.IdEstadoNavigation).WithMany(p => p.AspNetUserRole1s)
                .HasForeignKey(d => d.IdEstado)
                .HasConstraintName("FK__AspNetUse__IdEst__04E4BC85");

            entity.HasOne(d => d.Role).WithMany(p => p.AspNetUserRole1s)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__AspNetUse__RoleI__03F0984C");

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserRole1s)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__AspNetUse__UserI__02FC7413");
        });

        modelBuilder.Entity<AspNetUserToken>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserTokens).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<Empresa>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Empresas__3214EC079DE63491");

            entity.ToTable("Empresas", "sirpsi");

            entity.Property(e => e.Descripcion).HasMaxLength(500);
            entity.Property(e => e.DigitoVerificacion).HasMaxLength(10);
            entity.Property(e => e.Documento).HasMaxLength(100);
            entity.Property(e => e.FechaModifico).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");
            entity.Property(e => e.IdConsecutivo).ValueGeneratedOnAdd();
            entity.Property(e => e.IdEstado).HasMaxLength(450);
            entity.Property(e => e.IdTipoEmpresa).HasMaxLength(450);
            entity.Property(e => e.Nombre).HasMaxLength(300);
            entity.Property(e => e.TipoDocumento).HasMaxLength(450);
            entity.Property(e => e.UsuarioModifico).HasMaxLength(450);
            entity.Property(e => e.UsuarioRegistro).HasMaxLength(450);

            entity.HasOne(d => d.IdTipoEmpresaNavigation).WithMany(p => p.Empresas)
                .HasForeignKey(d => d.IdTipoEmpresa)
                .HasConstraintName("FK__Empresas__IdTipo__4E88ABD4");
        });

        modelBuilder.Entity<Estado>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Estados__3214EC072AEC3263");

            entity.ToTable("Estados", "sirpsi");

            entity.Property(e => e.FechaModifico).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");
            entity.Property(e => e.IdConsecutivo).ValueGeneratedOnAdd();
            entity.Property(e => e.Nombre).HasMaxLength(500);
            entity.Property(e => e.UsuarioModifico).HasMaxLength(450);
            entity.Property(e => e.UsuarioRegistro).HasMaxLength(450);
        });

        modelBuilder.Entity<Pai>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Pais__3214EC07893CE663");

            entity.ToTable("Pais", "sirpsi");

            entity.Property(e => e.FechaModifico).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");
            entity.Property(e => e.IdConsecutivo).ValueGeneratedOnAdd();
            entity.Property(e => e.IdEstado).HasMaxLength(450);
            entity.Property(e => e.Nombre).HasMaxLength(500);
            entity.Property(e => e.UsuarioModifico).HasMaxLength(450);
            entity.Property(e => e.UsuarioRegistro).HasMaxLength(450);
        });

        modelBuilder.Entity<TiposDocumento>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TiposDoc__3214EC07FE50A2DC");

            entity.ToTable("TiposDocumento", "sirpsi");

            entity.Property(e => e.FechaModifico).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");
            entity.Property(e => e.IdConsecutivo).ValueGeneratedOnAdd();
            entity.Property(e => e.IdEstado).HasMaxLength(450);
            entity.Property(e => e.Nombre).HasMaxLength(500);
            entity.Property(e => e.UsuarioModifico).HasMaxLength(450);
            entity.Property(e => e.UsuarioRegistro).HasMaxLength(450);
        });

        modelBuilder.Entity<TiposEmpresa>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TiposEmp__3214EC07F94F6980");

            entity.ToTable("TiposEmpresa", "sirpsi");

            entity.Property(e => e.FechaModifico).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");
            entity.Property(e => e.IdConsecutivo).ValueGeneratedOnAdd();
            entity.Property(e => e.IdEstado).HasMaxLength(450);
            entity.Property(e => e.Nombre).HasMaxLength(500);
            entity.Property(e => e.UsuarioModifico).HasMaxLength(450);
            entity.Property(e => e.UsuarioRegistro).HasMaxLength(450);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
