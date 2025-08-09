using BookRadar.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace BookRadar.Web.Data
{
    // .NET 8: DbContext con constructor primario
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        // DbSets
        public DbSet<HistorialBusqueda> HistorialBusquedas => Set<HistorialBusqueda>();
        public DbSet<LogHistorialBusqueda> LogHistorialBusquedas => Set<LogHistorialBusqueda>();
        // public DbSet<TblUsuario> TblUsuarios => Set<TblUsuario>(); // habilítalo si usarás EF para usuarios

        protected override void OnModelCreating(ModelBuilder mb)
        {
            base.OnModelCreating(mb);

            /* ---------------------------
             * dbo.HistorialBusquedas
             * --------------------------- */
            mb.Entity<HistorialBusqueda>(e =>
            {
                e.ToTable("HistorialBusquedas", "dbo");
                e.HasKey(x => x.Id);

                e.Property(x => x.Autor)
                    .HasMaxLength(200)
                    .IsRequired();

                e.Property(x => x.Titulo)
                    .HasMaxLength(400)
                    .IsRequired();

                e.Property(x => x.AnioPublicacion);

                e.Property(x => x.Editorial)
                    .HasMaxLength(200);

                e.Property(x => x.FechaConsulta)
                    .HasColumnType("datetime2(0)")
                    .HasDefaultValueSql("SYSUTCDATETIME()");

                // Índice funcional para consultas por autor/fecha (refleja IX_HB_Autor_Fecha)
                e.HasIndex(x => new { x.Autor, x.FechaConsulta })
                 .HasDatabaseName("IX_HB_Autor_Fecha");

                // Sombras para columnas calculadas persistidas (no se envían en INSERT/UPDATE)
                e.Property<DateTime>("FechaConsultaMin")
                    .HasColumnType("datetime2(0)")
                    .ValueGeneratedOnAddOrUpdate();
                SetIgnoreAfterSave(e, "FechaConsultaMin");

                e.Property<int>("AnioPublicacion_NN")
                    .ValueGeneratedOnAddOrUpdate();
                SetIgnoreAfterSave(e, "AnioPublicacion_NN");

                e.Property<string>("Editorial_NN")
                    .HasMaxLength(200)
                    .ValueGeneratedOnAddOrUpdate();
                SetIgnoreAfterSave(e, "Editorial_NN");
            });

            /* ---------------------------
             * dbo.log_HistorialBusquedas
             * --------------------------- */
            mb.Entity<LogHistorialBusqueda>(e =>
            {
                e.ToTable("log_HistorialBusquedas", "dbo");
                e.HasKey(x => x.Id);

                e.Property(x => x.Mensaje)
                    .IsRequired(); // NVARCHAR(MAX)

                e.Property(x => x.FechaCreacion)
                    .HasColumnName("fecha_creacion")
                    .HasColumnType("datetime2(0)")
                    .HasDefaultValueSql("SYSUTCDATETIME()");
            });

        }

        // Helper para marcar sombras calculadas como "ignoradas después de guardar"
        private static void SetIgnoreAfterSave<TEntity>(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<TEntity> e, string name)
            where TEntity : class
        {
            var prop = e.Metadata.FindProperty(name);
            if (prop is not null)
            {
                prop.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            }
        }
    }
}
