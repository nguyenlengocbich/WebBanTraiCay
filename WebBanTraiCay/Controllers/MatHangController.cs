using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using WebBanTraiCay.Models;

namespace WebBanTraiCay.Controllers
{
    [Authorize]
    public class MatHangController : BaseController
    {
        // GET: MatHang
        public static string conn = "Data Source=.;Initial Catalog=qlchtraicay;Integrated Security=True;Encrypt=False";
        WebsiteQuanLyBanTraicayDataContext con = new WebsiteQuanLyBanTraicayDataContext(conn);
        public ActionResult Index()
        {
            if (Role != "admin" && Role != "nv")
            {
                return new HttpStatusCodeResult(403);
            }
            List<loaimathang> lmh = con.loaimathangs.ToList();
            ViewBag.loaimh = lmh;
            List<sp_XemMhResult> DsProduct = con.sp_XemMh().ToList();
            return View(DsProduct);
        }
        public ActionResult SearchMH(FormCollection collection)
        {
            if (Role != "admin" && Role != "nv")
            {
                return new HttpStatusCodeResult(403);
            }
            List<sp_XemMhResult> DsProduct = con.sp_XemMh().ToList();
            List<mathang> Product = con.mathangs.ToList();
            if(collection.Count == 0)
            {
                DsProduct = con.sp_XemMh().ToList();
            }
            if (!string.IsNullOrEmpty(collection["txtsearch"]))
            {
                DsProduct = DsProduct.Where(sp => sp.tenmh.ToLower().Contains(collection["txtsearch"].ToLower())).ToList();
            }
            if (string.IsNullOrEmpty(collection["txtMin"]) && !string.IsNullOrEmpty(collection["txtMax"]))
            {
                double max = double.Parse(collection["txtMax"]);
                DsProduct = DsProduct.Where(sp => sp.giaban <= max).ToList();
            }
            else if (string.IsNullOrEmpty(collection["txtMax"]) && !string.IsNullOrEmpty(collection["txtMin"]))
            {
                double min = double.Parse(collection["txtMin"]);
                DsProduct = DsProduct.Where(sp => sp.giaban >= min).ToList();
            }
            else if(!string.IsNullOrEmpty(collection["txtMax"]) && !string.IsNullOrEmpty(collection["txtMin"]))
            {
                double min = double.Parse(collection["txtMin"]);
                double max = double.Parse(collection["txtMax"]);
                DsProduct = DsProduct.Where(sp => sp.giaban <= max && sp.giaban >= min).ToList();
            }
            if (string.IsNullOrEmpty(collection["txtSoluongMin"]) && !string.IsNullOrEmpty(collection["txtSoluongMax"]))
            {
                double max = double.Parse(collection["txtSoluongMax"]);
                DsProduct = DsProduct.Where(sp => sp.soluong <= max).ToList();
            }
            else if (string.IsNullOrEmpty(collection["txtSoluongMax"]) && !string.IsNullOrEmpty(collection["txtSoluongMin"]))
            {
                double min = double.Parse(collection["txtSoluongMin"]);
                DsProduct = DsProduct.Where(sp => sp.soluong >= min).ToList();
            }
            else if (!string.IsNullOrEmpty(collection["txtSoluongMax"]) && !string.IsNullOrEmpty(collection["txtSoluongMin"]))
            {
                double min = double.Parse(collection["txtSoluongMin"]);
                double max = double.Parse(collection["txtSoluongMax"]);
                DsProduct = DsProduct.Where(sp => sp.soluong <= max && sp.soluong >= min).ToList();
            }
            if (!string.IsNullOrEmpty(collection["loai"]))
            {
                Product = Product.Where(p => p.maloai == collection["loai"]).ToList();
                DsProduct = DsProduct.Where(x => Product.Any(sp => sp.mamh == x.mamh)).ToList();
            }
            if (!string.IsNullOrEmpty(collection["tinhtrang"]))
                DsProduct = DsProduct.Where(p => p.tinhtrang == collection["tinhtrang"]).ToList();
            return PartialView(DsProduct);
        }
        [HttpGet]
        public ActionResult Create()
        {
            if (Role != "admin" && Role != "nv")
            {
                return new HttpStatusCodeResult(403);
            }
            List<loaimathang> lmh = con.loaimathangs.ToList();
            ViewBag.loaimh = new SelectList(lmh, "maloai", "tenloai", lmh.FirstOrDefault().maloai);
            return View();
        }
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            if (Role != "admin" && Role != "nv")
            {
                return new HttpStatusCodeResult(403);
            }
            if (collection.Count > 0)
            {
                try
                {
                    mathang mathangmax = con.mathangs.OrderByDescending(x => x.mamh).FirstOrDefault();
                    mathang sp = new mathang();
                    string mamh = "";
                    if (mathangmax == null)
                    {
                        mamh = "MH001";
                    }
                    else
                    {
                        int temp = int.Parse(mathangmax.mamh.Substring(2, 3)) + 1;
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
                        mamh = String.Concat(mathangmax.mamh.Substring(0, 2), s);
                    }
                    sp.mamh = mamh;
                    sp.tenmh = collection["tenmh"];
                    sp.soluong = int.Parse(collection["soluong"]);
                    sp.giaban = int.Parse(collection["giaban"]);
                    sp.dvtinh = collection["dvtinh"];
                    sp.maloai = collection["maloai"];
                    sp.tinhtrang = "Còn hàng";
                    sp.mota = collection["mota"];
                    if(sp.tenmh == null || sp.dvtinh == null)
                    {
                        TempData["ThongBao"] = $"Thêm mặt hàng vào Database thất bại!! Dữ liệu bạn nhập thiếu!";
                        return RedirectToAction("Index");
                    }
                    HttpPostedFileBase file = Request.Files["hinhanhmh"];
                    if (file != null && file.FileName != "")
                    {
                        string serverpath = HttpContext.Server.MapPath("~/Hinh");
                        string extension = System.IO.Path.GetExtension(file.FileName);
                        string fileName = mamh + extension;
                        string filepath = serverpath + "/" + fileName;
                        file.SaveAs(filepath);
                        sp.hinhanhmh = fileName;
                    }
                    con.mathangs.InsertOnSubmit(sp);
                    con.SubmitChanges();
                    TempData["ThongBao"] = $"Thêm mặt hàng {mamh} vào Database thành công";
                }
                catch
                {
                    TempData["ThongBao"] = $"Thêm mặt hàng vào Database thất bại";
                }
                return RedirectToAction("Index");
            }
            return View();
        }
        public ActionResult Edit(string id)
        {
            if (Role != "admin" && Role != "nv")
            {
                return new HttpStatusCodeResult(403);
            }
            mathang product = con.mathangs.FirstOrDefault(sp => sp.mamh == id);
            List<loaimathang> lmh = con.loaimathangs.ToList();
            ViewBag.loaimh = new SelectList(lmh, "maloai", "tenloai", lmh.FirstOrDefault()?.maloai);
            return View(product);
        }
        [HttpPost]
        public ActionResult Edit(string id, FormCollection collection)
        {
            if (Role != "admin" && Role != "nv")
            {
                return new HttpStatusCodeResult(403);
            }
            try
            {
                mathang product = con.mathangs.FirstOrDefault(sp => sp.mamh == id);
                if (collection.Count == 0)
                {
                    return View(product);
                }
                product.tenmh = collection["tenmh"];
                product.soluong = int.Parse(collection["soluong"]);
                product.giaban = int.Parse(collection["giaban"]);
                product.dvtinh = collection["dvtinh"];
                product.maloai = collection["maloai"];
                product.mota = collection["mota"];
                HttpPostedFileBase file = Request.Files["hinhanhmh"];
                if (file != null && file.FileName != "")
                {
                    string serverpath = HttpContext.Server.MapPath("~/Hinh");
                    string extension = System.IO.Path.GetExtension(file.FileName);
                    string fileName = product.mamh + extension;
                    string filepath = serverpath + "/" + fileName;
                    file.SaveAs(filepath);
                    product.hinhanhmh = fileName;
                }
                if (product.tenmh == null || product.dvtinh == null)
                {
                    TempData["ThongBao"] = $"Chỉnh sửa mặt hàng {id} thất bại!! Dữ liệu bạn nhập thiếu!";
                    return RedirectToAction("Index");
                }
                con.SubmitChanges();
                TempData["ThongBao"] = $"Chỉnh sửa mặt hàng {id} thành công";
            }
            catch
            {
                TempData["ThongBao"] = $"Chỉnh sửa mặt hàng {id} thất bại";
            }
            return RedirectToAction("Index");
        }
        public ActionResult Delete(string id)
        {
            if (Role != "admin")
            {
                return new HttpStatusCodeResult(403);
            }
            mathang product = con.mathangs.FirstOrDefault(sp => sp.mamh == id);
            try
            {
                if (product != null)
                {
                    string fullPath = Request.MapPath("~/Hinh/" + product.hinhanhmh);
                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                    }
                    con.mathangs.DeleteOnSubmit(product);
                    con.SubmitChanges();
                }
            }
            catch
            {
                TempData["Xoa"] = $"Xóa thất bại!!! Mặt hàng {id} đã được lập hóa đơn";
            }
            return RedirectToAction("Index");
        }
        public ActionResult Details(string id)
        {
            if (Role != "admin" && Role != "nv")
            {
                return new HttpStatusCodeResult(403);
            }
            mathang Product = con.mathangs.FirstOrDefault(sp => sp.mamh == id);
            loaimathang lmh = con.loaimathangs.FirstOrDefault(x => x.maloai == Product.maloai);
            ViewBag.tenloai = lmh.tenloai;
            List<sp_mathangdetailsResult> dsdh = con.sp_mathangdetails(id).ToList();
            return View(Product);
        }
        public ActionResult MathangDetails(string id)
        {
            if (Role != "admin" && Role != "nv")
            {
                return new HttpStatusCodeResult(403);
            }
            List<sp_mathangdetailsResult> dsdh = con.sp_mathangdetails(id).ToList();
            return PartialView(dsdh);
        }
    }
}