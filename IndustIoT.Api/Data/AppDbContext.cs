using IndustIoT.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace IndustIoT.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Equipamento> Equipamentos => Set<Equipamento>();
    public DbSet<LeituraSensor> LeiturasSensor => Set<LeituraSensor>();
    public DbSet<Alerta> Alertas => Set<Alerta>();
    public DbSet<ComandoAtuacao> ComandosAtuacao => Set<ComandoAtuacao>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Equipamento>(e =>
        {
            e.Property(p => p.Tag).HasMaxLength(20).IsRequired();
            e.HasIndex(p => p.Tag).IsUnique();

            e.Property(p => p.Nome).HasMaxLength(150).IsRequired();
            e.Property(p => p.Fabricante).HasMaxLength(100);
            e.Property(p => p.Modelo).HasMaxLength(100);
            e.Property(p => p.Setor).HasMaxLength(100).IsRequired();
            e.Property(p => p.LinhaProducao).HasMaxLength(50);
            e.Property(p => p.PotenciaNominalWatts).HasPrecision(12, 2);
            e.Property(p => p.LimiteConsumoKwh).HasPrecision(12, 4);
            e.HasIndex(p => p.Setor);
        });

        modelBuilder.Entity<LeituraSensor>(e =>
        {
            e.Property(p => p.ConsumoKwh).HasPrecision(12, 4);
            e.Property(p => p.TemperaturaCelsius).HasPrecision(8, 2);
            e.Property(p => p.VibracaoMmPorSegundo).HasPrecision(8, 2);
            e.Property(p => p.CorrenteAmperes).HasPrecision(8, 2);

            // Índice composto que sustenta a consulta paginada de consumo
            // por equipamento ordenada por data — evita scan em tabelas grandes.
            e.HasIndex(p => new { p.EquipamentoId, p.DataHoraLeitura })
                .IsDescending(false, true);

            e.HasOne(p => p.Equipamento)
                .WithMany(eq => eq.Leituras)
                .HasForeignKey(p => p.EquipamentoId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Alerta>(e =>
        {
            e.Property(p => p.ConsumoRegistradoKwh).HasPrecision(12, 4);
            e.Property(p => p.LimiteConfiguradoKwh).HasPrecision(12, 4);
            e.Property(p => p.Mensagem).HasMaxLength(500).IsRequired();

            // Listagem de alertas é filtrada por resolvido/severidade e ordenada por data.
            e.HasIndex(p => new { p.Resolvido, p.DataHoraCriacao });
            e.HasIndex(p => p.EquipamentoId);

            e.HasOne(p => p.Equipamento)
                .WithMany(eq => eq.Alertas)
                .HasForeignKey(p => p.EquipamentoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Restrict para não haver dois caminhos de cascade até LeituraSensor.
            e.HasOne(p => p.LeituraSensor)
                .WithMany()
                .HasForeignKey(p => p.LeituraSensorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ComandoAtuacao>(e =>
        {
            e.Property(p => p.SolicitadoPor).HasMaxLength(100).IsRequired();
            e.Property(p => p.Motivo).HasMaxLength(500);
            e.HasIndex(p => new { p.EquipamentoId, p.DataHoraSolicitacao });

            e.HasOne(p => p.Equipamento)
                .WithMany(eq => eq.Comandos)
                .HasForeignKey(p => p.EquipamentoId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
