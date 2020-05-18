using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using Microsoft.EntityFrameworkCore;
using Mistware.Utils;
using ReportDist.Data;

namespace ReportDistDataTest
{
	class Program
	{
		static void Main(string[] args)
		{
            string sFail="";

            // Define Test Data 
            List<String> people = new List<string>() {"Aleshia Tomkiewicz", "Michell Throssell", "Dong Kopczynski", "Elinore Fulda"};
            string domain = "@gmx.com";
            string address = "Acme Corporation,\nBBC Quay House,\nMediaCityUK,\nSalford,\nM50 2QH"; 

            Config.Setup("appsettings.json", Directory.GetCurrentDirectory(), null, "ReportDistDataTest");

            DbContextOptions<DataContext> options = new DbContextOptions<DataContext>();
            DataContext context = new DataContext(options);

            Log.Me.DebugOn = true;
            Log.Me.Debug("Testing Recipient");            
            int rid = TestCRU<Recipient>( context.RecipientRepo, MakeRecipient(), CheckRecipient,  "Name");
            if (rid==-1) return;

            Log.Me.Debug("Testing PendingReport");            
            int pid = TestCRU<PendingReport>( context.PendingReportRepo, MakePendingReport(rid), CheckPendingReport,  "eFileName");
            if (pid==-1) return;

            Log.Me.Debug("Testing ReportCirculation"); 
            int circid;
            foreach (Recipient r in FillRecipients(people, domain, address)) 
            {
                rid = context.RecipientRepo.Find(r.Email);
                if (rid == 0) rid = context.RecipientRepo.Create(r);
                r.RecipientID = rid;
                circid = context.CirculationRepo.Create(MakeCirculation(r,pid));
            }
            rid = context.RecipientRepo.Find(MakeEmail(people[1], domain));
            Recipient r2 = context.RecipientRepo.Read(rid);
            circid = TestCRU<Circulation>( context.CirculationRepo, MakeCirculation(r2, pid), CheckCirculation,  "Email");
            if (circid==-1) return;

            Log.Me.Debug("Testing StandingData"); 
            StandingData sd = context.StandingDataRepo.Load();
            if (sd.Years.Count() != 5 || sd.Years.First() != DateTime.Now.Year.ToString()) sFail += "StandingData1 ";
            if (sd.Companies.Count() != 4 || sd.Companies.Where(c => c.StartYear==2011).First().Code != "ENT") sFail += "StandingData2 ";
            if (sd.ReportTypes.Count() != 3 || sd.ReportTypes.Where(r => r.Code=="R").First().Name != "Report (R)") sFail += "StandingData3 ";
            if (sd.SecurityLevels.Count() != 6 || sd.SecurityLevels.Where(r => r.Code==100).First().Text != "Secret") sFail += "StandingData4 ";
            if (sd.AccessCodes.Count() != 8 || sd.AccessCodes.Where(a => a.Code=="UIN").First().Title != "Uniper Innovation") sFail += "StandingData5 ";
	
            if (context.PendingReportRepo.GetCID("UTG/20/ASF/AR/2/R")  != null  ) sFail += "GetCID1 ";
            if (context.PendingReportRepo.GetCID("UTG/20/APC/CM/8/TIP")!= 539282) sFail += "GetCID2 ";
            if (context.PendingReportRepo.GetCID("UTG/20/APC/CM/5/R")  != 539281) sFail += "GetCID3 ";
            if (context.PendingReportRepo.GetCID("UTG/19/ASF/FP/543/R")!= 539083) sFail += "GetCID4 ";

            if (sFail.Length == 0) 
            {
                  Console.WriteLine("**********************");
                  Console.WriteLine("** All Tests passed **");
                  Console.WriteLine("**********************");      
            }            
            else 
            {
                  Console.WriteLine("!!!!!!!!!!!!!!!!!!!");                  
                  Console.WriteLine("!! Failed tests: " + sFail);                  
                  Console.WriteLine("!!!!!!!!!!!!!!!!!!!");                                    
            }
        }


        static Recipient MakeRecipient()
        {
            return new Recipient() { Name = "Ignatius O'Brien", JobTitle="Lord Chancellor of Ireland",
                                     Email="ignatius.obrien@gmail.com", UserName="ignatius",
                                     AddressLine1="87 Crown Street", AddressLine2="Westminster", 
                                     AddressLine3="London", AddressLine4="", 
                                     AddressLine5="", PostCode="WC2N 6WY" };
        }  

        static IEnumerable<Recipient> FillRecipients(List<String> people, string domain, string address)
        {
            List<Recipient> list = new List<Recipient>();

            foreach (string s in people)
            {
                list.Add(new Recipient() { Name = s,  Email=MakeEmail(s, domain),  Address=address });
            }

            return list;
        }

        static string MakeEmail(string name, string domain)
        {
            return name.ToLower().Replace(' ', '.') + domain;
        }       

        static bool CheckRecipient(Recipient r, Recipient testdata)
        {
            return (r.FirstName==testdata.FirstName && r.LastName==testdata.LastName && r.JobTitle==testdata.JobTitle &&
                    r.Email==testdata.Email && r.UserName==testdata.UserName && r.AddressLine1==testdata.AddressLine1 && 
                    r.AddressLine2==testdata.AddressLine2 && r.AddressLine3==testdata.AddressLine3 && 
                    r.AddressLine4==testdata.AddressLine4 && r.AddressLine5==testdata.AddressLine5 && 
                    r.PostCode==testdata.PostCode && r.Name==testdata.Name);
        }
       static PendingReport MakePendingReport(int rid)
        {
            return new PendingReport() { 
                ReportNo      = "PT/05/BB956/", 
                ReportType    = "WP        ",
                ReportYear    = "2005      ",
                Title         = "The report title as too long to fit here, so this line of rubbish will have to make do instead.%$£<>\\/'[]()@*+=?,.",
                Author        = "Bloggs F", 
                Abstract      = null, 
                JobNo         = "O500",
                Software      = null,                
                CreationDate  = "16/08/2005".ToDateTime(), 
                Axess         = "NONE",
                SecurityLevel = 50,
                State         = 0,
                Deleted       = false,
                RecipientID   = rid,
                eFilePath     = null,
                eFileName     = null,
                CID           = -1, 
                DateSent      = null
             };
        } 

        static bool CheckPendingReport(PendingReport p, PendingReport testdata)
        {
            return ( 
                p.ReportNo          == testdata.ReportNo          &&
                p.ReportType.Trim() == testdata.ReportType.Trim() &&
                p.ReportYear.Trim() == testdata.ReportYear.Trim() &&
                p.Title             == testdata.Title             &&
                p.Author            == testdata.Author            && 
                p.Abstract          == testdata.Abstract          &&
                p.JobNo             == testdata.JobNo             &&
                p.Software          == testdata.Software          &&
                p.CreationDate      == testdata.CreationDate      &&  
                p.Axess             == testdata.Axess             &&
                p.SecurityLevel     == testdata.SecurityLevel     &&
                p.State             == testdata.State             &&
                p.Deleted           == testdata.Deleted           &&
                p.RecipientID       == testdata.RecipientID       &&
                p.eFilePath         == testdata.eFilePath         &&
                p.eFileName         == testdata.eFileName         &&
                p.CID               == testdata.CID               &&
                p.DateSent          == testdata.DateSent
            );
        } 

        static Circulation MakeCirculation(Recipient r, int pendingId)
        {
            return new Circulation() { Name = r.Name, Email = r.Email, Address = r.Address, 
                                       ToCcBcc = Circulation.eToCcBcc.CC, Delivery = "ES      ",
                                       RecipientID = r.RecipientID, PendingId = pendingId };
        }  

        static bool CheckCirculation(Circulation c, Circulation testdata)
        {
            return ( c.Name==testdata.Name && c.Email==testdata.Email && c.ToCcBcc==testdata.ToCcBcc && c.Delivery==testdata.Delivery);
        } 

        static bool TestAll<T>(IRepository<T> r, T testdata, Func<T, T, bool> checker, string property) where T : class, IID
        {
            int id = TestCreate<T>(r, testdata, checker);
            if (id==-1) return false; 
   
            if (!TestUpdate<T>(r, id, property, testdata, checker)) return false;

            if (!TestDelete<T>(r, id)) return false;   

            Log.Me.Info("TestAll OK for " + typeof(T).ToString() + " - id =" + id.ToString());

            return true;
        }

        static int TestCRU<T>(IRepository<T> r, T testdata, Func<T, T, bool> checker, string property) where T : class, IID
        {
            Log.Me.Debug("Testing Create");
            int id = TestCreate<T>(r, testdata, checker);
            if (id==-1) return id; 

            Log.Me.Debug("Testing Update");
            if (!TestUpdate<T>(r, id, property, testdata, checker)) return -1;

            Log.Me.Info("TestCRU OK for " + typeof(T).ToString() + " - id =" + id.ToString());

            return id;
        }

        static int TestCreate<T>(IRepository<T> r, T testdata, Func<T, T, bool> checker) where T : class, IID
        {
            int id = -1;

            try
            {
                id = r.Create(testdata);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed at Create " + typeof(T).ToString() + " Exception: " + ex.Message);
                return -1;
            }
            
            T t = (T)r.Read(id);       
            if (!checker(t, testdata)) 
            { 
                Console.WriteLine("Failed in TestCreate at Read {0} {1}", typeof(T).ToString(), id); 
                return -1;
            }            

            return id;
        }

        static bool TestUpdate<T>(IRepository<T> r, int id, string property, T testdata, Func<T, T, bool> checker) where T : class, IID
        {
            T t = (T)r.Read(id);       
            if (!checker(t, testdata)) 
            { 
                Console.WriteLine("Failed in TestUpdate at Read {0} {1}", typeof(T).ToString(), id); 
                return false;
            }

            PropertyInfo prop = t.GetType().GetProperty(property);
            string save = (string)prop.GetValue(t);
            prop.SetValue(t, "Rubbish");

            try
            {
                r.Update(t);
            }
            catch (Exception ex)
            { 
                Console.WriteLine("Failed at Update {0}. Error was " + ex.Message, typeof(T).ToString()); 
                return false;
            }

            t = (T)r.Read(id);    
            if (checker(t, testdata)) 
            { 
                Console.WriteLine("{0} Update Failed at {0} {1}", typeof(T).ToString(), id); 
                return false;
            }

            prop.SetValue(t, save);
            try
            {
                r.Update(t);
            }
            catch (Exception ex)
            { 
                Console.WriteLine("Failed at Update {0}. Error was " + ex.Message, typeof(T).ToString()); 
                return false;
            }

            t = (T)r.Read(id);   
            if (!checker(t, testdata)) 
            { 
                Console.WriteLine("{0} Reinstate Failed at {0} {1}", typeof(T).ToString(), id); 
                return false;
            }

            return true;
        }

        static bool TestDelete<T>(IRepository<T> r, int id) where T : class, IID
        {
            try
            {
                r.Delete(id);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            
            T t = (T)r.Read(id);  
            if (t!=null) 
            { 
                Console.WriteLine("{0} {1} still there after delete.", typeof(T).ToString(), id); 
                return false; 
            }

            return true;
        }

    }
}