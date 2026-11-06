using Domain.Models;
using Domain.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Text.RegularExpressions;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyseController : ControllerBase
    {
        private readonly IGenericRepository<Analyse> _repository;

        public AnalyseController(IGenericRepository<Analyse> repository)
        {
            _repository = repository;
        }

        // Normalise un nom de paramètre pour la détection de doublons :
        // supprime " - ", " du ", " de ", " des ", espaces multiples, puis met en minuscules.
        private static string NomNormalise(string nom) =>
            Regex.Replace(
                nom.ToLower()
                   .Replace(" - ", " ")
                   .Replace(" du ", " ")
                   .Replace(" de ", " ")
                   .Replace(" des ", " "),
                @"\s+", " "
            ).Trim();

        private static readonly Dictionary<string, List<Analyse>> NormesStandards = new()
        {
            ["Jus de fruits"] =
            [
                new() { Nom = "pH Jus de fruits",                   Unite = "pH",       ValeurMin = 3.5,  ValeurMax = 4.5  },
                new() { Nom = "Brix Jus de fruits",                  Unite = "°Brix",    ValeurMin = 10,   ValeurMax = 14   },
                new() { Nom = "Acidité titrable Jus de fruits",      Unite = "g/L",      ValeurMin = 0.3,  ValeurMax = 0.8  },
                new() { Nom = "Vitamine C Jus de fruits",            Unite = "mg/100ml", ValeurMin = 20,   ValeurMax = 60   },
                new() { Nom = "Coliformes Jus de fruits",            Unite = "UFC/ml",   ValeurMin = 0,    ValeurMax = 0    },
                new() { Nom = "Levures Moisissures Jus de fruits",   Unite = "UFC/ml",   ValeurMin = 0,    ValeurMax = 10   },
            ],
            ["Nectar"] =
            [
                new() { Nom = "pH Nectar",                           Unite = "pH",       ValeurMin = 3.2,  ValeurMax = 4.2  },
                new() { Nom = "Brix Nectar",                         Unite = "°Brix",    ValeurMin = 12,   ValeurMax = 16   },
                new() { Nom = "Acidité titrable Nectar",             Unite = "g/L",      ValeurMin = 0.4,  ValeurMax = 1.0  },
                new() { Nom = "Coliformes Nectar",                   Unite = "UFC/ml",   ValeurMin = 0,    ValeurMax = 0    },
            ],
            ["Beurre"] =
            [
                new() { Nom = "Taux Matière Grasse Beurre",          Unite = "%",        ValeurMin = 82,   ValeurMax = 84   },
                new() { Nom = "Humidité Beurre",                     Unite = "%",        ValeurMin = 0,    ValeurMax = 16   },
                new() { Nom = "Acidité Beurre",                      Unite = "°D",       ValeurMin = 0,    ValeurMax = 0.5  },
                new() { Nom = "NaCl Beurre",                         Unite = "%",        ValeurMin = 0,    ValeurMax = 2    },
            ],
            ["Fromage frais"] =
            [
                new() { Nom = "pH Fromage frais",                    Unite = "pH",       ValeurMin = 4.2,  ValeurMax = 4.8  },
                new() { Nom = "Taux Matière Grasse Fromage frais",   Unite = "%",        ValeurMin = 20,   ValeurMax = 25   },
                new() { Nom = "Humidité Fromage frais",              Unite = "%",        ValeurMin = 50,   ValeurMax = 60   },
                new() { Nom = "Coliformes Fromage frais",            Unite = "UFC/g",    ValeurMin = 0,    ValeurMax = 10   },
            ],
            ["Crème glacée"] =
            [
                new() { Nom = "Matière Grasse Crème glacée",         Unite = "%",        ValeurMin = 8,    ValeurMax = 12   },
                new() { Nom = "Extrait Sec Crème glacée",            Unite = "%",        ValeurMin = 30,   ValeurMax = 36   },
                new() { Nom = "Coliformes Crème glacée",             Unite = "UFC/g",    ValeurMin = 0,    ValeurMax = 10   },
                new() { Nom = "Levures Moisissures Crème glacée",    Unite = "UFC/g",    ValeurMin = 0,    ValeurMax = 50   },
            ],
        };

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            var analyses = await _repository.GetAll();
            return Ok(analyses);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var analyse = await _repository.GetById(id);
            if (analyse == null) return NotFound();
            return Ok(analyse);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Add([FromBody] Analyse analyse)
        {
            if (analyse == null) return BadRequest();

            var nomNorm = NomNormalise(analyse.Nom);
            var existantes = await _repository.GetAll();
            var doublon = existantes.FirstOrDefault(a =>
                NomNormalise(a.Nom) == nomNorm ||
                NomNormalise(a.Nom).StartsWith(nomNorm) ||
                nomNorm.StartsWith(NomNormalise(a.Nom))
            );
            if (doublon != null)
                return Conflict(new
                {
                    message = $"Ce paramètre existe déjà pour ce produit (similaire à \"{doublon.Nom}\", ID {doublon.Id})"
                });

            await _repository.Add(analyse);
            return Ok(new { message = "Analyse ajoutée avec succès", analyse });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            await _repository.Delete(id);
            return Ok(new { message = "Analyse supprimée avec succès" });
        }

        [HttpGet("produits-standards")]
        [Authorize]
        public IActionResult GetProduitsStandards()
        {
            return Ok(NormesStandards.Keys.ToList());
        }

        [HttpPost("importer-normes-standards")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ImporterNormesStandards([FromBody] ImportNormesRequest request)
        {
            if (request?.Produits == null || request.Produits.Count == 0)
                return BadRequest(new { message = "Sélectionnez au moins un produit" });

            var existantes = await _repository.GetAll();
            var nomsNormExistants = existantes.Select(a => NomNormalise(a.Nom)).ToHashSet();
            int importes = 0;

            foreach (var produit in request.Produits)
            {
                if (!NormesStandards.TryGetValue(produit, out var parametres)) continue;

                foreach (var param in parametres)
                {
                    var nomNorm = NomNormalise(param.Nom);
                    if (nomsNormExistants.Contains(nomNorm)) continue;
                    await _repository.Add(new Analyse
                    {
                        Nom = param.Nom,
                        Unite = param.Unite,
                        ValeurMin = param.ValeurMin,
                        ValeurMax = param.ValeurMax,
                        Description = ""
                    });
                    nomsNormExistants.Add(nomNorm);
                    importes++;
                }
            }

            return Ok(new
            {
                message = importes > 0
                    ? $"{importes} paramètre(s) importé(s) avec succès"
                    : "Tous les paramètres sélectionnés existent déjà",
                importes
            });
        }
    }

    public record ImportNormesRequest(List<string> Produits);
}
