using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SistemaAlquilerHerramientas.Models;

public partial class AlquilerHerramientasContext : DbContext
{
    public AlquilerHerramientasContext()
    {
    }

    public AlquilerHerramientasContext(DbContextOptions<AlquilerHerramientasContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Alquiler> Alquilers { get; set; }

    public virtual DbSet<CategoriaHerramientum> CategoriaHerramienta { get; set; }

    public virtual DbSet<Cliente> Clientes { get; set; }

    public virtual DbSet<Devolucion> Devolucions { get; set; }

    public virtual DbSet<Herramientum> Herramienta { get; set; }

    public virtual DbSet<Mora> Moras { get; set; }

    public virtual DbSet<Proveedor> Proveedors { get; set; }

    public virtual DbSet<Reserva> Reservas { get; set; }

    public virtual DbSet<Rol> Rols { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("Name=ConnectionStrings:conexion");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Alquiler>(entity =>
        {
            entity.HasKey(e => e.IdAlquiler).HasName("PK__Alquiler__085FBAE156632A2E");

            entity.ToTable("Alquiler");

            entity.Property(e => e.IdAlquiler).HasColumnName("idAlquiler");
            entity.Property(e => e.EstadoAlquiler)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("estadoAlquiler");
            entity.Property(e => e.FechaDevolucionPactada)
                .HasColumnType("datetime")
                .HasColumnName("fechaDevolucionPactada");
            entity.Property(e => e.FechaEntrega)
                .HasColumnType("datetime")
                .HasColumnName("fechaEntrega");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fechaRegistro");
            entity.Property(e => e.IdCliente).HasColumnName("idCliente");
            entity.Property(e => e.IdHerramienta).HasColumnName("idHerramienta");
            entity.Property(e => e.IdReserva).HasColumnName("idReserva");
            entity.Property(e => e.MontoEstimado)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("montoEstimado");

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.Alquilers)
                .HasForeignKey(d => d.IdCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Alquiler_Cliente");

            entity.HasOne(d => d.IdHerramientaNavigation).WithMany(p => p.Alquilers)
                .HasForeignKey(d => d.IdHerramienta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Alquiler_Herramienta");

            entity.HasOne(d => d.IdReservaNavigation).WithMany(p => p.Alquilers)
                .HasForeignKey(d => d.IdReserva)
                .HasConstraintName("FK_Alquiler_Reserva");
        });

        modelBuilder.Entity<CategoriaHerramientum>(entity =>
        {
            entity.HasKey(e => e.IdCategoria).HasName("PK__Categori__8A3D240C750DB4A7");

            entity.Property(e => e.IdCategoria).HasColumnName("idCategoria");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("descripcion");
            entity.Property(e => e.Estado)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("estado");
            entity.Property(e => e.NombreCategoria)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombreCategoria");
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.IdCliente).HasName("PK__Cliente__885457EE0C0FB5EE");

            entity.ToTable("Cliente");

            entity.Property(e => e.IdCliente).HasColumnName("idCliente");
            entity.Property(e => e.Apellidos)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("apellidos");
            entity.Property(e => e.Correo)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("correo");
            entity.Property(e => e.Direccion)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("direccion");
            entity.Property(e => e.Dni)
                .HasMaxLength(8)
                .IsUnicode(false)
                .HasColumnName("dni");
            entity.Property(e => e.Estado)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("estado");
            entity.Property(e => e.Nombres)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombres");
            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("telefono");
        });

        modelBuilder.Entity<Devolucion>(entity =>
        {
            entity.HasKey(e => e.IdDevolucion).HasName("PK__Devoluci__BFAF069ACD49D5D8");

            entity.ToTable("Devolucion");

            entity.Property(e => e.IdDevolucion).HasColumnName("idDevolucion");
            entity.Property(e => e.EstadoRetorno)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("estadoRetorno");
            entity.Property(e => e.FechaDevolucionReal)
                .HasColumnType("datetime")
                .HasColumnName("fechaDevolucionReal");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fechaRegistro");
            entity.Property(e => e.IdAlquiler).HasColumnName("idAlquiler");
            entity.Property(e => e.Observacion)
                .IsUnicode(false)
                .HasColumnName("observacion");

            entity.HasOne(d => d.IdAlquilerNavigation).WithMany(p => p.Devolucions)
                .HasForeignKey(d => d.IdAlquiler)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Devolucion_Alquiler");
        });

        modelBuilder.Entity<Herramientum>(entity =>
        {
            entity.HasKey(e => e.IdHerramienta).HasName("PK__Herramie__D4582246E6DF530B");

            entity.Property(e => e.IdHerramienta).HasColumnName("idHerramienta");
            entity.Property(e => e.Descripcion)
                .IsUnicode(false)
                .HasColumnName("descripcion");
            entity.Property(e => e.EstadoHerramienta)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("estadoHerramienta");
            entity.Property(e => e.IdCategoria).HasColumnName("idCategoria");
            entity.Property(e => e.IdProveedor).HasColumnName("idProveedor");
            entity.Property(e => e.Nombre)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.PrecioPorDia)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("precioPorDia");

            entity.HasOne(d => d.IdCategoriaNavigation).WithMany(p => p.Herramienta)
                .HasForeignKey(d => d.IdCategoria)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Herramienta_Categoria");

            entity.HasOne(d => d.IdProveedorNavigation).WithMany(p => p.Herramienta)
                .HasForeignKey(d => d.IdProveedor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Herramienta_Proveedor");
        });

        modelBuilder.Entity<Mora>(entity =>
        {
            entity.HasKey(e => e.IdMora).HasName("PK__Mora__C0D065726280D8DB");

            entity.ToTable("Mora");

            entity.Property(e => e.IdMora).HasColumnName("idMora");
            entity.Property(e => e.DiasRetraso).HasColumnName("diasRetraso");
            entity.Property(e => e.EstadoPago)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("estadoPago");
            entity.Property(e => e.IdAlquiler).HasColumnName("idAlquiler");
            entity.Property(e => e.MontoMora)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("montoMora");

            entity.HasOne(d => d.IdAlquilerNavigation).WithMany(p => p.Moras)
                .HasForeignKey(d => d.IdAlquiler)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Mora_Alquiler");
        });

        modelBuilder.Entity<Proveedor>(entity =>
        {
            entity.HasKey(e => e.IdProveedor).HasName("PK__Proveedo__A3FA8E6B15330267");

            entity.ToTable("Proveedor");

            entity.Property(e => e.IdProveedor).HasColumnName("idProveedor");
            entity.Property(e => e.Correo)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("correo");
            entity.Property(e => e.Direccion)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("direccion");
            entity.Property(e => e.Estado)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("estado");
            entity.Property(e => e.RazonSocial)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("razonSocial");
            entity.Property(e => e.Ruc)
                .HasMaxLength(11)
                .IsUnicode(false)
                .HasColumnName("ruc");
            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("telefono");
        });

        modelBuilder.Entity<Reserva>(entity =>
        {
            entity.HasKey(e => e.IdReserva).HasName("PK__Reserva__94D104C831651A2A");

            entity.ToTable("Reserva");

            entity.Property(e => e.IdReserva).HasColumnName("idReserva");
            entity.Property(e => e.EstadoReserva)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("estadoReserva");
            entity.Property(e => e.FechaDevolucionEstimada)
                .HasColumnType("datetime")
                .HasColumnName("fechaDevolucionEstimada");
            entity.Property(e => e.FechaInicio)
                .HasColumnType("datetime")
                .HasColumnName("fechaInicio");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fechaRegistro");
            entity.Property(e => e.IdCliente).HasColumnName("idCliente");
            entity.Property(e => e.IdHerramienta).HasColumnName("idHerramienta");

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.Reservas)
                .HasForeignKey(d => d.IdCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reserva_Cliente");

            entity.HasOne(d => d.IdHerramientaNavigation).WithMany(p => p.Reservas)
                .HasForeignKey(d => d.IdHerramienta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reserva_Herramienta");
        });

        modelBuilder.Entity<Rol>(entity =>
        {
            entity.HasKey(e => e.IdRol).HasName("PK__Rol__3C872F76449E958B");

            entity.ToTable("Rol");

            entity.Property(e => e.IdRol).HasColumnName("idRol");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("descripcion");
            entity.Property(e => e.NombreRol)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombreRol");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PK__Usuario__645723A640CAE5BC");

            entity.ToTable("Usuario");

            entity.Property(e => e.IdUsuario).HasColumnName("idUsuario");
            entity.Property(e => e.Contrasena)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("contrasena");
            entity.Property(e => e.Correo)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("correo");
            entity.Property(e => e.Estado)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("estado");
            entity.Property(e => e.IdRol).HasColumnName("idRol");
            entity.Property(e => e.Nombre)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.Usuario1)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("usuario");

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdRol)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Usuario_Rol");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
