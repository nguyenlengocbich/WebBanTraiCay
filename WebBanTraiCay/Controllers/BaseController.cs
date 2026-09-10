using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace WebBanTraiCay.Controllers
{
    public class BaseController : Controller
    {
        protected string Role = "";
        protected string Manv;
        protected string Makh;
        protected string HoTen;
        protected string HinhAnh;
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (User.Identity.IsAuthenticated == true)
            {
                var identity = (FormsIdentity)User.Identity;
                var ticket = identity.Ticket;
                if (ticket != null)
                {
                    var data = ticket.UserData.Split('|');
                    Role = data.ElementAtOrDefault(0);
                    Manv = data.ElementAtOrDefault(1);
                    Makh = data.ElementAtOrDefault(2);
                    HoTen = data.ElementAtOrDefault(3);
                    HinhAnh = data.ElementAtOrDefault(4);
                    ViewBag.makh = Makh;
                    ViewBag.role = Role;
                    ViewBag.hoten = HoTen;
                    ViewBag.avatar = HinhAnh;
                }
                base.OnActionExecuting(filterContext);
            }
        }
        public void UpdateUserTicket(string newHoTen, string newHinhAnh)
        {
            this.HoTen = newHoTen;
            this.HinhAnh = newHinhAnh;
            ViewBag.hoten = newHoTen;
            ViewBag.avatar = newHinhAnh;
            string userData = string.Join("|", this.Role, this.Manv, this.Makh, newHoTen, newHinhAnh);
            var oldCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            if (oldCookie != null)
            {
                var oldTicket = FormsAuthentication.Decrypt(oldCookie.Value);
                var newTicket = new FormsAuthenticationTicket(oldTicket.Version,oldTicket.Name,oldTicket.IssueDate,oldTicket.Expiration,oldTicket.IsPersistent,userData,oldTicket.CookiePath);
                string hash = FormsAuthentication.Encrypt(newTicket);
                HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, hash);
                cookie.HttpOnly = true;
                if (newTicket.IsPersistent)
                {
                    cookie.Expires = newTicket.Expiration;
                }
                Response.Cookies.Add(cookie);
            }
        }
    }
}