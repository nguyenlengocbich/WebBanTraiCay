using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WebBanTraiCay.Models;
using Microsoft.AspNet.Identity;
using System.IO;
using System.Web.UI.WebControls;
using Microsoft.SqlServer.Management.Smo;

namespace WebBanTraiCay.Controllers
{
    public class AccountController : BaseController
    {
        // GET: Account
        WebsiteQuanLyBanTraicayDataContext con = new WebsiteQuanLyBanTraicayDataContext(MatHangController.conn);
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Register(AccountKH acc)
        {
            if (ModelState.IsValid)
            {
                dangnhap temp = con.dangnhaps.FirstOrDefault(x => x.tendn == acc.tendn);
                if (temp != null)
                {
                    ModelState.AddModelError("matkhau", "Tên đăng nhập này đã tồn tại");
                    return View(acc);
                }
                if (acc.matkhau != acc.ConfirmPassword)
                {
                    ModelState.AddModelError("matkhau", "Mật khẩu xác nhận không đúng");
                    return View(acc);
                }
                khachhang khmax = con.khachhangs.OrderByDescending(x => x.makh).Where(ma => !ma.makh.StartsWith("NV")).FirstOrDefault();
                khachhang kh = new khachhang();
                string makh = "";
                if (khmax == null)
                {
                    makh = "KH001";
                }
                else
                {
                    int tempkh = int.Parse(khmax.makh.Substring(2, 3)) + 1;
                    string s = "";
                    if (tempkh < 10)
                    {
                        s += "00" + tempkh;
                    }
                    else if (tempkh < 100)
                    {
                        s += "0" + tempkh;
                    }
                    else
                    {
                        s += tempkh;
                    }
                    makh = String.Concat(khmax.makh.Substring(0, 2), s);
                }
                kh.makh = makh;
                kh.tenkh = acc.hovaten;
                kh.sdt = acc.sodienthoai;
                kh.diachi = acc.diachi;
                var hasher = new PasswordHasher();
                string password = hasher.HashPassword(acc.matkhau);
                dangnhap account = new dangnhap();
                account.tendn = acc.tendn;
                account.makh = makh;
                account.quyen = "kh";
                account.matkhau = password;
                account.avartar = "avatar.png";
                con.khachhangs.InsertOnSubmit(kh);
                con.dangnhaps.InsertOnSubmit(account);
                TempData["ThongBao"] = "Đăng ký tài khoản thành công";
                con.SubmitChanges();
                return RedirectToAction("Login");
            }
            return View(acc);
        }
        [HttpPost]
        public ActionResult Login(dangnhap acc, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var hasher = new PasswordHasher();
                dangnhap temp = con.dangnhaps.FirstOrDefault(x => x.tendn == acc.tendn);
                if (temp != null && hasher.VerifyHashedPassword(temp.matkhau, acc.matkhau) == PasswordVerificationResult.Success)
                {
                    FormsAuthentication.SetAuthCookie(temp.tendn, true);
                    if (temp.quyen == "admin" || temp.quyen == "nv")
                    {
                        nhanvien a = con.nhanviens.FirstOrDefault(x => x.manv == temp.manv);
                        string hoten = a.tennv;
                        string hinhanh = a.hinhanhnv;
                        string userData = $"{temp.quyen}|{temp.manv}|{temp.makh}|{hoten}|{hinhanh}";
                        FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(1, temp.tendn, DateTime.Now, DateTime.Now.AddHours(8), true, userData);
                        string encrypted = FormsAuthentication.Encrypt(ticket);
                        Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, encrypted)
                        {
                            HttpOnly = true,
                            Expires = ticket.Expiration
                        });
                        if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                            && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                        {
                            return Redirect(returnUrl);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else if (temp.quyen == "kh")
                    {
                        khachhang a = con.khachhangs.FirstOrDefault(x => x.makh == temp.makh);
                        string hoten = a.tenkh;
                        string hinhanh = temp.avartar;
                        string userData = $"{temp.quyen}|{temp.manv}|{temp.makh}|{hoten}|{hinhanh}";
                        FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(1, temp.tendn, DateTime.Now, DateTime.Now.AddHours(8), true, userData);
                        string encrypted = FormsAuthentication.Encrypt(ticket);
                        Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, encrypted)
                        {
                            HttpOnly = true,
                            Expires = ticket.Expiration
                        });
                        if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                            && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                        {
                            return Redirect(returnUrl);
                        }
                        else
                        {
                            return RedirectToAction("Index", "MuiNeFruit");
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("matkhau", "Sai tên đăng nhập hoặc mật khẩu");
                }
            }
            return View(acc);
        }
        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            if (Request.Cookies[FormsAuthentication.FormsCookieName] != null)
            {
                var cookie = new HttpCookie(FormsAuthentication.FormsCookieName)
                {
                    Expires = DateTime.Now.AddDays(-1),
                    HttpOnly = true
                };
                Response.Cookies.Add(cookie);
            }
            return RedirectToAction("Login", "Account");
        }
        [Authorize]
        [HttpGet]
        public ActionResult PasswordChange()
        {
            string backUrl = Request.UrlReferrer != null ? Request.UrlReferrer.ToString() : "/";
            ViewBag.ReturnUrl = backUrl;
            return View();
        }
        [Authorize]
        [HttpPost]
        public ActionResult PasswordChange(Account acc, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var hasher = new PasswordHasher();
                dangnhap temp = con.dangnhaps.FirstOrDefault(x => x.tendn == User.Identity.Name);
                if (temp == null && !(hasher.VerifyHashedPassword(temp.matkhau, acc.matkhau) == PasswordVerificationResult.Success))
                {
                    ModelState.AddModelError("matkhau", "Mật khẩu bạn nhập không chính xác!!!");
                    return View(acc);
                }
                if (acc.newmatkhau != acc.ConfirmPassword)
                {
                    ModelState.AddModelError("matkhau", "Mật khẩu xác nhận không đúng");
                    return View(acc);
                }
                string password = hasher.HashPassword(acc.newmatkhau);
                temp.matkhau = password;
                con.SubmitChanges();
                TempData["SuccessMessage"] = "Chúc mừng! Bạn đã đổi mật khẩu thành công.";
                if (!string.IsNullOrEmpty(returnUrl))
                    return Redirect(returnUrl);
                else
                    return RedirectToAction("Index", "MuiNeFruit");
            }
            ViewBag.ReturnUrl = returnUrl;
            return View(acc);
        }
        [Authorize]
        public ActionResult UserProfile(string returnUrl)
        {
            khachhang kh = con.khachhangs.FirstOrDefault(x => x.makh == Makh);
            if (kh == null)
            {
                return new HttpStatusCodeResult(404);
            }
            string currentUrl = Request.Url.AbsolutePath;
            string backUrl = "/";
            if (!string.IsNullOrEmpty(returnUrl))
            {
                backUrl = returnUrl;
            }
            else if (Request.UrlReferrer != null)
            {
                string referrerUrl = Request.UrlReferrer.PathAndQuery;
                if (!referrerUrl.Contains(currentUrl))
                {
                    backUrl = referrerUrl;
                }
            }

            ViewBag.ReturnUrl = backUrl;
            return View(kh);
        }
        [Authorize]
        public ActionResult NVProfile(string returnUrl)
        {
            nhanvien nv = con.nhanviens.FirstOrDefault(x => x.manv == Manv);
            if (nv == null)
            {
                return new HttpStatusCodeResult(404);
            }
            string currentUrl = Request.Url.AbsolutePath;
            string backUrl = "/";
            if (!string.IsNullOrEmpty(returnUrl))
            {
                backUrl = returnUrl;
            }
            else if (Request.UrlReferrer != null)
            {
                string referrerUrl = Request.UrlReferrer.PathAndQuery;
                if (!referrerUrl.Contains(currentUrl))
                {
                    backUrl = referrerUrl;
                }
            }
            ViewBag.ReturnUrl = backUrl;
            return View(nv);
        }
        [HttpPost]
        [Authorize]
        public ActionResult UpdateProfile(FormCollection form, int i,string returnUrl)
        {
            if (i == 1)
            {
                khachhang kh = con.khachhangs.FirstOrDefault(x => x.makh == Makh);
                if (kh != null)
                {
                    string tenMoi = form["hoten"];
                    string sdtMoi = form["sdt"];
                    string diaChiMoi = form["diachi"];
                    if (string.IsNullOrEmpty(tenMoi) || string.IsNullOrEmpty(sdtMoi)||string.IsNullOrEmpty(diaChiMoi))
                    {
                        TempData["ThongBao"] = "Vui lòng nhập đầy thông tin!";
                        return RedirectToAction("UserProfile");
                    }
                    kh.tenkh = tenMoi;
                    kh.sdt = sdtMoi;
                    kh.diachi = diaChiMoi;
                    dangnhap dn = con.dangnhaps.FirstOrDefault(x => x.makh == Makh);
                    HttpPostedFileBase file = Request.Files["hinhanh"];
                    if (file != null && file.FileName != "")
                    {
                        string serverpath = HttpContext.Server.MapPath("~/HinhKH");
                        string extension = System.IO.Path.GetExtension(file.FileName);
                        string fileName = kh.makh + extension;
                        string filepath = serverpath + "/" + fileName;
                        file.SaveAs(filepath);
                        dn.avartar = fileName;
                    } 
                    con.SubmitChanges();
                    TempData["ThongBao"] = "Cập nhật thông tin thành công!";
                    UpdateUserTicket(kh.tenkh, dn.avartar);
                }
                return RedirectToAction("UserProfile", new { returnUrl = returnUrl });
            }
            else if (i == 0)
            {
                nhanvien nv = con.nhanviens.FirstOrDefault(n => n.manv == Manv);
                if (nv != null)
                {
                    string tenMoi = form["hoten"];
                    string sdtMoi = form["sdt"];
                    string diaChiMoi = form["diachi"];
                    if (string.IsNullOrEmpty(tenMoi) || string.IsNullOrEmpty(sdtMoi) || string.IsNullOrEmpty(diaChiMoi))
                    {
                        TempData["ThongBao"] = "Vui lòng nhập đầy thông tin!";
                        return RedirectToAction("NVProfile");
                    }
                    nv.tennv = tenMoi;
                    nv.sdtnv = sdtMoi;
                    nv.diachi = diaChiMoi;
                    HttpPostedFileBase file = Request.Files["hinhanh"];
                    if (file != null && file.FileName != "")
                    {
                        string serverpath = HttpContext.Server.MapPath("~/HinhNV");
                        string extension = System.IO.Path.GetExtension(file.FileName);
                        string fileName = nv.manv + extension;
                        string filepath = serverpath + "/" + fileName;
                        file.SaveAs(filepath);
                        nv.hinhanhnv = fileName;
                    }
                    con.SubmitChanges();
                    ViewBag.hoten = nv.tennv;
                    TempData["ThongBao"] = "Cập nhật thông tin thành công!"; 
                    UpdateUserTicket(nv.tennv, nv.hinhanhnv);
                }
                return RedirectToAction("NVProfile", new { returnUrl = returnUrl });
            }
            else
            {
                return new HttpStatusCodeResult(404);
            }
        }
    }
}