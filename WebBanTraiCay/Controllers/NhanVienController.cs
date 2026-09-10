using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using WebBanTraiCay.Models;
using Microsoft.AspNet.Identity;

namespace WebBanTraiCay.Controllers
{
    [Authorize]
    public class NhanVienController : BaseController
    {
        WebsiteQuanLyBanTraicayDataContext con = new WebsiteQuanLyBanTraicayDataContext(MatHangController.conn);
        public ActionResult Index()
        {
            if (Role != "admin")
            {
                return new HttpStatusCodeResult(403);
            }
            List<nhanvien> dsnv = con.nhanviens.ToList();
            return View(dsnv);
        }
        public ActionResult SearchNV(FormCollection collection)
        {
            if (Role != "admin")
            {
                return new HttpStatusCodeResult(403);
            }
            List<nhanvien> nv = con.nhanviens.ToList();
            if (collection.Count == 0)
            {
                nv = con.nhanviens.ToList();
            }
            if (!string.IsNullOrEmpty(collection["txtsearch"]))
            {
                nv = nv.Where(sp => sp.tennv.ToLower().Contains(collection["txtsearch"].ToLower())).ToList();
            }
            if (!string.IsNullOrEmpty(collection["txtdiachi"]))
            {
                nv = nv.Where(sp => sp.diachi.ToLower().Contains(collection["txtdiachi"].ToLower())).ToList();
            }
            return PartialView(nv);
        }
        public ActionResult Create()
        {
            if (Role != "admin")
            {
                return new HttpStatusCodeResult(403);
            }
            if (Request.Form.Count > 0)
            {
                try
                {
                    nhanvien nvmax = con.nhanviens.OrderByDescending(x => x.manv).FirstOrDefault();
                    nhanvien nv = new nhanvien();
                    string manv = "";
                    if (nvmax == null)
                    {
                        manv = "NV001";
                    }
                    else
                    {
                        int temp = int.Parse(nvmax.manv.Substring(2, 3)) + 1;
                        string s = "";
                        if (temp < 10)
                        {
                            s += "00" + temp;
                        }
                        else if (temp < 100)
                        {
                            s += "0" + temp;
                        }
                        else
                        {
                            s += temp;
                        }
                        manv = String.Concat(nvmax.manv.Substring(0, 2), s);
                    }
                    nv.manv = manv;
                    nv.tennv = Request.Form["tennv"];
                    nv.diachi = Request.Form["diachi"];
                    nv.sdtnv = Request.Form["sdtnv"];
                    if(nv.tennv == null||nv.sdtnv==null||nv.diachi==null)
                    {
                        TempData["ThongBao"] = $"Thêm nhân viên vào Database thất bại!!! Dữ liệu bạn nhập thiếu";
                        return RedirectToAction("Index");
                    }
                    nv.luongcb = int.Parse(Request.Form["luongcb"]);
                    nv.hesoluong = decimal.Parse(Request.Form["hesoluong"]);
                    HttpPostedFileBase file = Request.Files["hinhanhnv"];
                    if (file != null && file.FileName != "")
                    {
                        string serverpath = HttpContext.Server.MapPath("~/HinhNV");
                        string extension = System.IO.Path.GetExtension(file.FileName);
                        string fileName = manv + extension;
                        string filepath = serverpath + "/" + fileName;
                        file.SaveAs(filepath);
                        nv.hinhanhnv = fileName;
                    }
                    con.nhanviens.InsertOnSubmit(nv);
                    bool CapTaiKhoan = (Request.Form["capTaiKhoan"] == "on" || Request.Form["capTaiKhoan"] == "true");
                    if (CapTaiKhoan)
                    {
                        khachhang a = new khachhang();
                        a.tenkh = nv.tennv;
                        a.makh = nv.manv;
                        a.diachi = nv.diachi;
                        a.sdt = nv.sdtnv;
                        con.khachhangs.InsertOnSubmit(a);
                        var hasher = new PasswordHasher();
                        string[] temptendn = Request.Form["tennv"].ToLower().Split(' ');
                        string tendn = "";
                        if (temptendn.Length < 2)
                            tendn = temptendn[0];
                        else
                            tendn = String.Concat(temptendn[temptendn.Length - 2], temptendn[temptendn.Length - 1]);
                        tendn = chuanhoaten.Chuanhoaten(tendn);
                        string matkhau = String.Concat(tendn, manv);
                        string password = hasher.HashPassword(matkhau);
                        dangnhap account = new dangnhap();
                        account.tendn = tendn + manv.Substring(3, 2);
                        account.manv = manv;
                        account.makh = manv;
                        account.quyen = "nv";
                        account.avartar = "avatar.png";
                        account.matkhau = password;
                        con.dangnhaps.InsertOnSubmit(account);
                    }
                    con.SubmitChanges();
                    TempData["ThongBao"] = $"Đã thêm nhân viên {manv} vào Database thành công";
                }
                catch
                {
                    TempData["ThongBao"] = $"Thêm nhân viên vào Database thất bại!!!";
                }
                return RedirectToAction("Index");
            }
            return View();
        }
        public ActionResult Delete(string id)
        {
            if (Role != "admin")
            {
                return new HttpStatusCodeResult(403);
            }
            nhanvien nv = con.nhanviens.FirstOrDefault(sp => sp.manv == id);
            try
            {
                if (nv != null)
                {
                    string fullPath = Request.MapPath("~/HinhNV/" + nv.hinhanhnv);
                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                    }
                    dangnhap dn = con.dangnhaps.FirstOrDefault(d => d.manv == nv.manv);
                    if (dn != null)
                        con.dangnhaps.DeleteOnSubmit(dn);
                    khachhang a=con.khachhangs.FirstOrDefault(x => x.makh == nv.manv);
                    if (a != null)
                        con.khachhangs.DeleteOnSubmit(a);
                    con.nhanviens.DeleteOnSubmit(nv);
                    con.SubmitChanges();
                }
            }
            catch
            {
                TempData["Xoa"] = $"Xóa thất bại!!! Nhân viên {id} đã thực hiện lập đơn hàng hoặc phiếu nhập";
            }
            return RedirectToAction("Index");
        }
        public ActionResult Edit(string id)
        {
            if (Role != "admin")
            {
                return new HttpStatusCodeResult(403);
            }
            try
            {
                nhanvien nv = con.nhanviens.FirstOrDefault(sp => sp.manv == id);
                if (Request.Form.Count == 0)
                {
                    return View(nv);
                }
                nv.tennv = Request.Form["tennv"];
                nv.diachi = Request.Form["diachi"];
                nv.sdtnv = Request.Form["sdtnv"];
                if (nv.tennv == null || nv.sdtnv == null || nv.diachi == null)
                {
                    TempData["ThongBao"] = $"Chỉnh sửa nhân viên {id} thất bại!!! Dữ liệu bạn nhập thiếu";
                    return RedirectToAction("Index");
                }
                nv.luongcb = int.Parse(Request.Form["luongcb"]);
                nv.hesoluong = decimal.Parse(Request.Form["hesoluong"]);
                HttpPostedFileBase file = Request.Files["hinhanhnv"];
                if (file != null && file.FileName != "")
                {
                    string serverpath = HttpContext.Server.MapPath("~/HinhNV");
                    string extension = System.IO.Path.GetExtension(file.FileName);
                    string fileName = nv.manv + extension;
                    string filepath = serverpath + "/" + fileName;
                    file.SaveAs(filepath);
                    nv.hinhanhnv = fileName;
                }
                khachhang a = con.khachhangs.FirstOrDefault(x => x.makh == nv.manv);
                if(a != null)
                {
                    a.tenkh = nv.tennv;
                    a.diachi = nv.diachi;
                    a.sdt = nv.sdtnv;
                }
                con.SubmitChanges();
                TempData["ThongBao"] = $"Chỉnh sửa nhân viên {id} thành công";
            }
            catch
            {
                TempData["ThongBao"] = $"Chỉnh sửa nhân viên {id} thất bại";
            }
            return RedirectToAction("Index");
        }
        public ActionResult Details(string id)
        {
            if (Role != "admin")
            {
                return new HttpStatusCodeResult(403);
            }
            nhanvien nv = con.nhanviens.FirstOrDefault(sp => sp.manv == id);
            return View(nv);
        }
    }
}