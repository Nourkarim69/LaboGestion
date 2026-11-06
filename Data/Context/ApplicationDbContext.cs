using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Data.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Utilisateur> Utilisateurs { get; set; }
        public DbSet<Echantillon> Echantillons { get; set; }
        public DbSet<Analyse> Analyses { get; set; }
        public DbSet<Resultat> Resultats { get; set; }
        public DbSet<Filiale> Filiales { get; set; }
        public DbSet<Demande> Demandes { get; set; }
        public DbSet<CleAcces> ClesAcces { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Produit> Produits { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Produit>()
    .HasOne(p => p.Filiale)
    .WithMany()
    .HasForeignKey(p => p.FilialeId)
    .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Resultat>()
                .HasOne(r => r.Echantillon)
                .WithMany()
                .HasForeignKey(r => r.EchantillonId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Resultat>()
                .HasOne(r => r.Analyse)
                .WithMany()
                .HasForeignKey(r => r.AnalyseId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Resultat>()
                .HasOne(r => r.Utilisateur)
                .WithMany()
                .HasForeignKey(r => r.UtilisateurId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Echantillon>()
                .HasOne(e => e.Utilisateur)
                .WithMany()
                .HasForeignKey(e => e.UtilisateurId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Utilisateur>()
                .HasOne(u => u.Filiale)
                .WithMany()
                .HasForeignKey(u => u.FilialeId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Demande>()
                .HasOne(d => d.Filiale)
                .WithMany()
                .HasForeignKey(d => d.FilialeId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Demande>()
                .HasOne(d => d.Utilisateur)
                .WithMany()
                .HasForeignKey(d => d.UtilisateurId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Demande>()
                .HasOne(d => d.Technicien)
                .WithMany()
                .HasForeignKey(d => d.TechnicienId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Demande>()
                .HasOne(d => d.ValideParResponsable)
                .WithMany()
                .HasForeignKey(d => d.ValideParResponsableId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Demande>()
                .HasOne(d => d.ValideParDirecteur)
                .WithMany()
                .HasForeignKey(d => d.ValideParDirecteurId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<CleAcces>()
                .HasOne(c => c.Filiale)
                .WithMany()
                .HasForeignKey(c => c.FilialeId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Utilisateur)
                .WithMany()
                .HasForeignKey(n => n.UtilisateurId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Demande)
                .WithMany()
                .HasForeignKey(n => n.DemandeId)
                .OnDelete(DeleteBehavior.NoAction);

            // ── Message ───────────────────────────────────────
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Expediteur)
                .WithMany()
                .HasForeignKey(m => m.ExpediteurId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Destinataire)
                .WithMany()
                .HasForeignKey(m => m.DestinataireId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Demande)
                .WithMany()
                .HasForeignKey(m => m.DemandeId)
                .OnDelete(DeleteBehavior.NoAction);

            // ── AuditLog ──────────────────────────────────────
            modelBuilder.Entity<AuditLog>()
                .Property(a => a.DateAction)
                .HasDefaultValueSql("GETDATE()");

            // ── Demande — champs décimaux ─────────────────────
            modelBuilder.Entity<Demande>()
                .Property(d => d.SlaHeures)
                .HasDefaultValue(48);

            modelBuilder.Entity<Demande>()
                .Property(d => d.LotBloque)
                .HasDefaultValue(false);

            modelBuilder.Entity<Demande>()
                .Property(d => d.ContreAnalyseRequise)
                .HasDefaultValue(false);
        }
    }
}