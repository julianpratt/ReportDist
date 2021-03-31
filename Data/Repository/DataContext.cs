using Microsoft.EntityFrameworkCore;
using Mistware.Utils;

namespace ReportDist.Data
{
    public class DataContext : DbContext
    {   
        public DataContext(DbContextOptions<DataContext> options) : base(options) 
        {
            CirculationRepo   = new CirculationRepository(this);
            PendingReportRepo = new PendingReportRepository(this);
            RecipientRepo     = new RecipientRepository(this);
            StandingDataRepo  = new StandingDataRepository(this);
            OrgTreeRepo       = new OrgTreeRepository(this);
        }

        public CirculationRepository   CirculationRepo   { get; set; } 
        public PendingReportRepository PendingReportRepo { get; set; } 
        public RecipientRepository     RecipientRepo     { get; set; } 
        public StandingDataRepository  StandingDataRepo  { get; private set; }
        public OrgTreeRepository       OrgTreeRepo       { get; private set; }

        public DbSet<AccessCode>       AccessCodes    { get { return Set<AccessCode>();    } } 
        public DbSet<Circulation>      Circulations   { get { return Set<Circulation>();   } } 
        public DbSet<Recipient>        Recipients     { get { return Set<Recipient>();     } } 
        public DbSet<PendingReport>    PendingReports { get { return Set<PendingReport>(); } } 
        public DbSet<Company>          Companies      { get { return Set<Company>();       } } 
        public DbSet<ReportType>       ReportTypes    { get { return Set<ReportType>();    } }
        public DbSet<SecurityLevel>    SecurityLevels { get { return Set<SecurityLevel>(); } } 
        public DbSet<OrgTreeBase>      OrgTreeBaseSet { get { return Set<OrgTreeBase>();   } }  
        public DbSet<NextNumbers>      NextNumberSet  { get { return Set<NextNumbers>();   } }  

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // warning To protect potentially sensitive information in your connection string, 
            // you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 
            // for guidance on storing connection strings.
            string connection = Config.Get("SQLConnection");
            if (SQLServerType(connection) == "MySQL")
            {
                options.UseMySQL(connection);
            }
            else
            {
                options.UseSqlServer(connection);
            }
        }

        private string SQLServerType(string connection)
        {
            string server = ParseConnect(connection);
            string stem = ".database.windows.net,1433";
            if (server.Left(4).ToLower() == "tcp:" && server.Right(stem.Length).ToLower() == stem)
            {
                return "SQLServer";
            }
            else
            {
                return "MySQL";
            }
        }
        private string ParseConnect(string connect)
        {
            if (connect == null) return null;

            string[] segs = connect.Split(';');
            string server   = null;
            string database = null;
            string userid   = null;
            string password = null;

            foreach (string seg in segs)
            {
                int i = seg.IndexOf('=');
                if (i > 0)
                {
                    string key = seg.Substring(0, i).ToLower();
                    string value = seg.Substring(i + 1);
                    if (key == "data source"     || key == "server")       server   = value;
                    if (key == "initial catalog" || key == "database")     database = value;
                    if (key == "user id" || key == "user" || key == "uid") userid   = value;
                    if (key == "password"        || key == "pwd")          password = value;
                }
            }

            if (server == null || database == null || userid == null || password == null)
            {
                Log.Me.Fatal("Connection string " + connect + " is not formatted correctly.");
                System.Environment.Exit(8);
            }    

            return server;
        }
    }
}