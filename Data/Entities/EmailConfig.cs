using System.Net.Mail;
using Mistware.Utils;

namespace ReportDist.Data
{
    public class EmailConfig
    {
        private static EmailConfig _me = new EmailConfig();

        public static EmailConfig Me { get { return _me; } }
        private EmailConfig()
        {
            PostmasterEmail     = Config.Get("PostmasterEmail");
            PostmasterName      = Config.Get("PostmasterName");
            InternalEmailDomain = Config.Get("InternalEmailDomain");
            PortalLinkURL       = Config.Get("PortalLinkURL"); 
            Company             = Config.Get("Company");  
            CatalogueSupport    = Config.Get("CatalogueSupport");
            SendGridKey         = Config.Get("SendGridKey");
        }

        public string PostmasterEmail      { get; private set; }
        public string PostmasterName       { get; private set; }
        public string InternalEmailDomain  { get; private set; }
        public string PortalLinkURL        { get; private set; }
        public string Company              { get; private set; }
        public string CatalogueSupport     { get; private set; }
        public string SendGridKey          { get; private set; }

        public MailAddress Postmaster
        {
            get { return new MailAddress(PostmasterEmail, PostmasterName); }
        }

        public bool IsValid()
        {
            bool ok = PostmasterEmail.HasValue()      && PostmasterName.HasValue();
            ok = ok && InternalEmailDomain.HasValue() && PortalLinkURL.HasValue();
            ok = ok && Company.HasValue() && CatalogueSupport.HasValue() && SendGridKey.HasValue();
            return ok;
        }

        public void Debug()
        {
            Log.Me.Debug("----------------------------------------------------------------------------------------------");
            Log.Me.Debug("Email Configuration:");
            Log.Me.Debug("");
            Log.Me.Debug("PostmasterEmail:     " + (PostmasterEmail     ?? ""));
            Log.Me.Debug("PostmasterName:      " + (PostmasterName      ?? ""));
            Log.Me.Debug("InternalEmailDomain: " + (InternalEmailDomain ?? ""));
            Log.Me.Debug("PortalLinkURL:       " + (PortalLinkURL       ?? ""));
            Log.Me.Debug("Company:             " + (Company             ?? ""));
            Log.Me.Debug("CatalogueSupport:    " + (CatalogueSupport    ?? ""));
            Log.Me.Debug("SendGridKey:         " + (SendGridKey         ?? ""));
            if (IsValid()) Log.Me.Debug("Everything is configured.");
            else           Log.Me.Warn("Some Email configuration is missing");
        }

    }
}