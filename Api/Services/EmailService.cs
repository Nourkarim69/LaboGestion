using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Api.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task EnvoyerEmail(string destinataire, string sujet, string corps)
        {
            if (!bool.TryParse(_configuration["Email:Enabled"], out bool enabled) || !enabled)
            {
                Console.WriteLine($"[Email désactivé] Destinataire: {destinataire} | Sujet: {sujet}");
                return;
            }

            try
            {
                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse(_configuration["Email:From"]));
                email.To.Add(MailboxAddress.Parse(destinataire));
                email.Subject = sujet;

                var builder = new BodyBuilder();
                builder.HtmlBody = corps;
                email.Body = builder.ToMessageBody();

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(
                    _configuration["Email:Host"],
                    int.Parse(_configuration["Email:Port"]!),
                    SecureSocketOptions.StartTls
                );
                await smtp.AuthenticateAsync(
                    _configuration["Email:From"],
                    _configuration["Email:Password"]
                );
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur email : {ex.Message}");
            }
        }

        public async Task NotifierAnalyseTerminee(string destinataire, string nomDestinataire, string reference, string typeProduit, string commentaire)
        {
            var sujet = $"[Paulina Groupe] Résultats disponibles — {reference}";
            var corps = $@"
            <div style='font-family: Segoe UI, sans-serif; max-width: 600px; margin: 0 auto;'>
                <div style='background: #0f172a; padding: 24px 32px;'>
                    <h2 style='color: #fff; margin: 0; font-size: 18px;'>Paulina Groupe Holding</h2>
                    <p style='color: #64748b; margin: 4px 0 0; font-size: 13px;'>Laboratoire Produits Laitiers</p>
                </div>
                <div style='padding: 32px; background: #f8fafc; border: 1px solid #e2e8f0;'>
                    <p style='color: #374151; font-size: 15px;'>Bonjour <strong>{nomDestinataire}</strong>,</p>
                    <p style='color: #374151;'>Les résultats d'analyse pour la demande suivante sont maintenant disponibles :</p>
                    <div style='background: #fff; border: 1px solid #e2e8f0; border-radius: 8px; padding: 20px; margin: 20px 0;'>
                        <p style='margin: 0 0 8px;'><strong>Référence :</strong> <span style='font-family: monospace; color: #1d4ed8;'>{reference}</span></p>
                        <p style='margin: 0 0 8px;'><strong>Type de produit :</strong> {typeProduit}</p>
                        <p style='margin: 0;'><strong>Commentaire du laboratoire :</strong> {commentaire}</p>
                    </div>
                    <p style='color: #374151;'>Connectez-vous à la plateforme pour consulter les résultats complets et télécharger le rapport PDF.</p>
                    <a href='http://localhost:4200' style='display: inline-block; background: #1d4ed8; color: #fff; padding: 12px 24px; border-radius: 6px; text-decoration: none; font-weight: 600; margin-top: 8px;'>
                        Consulter les résultats
                    </a>
                </div>
                <div style='padding: 16px 32px; background: #f1f5f9; border-top: 1px solid #e2e8f0;'>
                    <p style='color: #94a3b8; font-size: 12px; margin: 0;'>Ce message est généré automatiquement par la plateforme Gestion Laboratoire — Paulina Groupe Holding. Usage interne uniquement.</p>
                </div>
            </div>";

            await EnvoyerEmail(destinataire, sujet, corps);
        }

        public async Task NotifierAnomalieDetectee(string destinataire, string nomDestinataire, string reference, string nomAnalyse, double valeur, string unite, double min, double max)
        {
            var sujet = $"[ALERTE QUALITÉ] Anomalie détectée — {reference}";
            var corps = $@"
            <div style='font-family: Segoe UI, sans-serif; max-width: 600px; margin: 0 auto;'>
                <div style='background: #7f1d1d; padding: 24px 32px;'>
                    <h2 style='color: #fff; margin: 0; font-size: 18px;'>ALERTE QUALITÉ — Paulina Groupe</h2>
                    <p style='color: #fca5a5; margin: 4px 0 0; font-size: 13px;'>Anomalie détectée en laboratoire</p>
                </div>
                <div style='padding: 32px; background: #fff5f5; border: 1px solid #fecaca;'>
                    <p style='color: #374151; font-size: 15px;'>Bonjour <strong>{nomDestinataire}</strong>,</p>
                    <p style='color: #374151;'>Une anomalie a été détectée lors de l'analyse. Votre attention est requise immédiatement.</p>
                    <div style='background: #fff; border: 2px solid #dc2626; border-radius: 8px; padding: 20px; margin: 20px 0;'>
                        <p style='margin: 0 0 8px;'><strong>Référence :</strong> <span style='font-family: monospace; color: #dc2626;'>{reference}</span></p>
                        <p style='margin: 0 0 8px;'><strong>Analyse :</strong> {nomAnalyse}</p>
                        <p style='margin: 0 0 8px;'><strong>Valeur mesurée :</strong> <span style='color: #dc2626; font-weight: bold;'>{valeur} {unite}</span></p>
                        <p style='margin: 0;'><strong>Norme attendue :</strong> {min} — {max} {unite}</p>
                    </div>
                    <p style='color: #dc2626; font-weight: 600;'>Action requise : Contacter le responsable qualité immédiatement.</p>
                </div>
                <div style='padding: 16px 32px; background: #f1f5f9; border-top: 1px solid #e2e8f0;'>
                    <p style='color: #94a3b8; font-size: 12px; margin: 0;'>Message automatique — Plateforme Gestion Laboratoire — Paulina Groupe Holding.</p>
                </div>
            </div>";

            await EnvoyerEmail(destinataire, sujet, corps);
        }
    }
}