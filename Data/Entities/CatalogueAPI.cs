using System;
using System.IO;
using System.Text.Json;
using Mistware.Utils;

namespace ReportDist.Data
{
    public class CatalogueAPI
    {
        private static CatalogueAPI _me = new CatalogueAPI();

        public static CatalogueAPI Me { get { return _me; } }
        private CatalogueAPI()
        {
            string root = Config.Get("CatalogueDomain");

            string authorization = Config.Get("Authorization");
            string lmsApiToken   = Config.Get("LmsApiToken");

            if (authorization.HasValue() && lmsApiToken.HasValue())
            {
                Authorization = new KeyList();
                Authorization.Add("Authorization", authorization);
                Authorization.Add("LmsApiToken",   lmsApiToken);
                ReadJsonConfig("Config/Catalogue2021API.json");
                CatalogueUserId = "";
            }
            else
            {
                Authorization = null;
                CatalogueUserId = Config.Get("CatalogueUserId");
                if (!CatalogueUserId.HasValue())
                {
                    Log.Me.Fatal("Catalogue API authorization is missing");
                    System.Environment.Exit(8);    
                }
                ReadJsonConfig("Config/Catalogue2015API.json");
            }

            SearchCataloguesAPI  = root + Config.Get("SearchCataloguesAPI");
            GetCatalogueAPI      = root + Config.Get("GetCatalogueAPI");
            string searchCataloguesXpath = Config.Get("SearchCataloguesXpath");
            ReportNoXpath                = searchCataloguesXpath + Config.Get("ReportNoXpath"); 
            CIDXpath                     = searchCataloguesXpath + Config.Get("CIDXpath");  
            string getCatalogueXpath     = Config.Get("GetCatalogueXpath");
            ReportNoGetXpath             = getCatalogueXpath + Config.Get("ReportNoXpath"); 
            eFileNameXpath               = getCatalogueXpath + Config.Get("eFileNameXpath");
            AttachmentXpath              = getCatalogueXpath + Config.Get("AttachmentXpath");
            //AbstractXpath                = getCatalogueXpath + Config.Get("AbstractXpath");

        }

        public string  SearchCataloguesAPI         { get; private set; }
        public string  GetCatalogueAPI             { get; private set; }
        public string  ReportNoXpath               { get; private set; }
        public string  ReportNoGetXpath            { get; private set; }
        public string  CIDXpath                    { get; private set; }
        public string  eFileNameXpath              { get; private set; }
        public string  AttachmentXpath             { get; private set; }
        //public string  AbstractXpath               { get; private set; }
        public KeyList Authorization               { get; private set; }
        public string  CatalogueUserId             { get; private set; }
        
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

        private static void ReadJsonConfig(string configFile)
        {
            if (File.Exists(configFile))
            {
                string json = File.ReadAllText(configFile);
            
                using (JsonDocument document = JsonDocument.Parse(json))
                {
                    foreach (JsonProperty prop in document.RootElement.EnumerateObject())
                    {
                        string key = prop.Name;
                        string valuekind = prop.Value.ValueKind.ToString();
                        if      (valuekind == "String")  Config.Set(key, prop.Value.GetString());
                        else if (valuekind == "Number")  Config.Set(key, prop.Value.GetInt64().ToString());
                    }
                }
            }    
        }     
    }
}