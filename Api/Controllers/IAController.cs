using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IAController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<IAController> _logger;

        public IAController(IConfiguration configuration, ILogger<IAController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("analyser")]
        public async Task<IActionResult> Analyser([FromBody] AnalyseIARequest request)
        {
            var prompt = $@"Tu es un expert en contrôle qualité pour un laboratoire de fabrication de produits laitiers.

Une anomalie a été détectée :
- Analyse : {request.NomAnalyse}
- Valeur mesurée : {request.Valeur} {request.Unite}
- Valeur minimale normale : {request.ValeurMin} {request.Unite}
- Valeur maximale normale : {request.ValeurMax} {request.Unite}

En 3 phrases maximum, explique :
1. Ce que cette anomalie signifie
2. La cause probable
3. La recommandation à suivre

Réponds directement sans introduction.";

            return await CallClaude(prompt, 1000);
        }

        [HttpPost("analyser-demande")]
        public async Task<IActionResult> AnalyserDemande([FromBody] AnalyseDemandeIARequest request)
        {
            var lotSection = !string.IsNullOrWhiteSpace(request.NumeroDeLot)
                ? $"Numéro de lot : {request.NumeroDeLot}\n"
                : "";

            var historiqueSection = !string.IsNullOrWhiteSpace(request.Historique)
                ? $"\nHISTORIQUE DES 5 DERNIÈRES ANALYSES DU MÊME PRODUIT (même filiale) :\n{request.Historique}\n"
                : "\nHISTORIQUE : Aucune analyse antérieure disponible pour ce produit dans cette filiale.\n";

            var prompt = $@"Tu es un expert senior en contrôle qualité pour Poulina Group Holding, Tunisie. Tu analyses des résultats de laboratoire pour des produits alimentaires.

DEMANDE EN COURS :
Produit : {request.Produit}
Filiale : {request.Filiale}
{lotSection}
PARAMÈTRES MESURÉS (valeur mesurée, norme min–max, résultat, proximité limite) :
{request.Lignes}
{historiqueSection}
RÈGLES DE GÉNÉRATION — respecte-les impérativement :

1. COMMENTAIRE GÉNÉRAL (champ ""commentaireGeneral"") — TOUJOURS présent, JAMAIS vide :
   - Tous conformes → commentaire positif professionnel : ex. ""Tous les paramètres analysés sont conformes aux normes établies. La qualité du lot est satisfaisante.""
   - Valeur proche d'une limite (dans les 10% du seuil) → avertissement préventif : ex. ""Le pH est conforme mais proche de la limite basse. Une surveillance renforcée est recommandée.""
   - Anomalie détectée → alerte professionnelle avec contexte

2. ANALYSE PAR PARAMÈTRE (champ ""analysesParParametre"") — un objet par paramètre mesuré :
   - tendance : ""En hausse"" si valeur proche du max ou supérieure à la médiane, ""En baisse"" si proche du min ou inférieure, ""Stable"" sinon ; ajuste selon l'historique si disponible
   - commentaire : phrase courte descriptive (ex: ""Valeur stable dans la norme"", ""Valeur proche de la limite haute — surveiller"", ""Deuxième anomalie consécutive — vérifier le process"")

3. CAUSES PROBABLES (champ ""causesProbables"") :
   - Si aucune anomalie → liste vide []
   - Si anomalie → 2-3 hypothèses techniques avec niveau Très probable/Possible/Moins probable
   - Comparer avec l'historique : mentionner si c'est la première ou Nième anomalie sur ce paramètre

4. RECOMMANDATIONS (champ ""recommandations"") — TOUJOURS au moins une entrée :
   - Type ""Immédiate"" si anomalie critique
   - Type ""Corrective"" si anomalie non critique
   - Type ""Surveillance"" si valeur proche d'un seuil
   - Type ""RAS"" si tout est conforme et stable : ex. ""Aucune action requise — qualité conforme aux standards Poulina.""

Réponds UNIQUEMENT en JSON valide, sans texte avant ni après, sans balises markdown :
{{
  ""commentaireGeneral"": ""commentaire professionnel toujours présent"",
  ""statut"": ""Conforme"",
  ""gravite"": ""Aucune"",
  ""resume"": ""résumé synthétique en une phrase"",
  ""analysesParParametre"": [
    {{ ""nom"": ""nom exact du paramètre"", ""tendance"": ""Stable"", ""commentaire"": ""commentaire court"" }}
  ],
  ""causesProbables"": [],
  ""recommandations"": [
    {{ ""type"": ""RAS"", ""texte"": ""description de la recommandation"" }}
  ],
  ""impact"": {{
    ""qualiteProduit"": ""impact ou 'Aucun impact détecté'"",
    ""santeConsommateur"": ""risque ou 'Aucun risque sanitaire immédiat identifié'"",
    ""lotProduction"": ""impact lot ou 'Lot conforme — libération autorisée'""
  }},
  ""actions"": {{
    ""immediates"": [],
    ""correctives"": [],
    ""preventives"": [""maintenir le suivi habituel""]
  }},
  ""decision"": {{
    ""decision"": ""Libérer le lot"",
    ""urgence"": ""Modéré"",
    ""justification"": ""justification professionnelle""
  }},
  ""lotABloquer"": false,
  ""risqueConsommateur"": ""synthèse risque en une phrase"",
  ""actionImmediate"": ""action principale ou 'Aucune action immédiate requise'"",
  ""actionCorrective"": ""action corrective ou 'Maintenir les bonnes pratiques de fabrication'""
}}";

            return await CallClaude(prompt, 2500);
        }

        private async Task<IActionResult> CallClaude(string prompt, int maxTokens)
        {
            var apiKey = _configuration["Anthropic:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey) || apiKey == "VOTRE_CLE_API_ICI")
            {
                _logger.LogError("Anthropic API key is missing or is the placeholder value. Set Anthropic:ApiKey in appsettings.json.");
                return StatusCode(503, new { error = "IA non configurée — clé API manquante" });
            }

            var requestBody = new
            {
                model = "claude-sonnet-4-6",
                max_tokens = maxTokens,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("x-api-key", apiKey);
            client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync("https://api.anthropic.com/v1/messages", content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    // Never forward Anthropic's status codes (401/403/429) — they would be
                    // misread by Angular's auth interceptor as a JWT expiry and trigger logout.
                    _logger.LogError("Anthropic API error {Status}: {Body}", (int)response.StatusCode, responseBody);
                    return StatusCode(502, new { error = "IA service unavailable", status = (int)response.StatusCode });
                }

                // Return the raw JSON directly — Ok(string) would double-serialize it,
                // causing Angular to receive a JS string instead of a parsed object.
                return Content(responseBody, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception calling Anthropic API: {Message}", ex.Message);
                return StatusCode(502, new { error = "IA service unreachable", detail = ex.Message });
            }
        }
    }

    public class AnalyseIARequest
    {
        public string NomAnalyse { get; set; } = "";
        public double Valeur { get; set; }
        public double ValeurMin { get; set; }
        public double ValeurMax { get; set; }
        public string Unite { get; set; } = "";
    }

    public class AnalyseDemandeIARequest
    {
        public string Produit { get; set; } = "";
        public string Filiale { get; set; } = "";
        public string NumeroDeLot { get; set; } = "";
        public string Lignes { get; set; } = "";
        public string Historique { get; set; } = "";
    }
}