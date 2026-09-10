using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanTraiCay.Models;

namespace WebBanTraiCay.Controllers
{
    [Authorize]
    public class KhachHangController : BaseController
    {
        // GET: KhachHang
        WebsiteQuanLyBanTraicayDataContext con = new WebsiteQuanLyBanTraicayDataContext(MatHangController.conn);
        public ActionResult Index()
        {
            if (Role != "admin" && Role != "nv")
            {
                return new HttpStatusCodeResult(403);
            }
            List<khachhang> kh = con.khachhangs.Where(x=>x.makh.StartsWith("KH")).ToList();
            return View(kh);
        }
        public ActionResult SearchKH(FormCollection collection)
        {
            if (Role != "admin" && Role != "nv")
            {
                return new HttpStatusCodeResult(403);
            }
            List<khachhang> kh = con.khachhangs.Where(x => x.makh.StartsWith("KH")).ToList();
            if (collection.Count == 0)
            {
                kh = con.khachhangs.Where(x => x.makh.StartsWith("KH")).ToList();
            }
            if (!string.IsNullOrEmpty(collection["txtsearch"]))
            {
                kh = kh.Where(sp => sp.tenkh.ToLower().Contains(collection["txtsearch"].ToLower())).ToList();
            }
            if (!string.IsNullOrEmpty(collection["txtdiachi"]))
            {
                kh = kh.Where(sp => sp.diachi.ToLower().Contains(collection["txtdiachi"].ToLower())).ToList();
            }
            return PartialView(kh);
        }
        public ActionResult Create()
        {
            if (Role != "admin" && Role != "nv")
            {
                return new HttpStatusCodeResult(403);
            }
            if (Request.Form.Count > 0)
            {
                try
                {
                    khachhang khmax = con.khachhangs.OrderByDescending(x => x.makh).Where(ma=>!ma.makh.StartsWith("NV")).FirstOrDefault();
                    khachhang kh = new khachhang();
                    string makh = "";
                    if (khmax == null)
                    {
                        makh = "KH001";
                    }
                    else
                    {
                        int temp = int.Parse(khmax.makh.Substring(2, 3)) + 1;
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
                        makh = String.Concat(khmax.makh.Substring(0, 2), s);
                    }
                    kh.makh = makh;
                    kh.tenkh = Request.Form["tenkh"];
                    kh.diachi = Request.Form["diachi"];
                    kh.sdt = Request.Form["sdt"];
                    if(kh.tenkh == null || kh.diachi == null || kh.sdt == null)
                    {
                        TempData["ThongBao"] = $"Thêm khách hàng thất bại";
                        return RedirectToAction("Index");
                    }
                    con.khachhangs.InsertOnSubmit(kh);
                    con.SubmitChanges();
                    TempData["ThongBao"] = $"Đã thêm khách hàng {makh} vào Database thành công";
                }
                catch
                {
                    TempData["ThongBao"] = $"Thêm khách hàng thất bại";
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
            khachhang kh = con.khachhangs.FirstOrDefault(sp => sp.makh == id);
            try
            {
                if (kh != null)
                {
                    dangnhap dn = con.dangnhaps.FirstOrDefault(x => x.makh == kh.makh);
                    if (dn != null)
                        con.dangnhaps.DeleteOnSubmit(dn);
                    con.khachhangs.DeleteOnSubmit(kh);
                    con.SubmitChanges();
                }
            }
            catch
            {
                TempData["Xoa"] = $"Xóa thất bại!!! Khách hàng {id} đã thực hiện giao dịch";
            }
            return RedirectToAction("Index");
        }
        public ActionResult Edit(string id)
        {
            if (Role != "admin" && Role != "nv")
            {
                return new HttpStatusCodeResult(403);
            }
            try
            {
                khachhang ncc = con.khachhangs.FirstOrDefault(sp => sp.makh == id);
                if (Request.Form.Count == 0)
                {
                    return View(ncc);
                }
                ncc.tenkh = Request.Form["tenkh"];
                ncc.diachi = Request.Form["diachi"];
                ncc.sdt = Request.Form["sdt"];
                if (ncc.tenkh == null || ncc.diachi == null || ncc.sdt == null)
                {
                    TempData["ThongBao"] = $"Chỉnh sửa khách hàng {id} thất bại";
                    return RedirectToAction("Index");
                }
                con.SubmitChanges();
                TempData["ThongBao"] = $"Chỉnh sửa khách hàng {id} thành công";
            }
            catch
            {
                TempData["ThongBao"] = $"Chỉnh sửa khách hàng {id} thất bại";
            }
            return RedirectToAction("Index");
        }
        public ActionResult Details(string id)
        {
            if (Role != "admin" && Role != "nv")
            {
                return new HttpStatusCodeResult(403);
            }
            khachhang kh = con.khachhangs.FirstOrDefault(sp => sp.makh == id);
            return View(kh);
        }
    }
}