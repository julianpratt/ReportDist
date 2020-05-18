using Mistware.Utils;

namespace ReportDist.Data
{
    public class CatalogueAPI
    {
        private static CatalogueAPI _me = new CatalogueAPI();

        /// Returns singleton instance of Log class. 
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
            if (SearchCataloguesAPI == null || GetCatalogueAPI == null || ReportNoXpath == null) return false;
            if (CIDXpath == null || eFileNameXpath == null || AttachmentXpath == null) return false;
            return true;
        }

    }
}