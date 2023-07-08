namespace Tellma.Utilities.EmailLogger
{
    public class EmailOptions
    {
        public string? EmailAddresses { get; set; }
        public string? InstallationIdentifier { get; set; }
        public string? SmtpHost { get; set; }
        public int? SmtpPort { get; set; }
        public bool SmtpUseSsl { get; set; }
        public string? SmtpUsername { get; set; }
        public string? SmtpPassword { get; set; }
    }
}