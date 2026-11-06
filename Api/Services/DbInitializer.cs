using Data.Context;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Api.Services
{
    public static class DbInitializer
    {
        public static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            return Convert.ToBase64String(sha256.ComputeHash(bytes));
        }

        public static async Task<(int inserted, int skipped, object diagnostic)> SeedDemoAccountsAsync(ApplicationDbContext context)
        {
            const string demoPassword = "Demo@1234";
            var hash = HashPassword(demoPassword);

            // ── 1. Trouver la filiale de démo ─────────────────────────────────────
            // Priorité : "yab" → "selja" → première dispo → créer "Selja"
            var filiale = await context.Filiales
                .FirstOrDefaultAsync(f => f.Nom.ToLower().Contains("yab"));

            if (filiale == null)
                filiale = await context.Filiales
                    .FirstOrDefaultAsync(f => f.Nom.ToLower().Contains("selja"));

            if (filiale == null)
                filiale = await context.Filiales.FirstOrDefaultAsync();

            if (filiale == null)
            {
                filiale = new Filiale
                {
                    Nom      = "Selja",
                    Adresse  = "Zone Industrielle",
                    Ville    = "Tunis",
                    Telephone = "71 000 000",
                    Email    = "selja@poulina.tn",
                    Produit  = "Lait — Beurre — Yaourt — Fromage",
                    EstActive = true
                };
                context.Filiales.Add(filiale);
                await context.SaveChangesAsync();
            }

            // ── 2. Seeder les produits si la filiale n'en a aucun ─────────────────
            // La table Produits est distincte du champ texte Filiale.Produit.
            // C'est elle qui alimente la liste déroulante "Sélectionner un produit".
            var nbProduits = await context.Produits
                .CountAsync(p => p.FilialeId == filiale.Id && p.Actif);

            if (nbProduits == 0)
            {
                // Dériver les noms depuis le champ texte Filiale.Produit (séparateur "," ou "—")
                // puis compléter avec un catalogue standard minimal.
                var nomsTexte = (filiale.Produit ?? "")
                    .Split(new[] { ',', '—', '-', ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(n => n.Trim())
                    .Where(n => n.Length > 0)
                    .ToList();

                // Catalogue standard Poulina — ajouté si absent du texte
                var catalogueStandard = new[] { "Beurre", "Yaourt nature", "Fromage frais", "Lait UHT" };
                foreach (var c in catalogueStandard)
                    if (!nomsTexte.Any(n => n.Equals(c, StringComparison.OrdinalIgnoreCase)))
                        nomsTexte.Add(c);

                var unitesParDefaut = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["Lait UHT"]           = "L",
                    ["Lait pasteurisé"]    = "L",
                    ["Lait écrémé"]        = "L",
                    ["Lait demi-écrémé"]   = "L",
                    ["Lait"]               = "L",
                    ["Yaourt nature"]      = "g",
                    ["Yaourt aromatisé"]   = "g",
                    ["Yaourt"]             = "g",
                    ["Fromage frais"]      = "g",
                    ["Fromage fondu"]      = "g",
                    ["Fromage"]            = "g",
                    ["Beurre"]             = "g",
                    ["Crème fraîche"]      = "g",
                    ["Crème glacée"]       = "g",
                    ["Crème dessert"]      = "g",
                    ["Jus de fruits"]      = "L",
                    ["Jus"]                = "L",
                    ["Nectar"]             = "L",
                    ["Pâtisserie industrielle"] = "g",
                    ["Viennoiserie"]       = "g",
                };

                foreach (var nom in nomsTexte)
                {
                    unitesParDefaut.TryGetValue(nom, out var unite);
                    context.Produits.Add(new Produit
                    {
                        Nom        = nom,
                        Description = "",
                        Unite      = unite ?? "unité",
                        Actif      = true,
                        FilialeId  = filiale.Id
                    });
                }

                await context.SaveChangesAsync();
                nbProduits = nomsTexte.Count;
            }

            // ── 3. Comptes demo ───────────────────────────────────────────────────
            var comptesDémo = new[]
            {
                new { Email = "admin@poulina.tn",       Nom = "Admin",     Prenom = "Système", Role = "Admin",            FilialeId = (int?)null       },
                new { Email = "responsable@poulina.tn", Nom = "Ben Salem", Prenom = "Karim",   Role = "ResponsableLabo",  FilialeId = (int?)null       },
                new { Email = "directeur@poulina.tn",   Nom = "Mzali",     Prenom = "Sonia",   Role = "DirecteurQualite", FilialeId = (int?)null       },
                new { Email = "technicien@poulina.tn",  Nom = "Jomaa",     Prenom = "Amine",   Role = "Technicien",       FilialeId = (int?)null       },
                new { Email = "production@poulina.tn",  Nom = "Hamdi",     Prenom = "Leila",   Role = "Production",       FilialeId = (int?)filiale.Id },
                new { Email = "commercial@poulina.tn",  Nom = "Ayari",     Prenom = "Omar",    Role = "Commercial",       FilialeId = (int?)filiale.Id },
                new { Email = "auditeur@poulina.tn",    Nom = "Nasri",     Prenom = "Rania",   Role = "Auditeur",         FilialeId = (int?)null       },
            };

            int inserted = 0, skipped = 0;
            foreach (var compte in comptesDémo)
            {
                var existant = await context.Utilisateurs
                    .FirstOrDefaultAsync(u => u.Email == compte.Email);

                if (existant == null)
                {
                    context.Utilisateurs.Add(new Utilisateur
                    {
                        Nom        = compte.Nom,
                        Prenom     = compte.Prenom,
                        Email      = compte.Email,
                        MotDePasse = hash,
                        Role       = compte.Role,
                        FilialeId  = compte.FilialeId
                    });
                    inserted++;
                }
                else
                {
                    // Toujours synchroniser la filiale pour production/commercial
                    if (compte.FilialeId.HasValue && existant.FilialeId != compte.FilialeId)
                        existant.FilialeId = compte.FilialeId;
                    skipped++;
                }
            }

            if (inserted > 0 || context.ChangeTracker.HasChanges())
                await context.SaveChangesAsync();

            // ── Diagnostic ────────────────────────────────────────────────────────
            var prodUser = await context.Utilisateurs
                .FirstOrDefaultAsync(u => u.Email == "production@poulina.tn");

            var produits = await context.Produits
                .Where(p => p.FilialeId == filiale.Id && p.Actif)
                .Select(p => p.Nom)
                .ToListAsync();

            var diagnostic = new
            {
                filiale = new { filiale.Id, filiale.Nom, filiale.Ville },
                produits,
                productionUser = prodUser == null ? null : new
                {
                    prodUser.Id,
                    prodUser.Email,
                    prodUser.FilialeId,
                    synchro = prodUser.FilialeId == filiale.Id
                }
            };

            return (inserted, skipped, diagnostic);
        }

        public static async Task SeedAnalysesAsync(ApplicationDbContext context)
        {
            static string Norm(string s) => System.Text.RegularExpressions.Regex.Replace(
                s.ToLower()
                 .Replace(" - ", " ").Replace(" du ", " ").Replace(" de ", " ").Replace(" des ", " "),
                @"\s+", " ").Trim();

            var catalogue = new[]
            {
                // ── Lait — noms exacts correspondant aux IDs 1-6 en base ──
                new Analyse { Nom = "pH du Lait UHT",            Description = "Acidité pH",              ValeurMin = 6.6,  ValeurMax = 6.8,   Unite = "pH"   },
                new Analyse { Nom = "Taux Matière Grasse Lait",  Description = "Matière grasse totale",   ValeurMin = 3.5,  ValeurMax = 4.5,   Unite = "%"    },
                new Analyse { Nom = "Taux Protéines Lait",       Description = "Teneur en protéines",     ValeurMin = 3.0,  ValeurMax = 3.8,   Unite = "%"    },
                new Analyse { Nom = "Densité Lait",              Description = "Densité à 20°C",          ValeurMin = 1.028,ValeurMax = 1.034,  Unite = "g/mL" },
                new Analyse { Nom = "Acidité Dornic Lait",       Description = "Acidité titrable",        ValeurMin = 13,   ValeurMax = 18,    Unite = "°D"   },
                new Analyse { Nom = "Extrait Sec Total Lait",    Description = "Extrait sec total",       ValeurMin = 115,  ValeurMax = 135,   Unite = "g/L"  },

                // ── Yaourt — noms exacts correspondant aux IDs 7-11 en base ─
                new Analyse { Nom = "pH Yaourt",                 Description = "Acidité pH",              ValeurMin = 4.0,  ValeurMax = 4.6,   Unite = "pH"   },
                new Analyse { Nom = "Acidité Yaourt",            Description = "Acidité titrable",        ValeurMin = 0.7,  ValeurMax = 1.5,   Unite = "%"    },
                new Analyse { Nom = "Taux Matière Grasse Yaourt",Description = "Matière grasse totale",   ValeurMin = 2.5,  ValeurMax = 3.5,   Unite = "%"    },
                new Analyse { Nom = "Extrait Sec Yaourt",        Description = "Extrait sec total",       ValeurMin = 12,   ValeurMax = 18,    Unite = "%"    },
                new Analyse { Nom = "Coliformes Yaourt",         Description = "Coliformes totaux",       ValeurMin = 0,    ValeurMax = 10,    Unite = "UFC/g"},

                // ── Beurre — IDs 35-38 en base ───────────────────────────
                new Analyse { Nom = "Taux Matière Grasse - Beurre",Description = "Teneur en matière grasse",ValeurMin = 82, ValeurMax = 84,  Unite = "%"    },
                new Analyse { Nom = "Humidité - Beurre",           Description = "Teneur en eau",           ValeurMin = 0,  ValeurMax = 16,  Unite = "%"    },
                new Analyse { Nom = "Acidité - Beurre",            Description = "Acidité titrable",        ValeurMin = 0,  ValeurMax = 0.5, Unite = "°D"   },
                new Analyse { Nom = "NaCl - Beurre",               Description = "Teneur en sel",           ValeurMin = 0,  ValeurMax = 2,   Unite = "%"    },

                // ── Fromage frais — IDs 39-42 en base ────────────────────
                new Analyse { Nom = "pH - Fromage frais",                  Description = "Acidité pH",              ValeurMin = 4.2, ValeurMax = 4.8,  Unite = "pH"   },
                new Analyse { Nom = "Taux Matière Grasse - Fromage frais", Description = "Teneur en matière grasse",ValeurMin = 20,  ValeurMax = 25,   Unite = "%"    },
                new Analyse { Nom = "Humidité - Fromage frais",            Description = "Teneur en eau",           ValeurMin = 50,  ValeurMax = 60,   Unite = "%"    },
                new Analyse { Nom = "Coliformes - Fromage frais",          Description = "Coliformes totaux",       ValeurMin = 0,   ValeurMax = 10,   Unite = "UFC/g"},
            };

            var existantes = await context.Analyses.ToListAsync();
            var nomsNormExistants = existantes.Select(a => Norm(a.Nom)).ToHashSet();

            foreach (var analyse in catalogue)
            {
                var nomNorm = Norm(analyse.Nom);
                if (!nomsNormExistants.Contains(nomNorm))
                {
                    context.Analyses.Add(analyse);
                    nomsNormExistants.Add(nomNorm);
                }
            }

            if (context.ChangeTracker.HasChanges())
                await context.SaveChangesAsync();
        }
    }
}
