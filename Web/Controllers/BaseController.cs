using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReportDist.Data;
using Mistware.Utils;

namespace ReportDist.Controllers
{
    public class BaseController : Controller
    {
        protected readonly DataContext _context;
        
        public BaseController(DataContext context)
        {
            _context = context;
        }

        protected string CheckIdentity()
        {
            string name = "";
            try
            {
                // Avoid database hit if we can
                string id    = HttpContext.Session.GetString("UserId");
                       name  = HttpContext.Session.GetString("UserName");
                string login = HttpContext.Session.GetString("UserLogin");

                ClaimsIdentity identity = User.Identity as ClaimsIdentity;
                if (identity != null && login != null && identity?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value == login)
                {
                    // We have the user's identity
                }
                else
                {
                    // Identify the user
                    Recipient r = _context.RecipientRepo.Identify(identity);
                    if (r != null)
                    {
                        id    = r.Id.ToString();
                        name  = r.Name;
                        login = r.UserName;
                        HttpContext.Session.SetString("UserId",    id);
                        HttpContext.Session.SetString("UserName",  name);
                        HttpContext.Session.SetString("UserLogin", login);
                    }
                    else
                    {
                        Log.Me.Fatal("User NOT identified. This is often caused by lack of database connection.");
                        return null;
                    }
                }
                
                ViewData["UserId"]   = id; 
                ViewData["UserName"] = name;
            }
            catch (Exception ex)
            {
                Log.Me.Fatal("Exception in BaseController.CheckIdentity(): " + ex.Message);
                return null;
            }    

            return name;
        }

        protected void ApplicationVersion()
        {
            ViewData["AppVersion"] = Config.Get("AppVersion");
        }


        protected bool IsNull(object o, string name, string method)
        {
            if (o == null) 
            {
                Log.Me.Error("Null " + name + " in " + method);
                return true;
            }
            return false;
        }
        protected bool NullId(int? id, string name, string method)
        {
            if (!id.HasValue || id == null) 
            {
                Log.Me.Error("Null " + name + " in " + method);
                return true;
            }
            return false;
        }

        protected bool ZeroId(int id, string name, string method)
        {
            if (id <= 0)
            {
                Log.Me.Error("Zero " + name + " in " + method);
                return true; 
            }
            return false;
        }
    }
}