using Data.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var demandes = await _context.Demandes.ToListAsync();
            var resultats = await _context.Resultats.ToListAsync();
            var filiales = await _context.Filiales.ToListAsync();
            var analyses = await _context.Analyses.ToListAsync();

            // ── Taux de conformité global ──────────────────────────
            var totalResultats = resultats.Count;
            var totalAnomalies = resultats.Count(r => r.EstAnomal);
            var tauxConformite = totalResultats > 0
                ? Math.Round((double)(totalResultats - totalAnomalies) / totalResultats * 100, 1)
                : 0;

            // ── Stats par filiale ──────────────────────────────────
            var statsByFiliale = filiales.Select(f => {
                var demandesFiliale = demandes.Where(d => d.FilialeId == f.Id).ToList();
                var resultatsByDemande = resultats.Count();
                var anomaliesFiliale = 0;
                var totalFiliale = 0;

                // Compter anomalies via echantillons liés à la filiale
                var totalDemandesFiliale = demandesFiliale.Count;
                var demandesTerminees = demandesFiliale.Count(d => d.Statut == "Terminé" || d.Statut == "Clôturé");

                // Délai moyen
                var demandesAvecDelai = demandesFiliale
                    .Where(d => d.DateReception.HasValue && d.DateRenduReelle.HasValue)
                    .ToList();

                var delaiMoyen = demandesAvecDelai.Any()
                    ? Math.Round(demandesAvecDelai.Average(d =>
                        (d.DateRenduReelle!.Value - d.DateReception!.Value).TotalDays), 1)
                    : 0;

                return new
                {
                    filialeId = f.Id,
                    filiale = f.Nom,
                    ville = f.Ville,
                    totalDemandes = totalDemandesFiliale,
                    demandesTerminees,
                    demandesEnCours = demandesFiliale.Count(d => d.Statut == "En cours"),
                    demandesEnAttente = demandesFiliale.Count(d => d.Statut == "En attente"),
                    delaiMoyenJours = delaiMoyen
                };
            }).ToList();

            // ── Stats par type de produit ──────────────────────────
            var statsByProduit = demandes
                .GroupBy(d => d.TypeProduit)
                .Select(g => new
                {
                    produit = g.Key,
                    total = g.Count(),
                    terminees = g.Count(d => d.Statut == "Terminé" || d.Statut == "Clôturé"),
                    enCours = g.Count(d => d.Statut == "En cours"),
                    enAttente = g.Count(d => d.Statut == "En attente")
                })
                .OrderByDescending(x => x.total)
                .ToList();

            // ── Évolution mensuelle des 6 derniers mois ────────────
            var sixMoisAvant = DateTime.Now.AddMonths(-6);
            var evolutionMensuelle = demandes
                .Where(d => d.DateDemande >= sixMoisAvant)
                .GroupBy(d => new { d.DateDemande.Year, d.DateDemande.Month })
                .Select(g => new
                {
                    annee = g.Key.Year,
                    mois = g.Key.Month,
                    label = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                    total = g.Count(),
                    terminees = g.Count(d => d.Statut == "Terminé" || d.Statut == "Clôturé")
                })
                .OrderBy(x => x.annee).ThenBy(x => x.mois)
                .ToList();

            // ── Délai moyen global ─────────────────────────────────
            var demandesGlobalesAvecDelai = demandes
                .Where(d => d.DateReception.HasValue && d.DateRenduReelle.HasValue)
                .ToList();

            var delaiMoyenGlobal = demandesGlobalesAvecDelai.Any()
                ? Math.Round(demandesGlobalesAvecDelai.Average(d =>
                    (d.DateRenduReelle!.Value - d.DateReception!.Value).TotalDays), 1)
                : 0;

            // ── Demandes en retard ─────────────────────────────────
            var demandesEnRetard = demandes.Count(d => {
                if (!d.DateRenduEstimee.HasValue) return false;
                if (d.Statut == "Terminé" || d.Statut == "Clôturé") return false;
                return d.DateRenduEstimee.Value < DateTime.Now;
            });

            return Ok(new
            {
                global = new
                {
                    totalDemandes = demandes.Count,
                    totalResultats,
                    totalAnomalies,
                    tauxConformite,
                    delaiMoyenJours = delaiMoyenGlobal,
                    demandesEnRetard,
                    demandesEnCours = demandes.Count(d => d.Statut == "En cours"),
                    demandesTerminees = demandes.Count(d => d.Statut == "Terminé" || d.Statut == "Clôturé")
                },
                parFiliale = statsByFiliale,
                parProduit = statsByProduit,
                evolutionMensuelle
            });
        }
    }
}