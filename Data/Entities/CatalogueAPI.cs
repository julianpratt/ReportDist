using Mistware.Utils;

namespace ReportDist.Data
{
    public class CatalogueAPI
    {
        private static CatalogueAPI _me = new CatalogueAPI();

        public static CatalogueAPI Me { get { return _me; } }
        private CatalogueAPI()
        {
            string root = Config.Get("CatalogueServiceRoot");
            SearchCataloguesAPI  = root + Config.Get("SearchCataloguesAPI");
            GetCatalogueAPI      = root + Config.Get("GetCatalogueAPI");
            string searchCataloguesXpath = Config.Get("SearchCataloguesXpath");
            ReportNoXpath                = searchCataloguesXpath + Config.Get("ReportNoXpath"); 
            CIDXpath                     = searchCataloguesXpath + Config.Get("CIDXpath");  
            string getCatalogueXpath     = Config.Get("GetCatalogueXpath");
            eFileNameXpath               = getCatalogueXpath + Config.Get("eFileNameXpath");
            AttachmentXpath              = getCatalogueXpath + Config.Get("AttachmentXpath");
        }

        public string SearchCataloguesAPI  { get; private set; }
        public string GetCatalogueAPI      { get; private set; }
        public string ReportNoXpath        { get; private set; }
        public string CIDXpath             { get; private set; }
        public string eFileNameXpath       { get; private set; }
        public string AttachmentXpath      { get; private set; }

        public bool IsValid()
        {
            bool ok = SearchCataloguesAPI.HasValue() && GetCatalogueAPI.HasValue() && ReportNoXpath.HasValue();
            ok = ok && CIDXpath.HasValue() && eFileNameXpath.HasValue() && AttachmentXpath.HasValue();
            return ok;
        }

        public void Debug()
        {
            Log.Me.Debug("----------------------------------------------------------------------------------------------");
            Log.Me.Debug("Catalogue API Configuration:");
            Log.Me.Debug("");
            Log.Me.Debug("SearchCataloguesAPI: " + (SearchCataloguesAPI ?? ""));
            Log.Me.Debug("GetCataloguesAPI:    " + (GetCatalogueAPI     ?? ""));
            Log.Me.Debug("ReportNoXpath:       " + (ReportNoXpath       ?? ""));
            Log.Me.Debug("CIDXpath:            " + (CIDXpath            ?? ""));
            Log.Me.Debug("eFileNameXpath:      " + (eFileNameXpath      ?? ""));
            Log.Me.Debug("AttachmentXpath:     " + (AttachmentXpath     ?? ""));
            if (IsValid()) Log.Me.Debug("Everything is configured.");
            else           Log.Me.Warn("Some Catalogue API configuration is missing");
        }

    }
}