using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanTraiCay.Models;

namespace WebBanTraiCay.Controllers
{
    [Authorize]
    public class LoaiMatHangController : BaseController
    {
        // GET: LoaiMatHang
        WebsiteQuanLyBanTraicayDataContext con = new WebsiteQuanLyBanTraicayDataContext(MatHangController.conn);
        public ActionResult Index()
        {
            if (Role != "admin" && Role != "nv")
            {
                return new HttpStatusCodeResult(403);
            }
            List<loaimathang> DsCatalog = con.loaimathangs.ToList();
            return View(DsCatalog);
        }
        public ActionResult SearchLMH(FormCollection collection)
        {
            if (Role != "admin" && Role != "nv")
            {
                return new HttpStatusCodeResult(403);
            }
            List<loaimathang> DsProduct = con.loaimathangs.ToList();
            if (collection.Count == 0)
            {
                DsProduct = con.loaimathangs.ToList();
            }
            if (!string.IsNullOrEmpty(collection["txtsearch"]))
            {
                DsProduct = DsProduct.Where(sp => sp.tenloai.ToLower().Contains(collection["txtsearch"].ToLower())).ToList();
            }
            return PartialView(DsProduct);
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
                    loaimathang lmathangmax = con.loaimathangs.OrderByDescending(x => x.maloai).FirstOrDefault();
                    string maloai = "";
                    if (lmathangmax == null)
                    {
                        maloai = "LH001";
                    }
                    else
                    {
                        int temp = int.Parse(lmathangmax.maloai.Substring(2, 3)) + 1;
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
                        maloai = String.Concat(lmathangmax.maloai.Substring(0, 2), s);
                    }
                    string catalogCode = maloai;
                    string catalogName = Request.Form["tenloai"];
                    loaimathang catalog = new loaimathang();
                    catalog.maloai = catalogCode;
                    catalog.tenloai = catalogName;
                    if (catalog.tenloai == null)
                    {
                        TempData["ThongBao"] = $"Thêm loại mặt hàng thất bại";
                        return RedirectToAction("Index");
                    }
                    con.loaimathangs.InsertOnSubmit(catalog);
                    con.SubmitChanges();
                    TempData["ThongBao"] = $"Đã thêm loại mặt hàng {maloai} vào Database thành công";
                }
                catch
                {
                    TempData["ThongBao"] = $"Thêm loại mặt hàng thất bại";
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
            loaimathang lmh = con.loaimathangs.FirstOrDefault(sp => sp.maloai == id);
            try
            {
                if (lmh != null)
                {
                    con.loaimathangs.DeleteOnSubmit(lmh);
                    con.SubmitChanges();
                }
            }
            catch
            {
                TempData["Xoa"] = $"Xóa thất bại!!! Tồn tại mặt hàng thuộc loại mặt hàng {id}";
            }
            return RedirectToAction("Index");
        }
        public ActionResult Details(string id)
        {
            if (Role != "admin" && Role != "nv")
            {
                return new HttpStatusCodeResult(403);
            }
            List<string> mathangs= con.mathangs.Where(x => x.maloai == id).Select(x=>x.mamh).ToList();
            List<sp_XemMhResult> xemmathang = con.sp_XemMh().Where(x => mathangs.Contains(x.mamh)).ToList();
            return View(xemmathang);
        }
        public ActionResult Edit(string id )
        {
            if (Role != "admin" && Role != "nv")
            {
                return new HttpStatusCodeResult(403);
            }
            try
            {
                loaimathang lmh = con.loaimathangs.FirstOrDefault(sp => sp.maloai == id);
                if (Request.Form.Count == 0)
                {
                    return View(lmh);
                }
                lmh.tenloai = Request.Form["tenloai"];
                if (lmh.tenloai == null)
                {
                    TempData["ThongBao"] = $"Chỉnh sửa loại mặt hàng thất bại";
                    return RedirectToAction("Index");
                }
                con.SubmitChanges();
                TempData["ThongBao"] = $"Chỉnh sửa loại mặt hàng {id} thành công";
            }
            catch
            {
                TempData["ThongBao"] = $"Chỉnh sửa loại mặt hàng {id} thất bại";
            }
            return RedirectToAction("Index");
        }
    }
}