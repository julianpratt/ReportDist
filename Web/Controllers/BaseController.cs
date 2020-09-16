using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReportDist.Data;
using ReportDist.Models;
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

        /*
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
        */
        protected User CheckIdentity()
        {
            ClaimsIdentity identity = User.Identity as ClaimsIdentity;
            if (identity == null) return null;

            string login = identity?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            string key   = "user-" + login; 

            User user = Cache<User>.GetCache().Get(key, () => IdentifyUser(identity)); 

            if (user == null)
            {
                Log.Me.Fatal("Failed to get user identity in GetUserIdentity()");
                return null;
            }

            ViewData["UserId"]   = user.UserId;
            ViewData["UserName"] = user.UserName;
            
            return user;
        }

        private User IdentifyUser(ClaimsIdentity identity)
        {            
            User user = null;

            try
            {
                Recipient r = _context.RecipientRepo.Identify(identity);
                if (r != null)
                {
                    user = new User();
                    user.UserId   = r.Id.ToString();
                    user.UserName = r.Name;
                }
                else
                {
                    Log.Me.Fatal("User NOT identified. This is often caused by lack of database connection.");
                    return null;
                }
            }    
            catch (Exception ex)
            {
                Log.Me.Fatal("Exception in IdentifyUser(): " + ex.Message);
                return null;
            }        

            return user;
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