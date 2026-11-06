using Data.Context;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Services
{
    public class SlaAlertBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<SlaAlertBackgroundService> _logger;
        private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

        public SlaAlertBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<SlaAlertBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("SlaAlertBackgroundService démarré.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await VerifierSlasAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur lors de la vérification des SLA.");
                }

                await Task.Delay(Interval, stoppingToken);
            }
        }

        private async Task VerifierSlasAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var now = DateTime.Now;
            var statutsActifs = new[] { "En attente", "Reçu", "En cours", "Contre-analyse" };

            var demandesEnRetard = await context.Demandes
                .Where(d => statutsActifs.Contains(d.Statut)
                         && d.DateRenduEstimee != null
                         && d.DateRenduEstimee < now)
                .ToListAsync();

            foreach (var demande in demandesEnRetard)
            {
                // Notifier le technicien assigné
                if (demande.TechnicienId.HasValue)
                {
                    var dejaNotifie = await context.Notifications.AnyAsync(n =>
                        n.UtilisateurId == demande.TechnicienId.Value &&
                        n.DemandeId == demande.Id &&
                        n.Titre.Contains("SLA") &&
                        n.DateCreation > now.AddHours(-24));

                    if (!dejaNotifie)
                    {
                        var retardH = (int)(now - demande.DateRenduEstimee!.Value).TotalHours;
                        context.Notifications.Add(new Notification
                        {
                            UtilisateurId = demande.TechnicienId.Value,
                            Titre = "SLA dépassé — Action urgente requise",
                            Message = $"La demande {demande.Reference} ({demande.TypeProduit}) dépasse le délai de {demande.SlaHeures}h — retard de {retardH}h.",
                            Type = "danger",
                            DemandeId = demande.Id,
                            EstLue = false,
                            DateCreation = now
                        });
                    }
                }

                // Notifier les responsables labo
                var responsables = await context.Utilisateurs
                    .Where(u => u.Role == "ResponsableLabo")
                    .ToListAsync();

                foreach (var responsable in responsables)
                {
                    var dejaNotifieResp = await context.Notifications.AnyAsync(n =>
                        n.UtilisateurId == responsable.Id &&
                        n.DemandeId == demande.Id &&
                        n.Titre.Contains("SLA") &&
                        n.DateCreation > now.AddHours(-24));

                    if (!dejaNotifieResp)
                    {
                        var retardH = (int)(now - demande.DateRenduEstimee!.Value).TotalHours;
                        context.Notifications.Add(new Notification
                        {
                            UtilisateurId = responsable.Id,
                            Titre = "SLA dépassé — Intervention requise",
                            Message = $"Demande {demande.Reference} en retard de {retardH}h. Technicien : #{demande.TechnicienId?.ToString() ?? "non assigné"}.",
                            Type = "warning",
                            DemandeId = demande.Id,
                            EstLue = false,
                            DateCreation = now
                        });
                    }
                }
            }

            if (demandesEnRetard.Any())
            {
                await context.SaveChangesAsync();
                _logger.LogInformation("{Count} demande(s) en retard SLA détectée(s).", demandesEnRetard.Count);
            }
        }
    }
}
