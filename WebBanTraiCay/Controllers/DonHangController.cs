using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WebBanTraiCay.Models;

namespace WebBanTraiCay.Controllers
{
    [Authorize]
    public class DonHangController : BaseController
    {
        // GET: DonHang
        WebsiteQuanLyBanTraicayDataContext con = new WebsiteQuanLyBanTraicayDataContext(MatHangController.conn);

        public ActionResult Index()
        {
            if (Role != "admin" && Role != "nv")
            {
                return new HttpStatusCodeResult(403);
            }
            List<Sp_XemDHResult> dsdonhang = con.Sp_XemDH().ToList();
            return View(dsdonhang);
        }
        public ActionResult SearchDH(FormCollection collection)
        {
            if (Role != "admin" && Role != "nv")
            {
                return new HttpStatusCodeResult(403);
            }
            List<Sp_XemDHResult> DsDH = con.Sp_XemDH().ToList();
            if (collection.Count == 0)
            {
                DsDH = con.Sp_XemDH().ToList();
            }
            if (!string.IsNullOrEmpty(collection["txtsearch"]))
            {
                DsDH = DsDH.Where(sp => sp.tenkh.ToLower().Contains(collection["txtsearch"].ToLower())).ToList();
            }
            if (!string.IsNullOrEmpty(collection["tinhtrang"]))
                DsDH = DsDH.Where(p => p.trangthai == collection["tinhtrang"]).ToList();
            DateTime from, to;
            if (DateTime.TryParse(collection["dateFrom"], out from) && DateTime.TryParse(collection["dateTo"], out to))
                DsDH = DsDH.Where(p => p.ngayban >= from && p.ngayban <= to).ToList();
            else if (DateTime.TryParse(collection["dateFrom"], out from))
                DsDH = DsDH.Where(p => p.ngayban >= from).ToList();
            else if (DateTime.TryParse(collection["dateTo"], out to))
                DsDH = DsDH.Where(p => p.ngayban <= to).ToList();
            return PartialView(DsDH);
        }
        public ActionResult Delete(string id)
        {
            if (Role != "admin")
            {
                return new HttpStatusCodeResult(403);
            }
            donhang dh = con.donhangs.FirstOrDefault(sp => sp.madh == id);
            if (dh != null)
            {
                if (dh.trangthai != "Chờ xử lý")
                {
                    TempData["ThongBao"] = "Chỉ được xóa đơn đang chờ xử lý";
                    return RedirectToAction("Index");
                }
                List<ctdh> ctdh = con.ctdhs.Where(x => x.madh == id).ToList();
                if (ctdh != null)
                {
                    con.ctdhs.DeleteAllOnSubmit(ctdh);
                }
                con.donhangs.DeleteOnSubmit(dh);
                con.SubmitChanges();
            }
            return RedirectToAction("Index");
        }
        [HttpPost]
        public ActionResult HuyDon(string madh)
        {
            donhang dh = con.donhangs.FirstOrDefault(x => x.madh == madh && x.makh == Makh);
            if (dh == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đơn hàng!" });
            }
            if (dh.trangthai != "Chờ xử lý")
            {
                return Json(new { success = false, message = "Đơn hàng đã được xử lý, không thể hủy!" });
            }
            dh.trangthai = "Đã hủy";
            con.SubmitChanges();
            return Json(new { success = true, message = "Hủy đơn hàng thành công!" });
        }
        public ActionResult Details(string id)
        {
            if (Role != "admin" && Role != "nv")
            {
                return new HttpStatusCodeResult(403);
            }
            List<Sp_XemCtdhResult> ctdh = con.Sp_XemCtdh().Where(x => x.madh == id).ToList();
            return View(ctdh);
        }
        public ActionResult Create()
        {
            if (Role != "admin" && Role != "nv")
            {
                return new HttpStatusCodeResult(403);
            }
            List<khachhang> kh = con.khachhangs.ToList();
            ViewBag.khachhang = new SelectList(kh, "makh", "tenkh", kh.FirstOrDefault().makh);
            List<mathang> mathang = con.mathangs.ToList();
            ViewBag.mathang = mathang;
            nhanvien nv = con.nhanviens.FirstOrDefault(x => x.manv == Manv);
            ViewBag.nv = nv.tennv;
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
                    donhang dhmax = con.donhangs.OrderByDescending(x => x.madh).FirstOrDefault();
                    nhanvien nv = con.nhanviens.FirstOrDefault(x => x.manv == Manv);
                    donhang dh = new donhang();
                    string madh = "";
                    if (dhmax == null)
                    {
                        madh = "DH001";
                    }
                    else
                    {
                        int temp = int.Parse(dhmax.madh.Substring(2, 3)) + 1;
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
                        madh = String.Concat(dhmax.madh.Substring(0, 2), s);
                    }
                    dh.madh = madh;
                    dh.ngayban = DateTime.Now.Date;
                    dh.makh = collection["makh"];
                    dh.trangthai = "Chờ xử lý";
                    dh.manv = nv.manv;
                    string[] dsmh = collection.GetValues("mamh");
                    string[] dssl = collection.GetValues("soluong");
                    for (int i = 0; i < dsmh.Length; i++)
                    {
                        string mamh = dsmh[i];
                        int soluong = int.Parse(dssl[i]);
                        mathang mh = con.mathangs.FirstOrDefault(x => x.mamh == mamh);
                        if (mh != null)
                        {
                            ctdh ct = new ctdh();
                            ct.madh = madh;
                            ct.mamh = mamh;
                            ct.sl = soluong;
                            int temp = mh.soluong - soluong;
                            ct.gb = mh.giaban;
                            if (temp < 0)
                            {
                                TempData["ThongBao"] = $"Chỉnh sửa đơn hàng thất bại!! Số lượng vượt quá tồn kho";
                                return RedirectToAction("Index");
                            }
                            con.ctdhs.InsertOnSubmit(ct);
                        }
                    }
                    con.donhangs.InsertOnSubmit(dh);
                    con.SubmitChanges();
                    TempData["ThongBao"] = $"Đã thêm đơn hàng {madh} vào Database thành công";
                }
                catch
                {
                    TempData["ThongBao"] = $"Thêm đơn hàng thất bại";
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
            donhang dh = con.donhangs.FirstOrDefault(sp => sp.madh == id);
            ViewBag.kh = con.khachhangs.FirstOrDefault(x => x.makh == dh.makh).tenkh;
            List<mathang> mathang = con.mathangs.ToList();
            ViewBag.mathang = mathang;
            nhanvien nv = con.nhanviens.FirstOrDefault(x => x.manv == dh.manv);
            ViewBag.nv = nv == null ? "Đơn khách hàng đặt online" : nv.tennv;
            if (dh.trangthai == "Chờ xử lý")
            {
                List<string> tinhtrang = new List<string> { "Chờ xử lý", "Đã thanh toán", "Đã hủy", "Đang giao hàng" };
                ViewBag.tinhtrang = new SelectList(tinhtrang);
            }
            else if (dh.trangthai == "Đã thanh toán")
            {
                if (Role == "admin")
                {
                    List<string> tinhtrang = new List<string> { "Đã thanh toán", "Đã hủy" };
                    ViewBag.tinhtrang = new SelectList(tinhtrang);
                }
                else
                {
                    TempData["ThongBao"] = $"Đơn hàng đã thanh toán không được phép chỉnh sửa";
                    return RedirectToAction("Index");
                }
            }
            else if (dh.trangthai == "Đã hủy")
            {
                if (Role == "admin")
                {
                    List<string> tinhtrang = new List<string> { "Đã hủy" };
                    ViewBag.tinhtrang = new SelectList(tinhtrang);
                }
                else
                {
                    TempData["ThongBao"] = $"Đơn hàng đã hủy không được phép chỉnh sửa";
                    return RedirectToAction("Index");
                }
            }
            else if (dh.trangthai == "Đang giao hàng")
            {
                List<string> tinhtrang = new List<string> { "Đã thanh toán", "Đã hủy", "Đang giao hàng" };
                ViewBag.tinhtrang = new SelectList(tinhtrang);
                ViewBag.check = "readonly";
            }
            List<ctdh> ctdh = con.ctdhs.Where(x => x.madh == id).ToList();
            ViewBag.ctdh = con.Sp_XemCtdh().Where(x => x.madh == id).ToList();
            return View(dh);
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
                nhanvien nv = con.nhanviens.FirstOrDefault(x => x.manv == Manv.ToString());
                donhang dh = con.donhangs.FirstOrDefault(sp => sp.madh == id);
                if ((dh.trangthai == "Đã thanh toán" || dh.trangthai == "Đã hủy") && Role!="admin")
                {
                    if (Role != "admin")
                    {
                        TempData["ThongBao"] = $"Đơn hàng đã thanh toán hoặc đã hủy không được phép chỉnh sửa";
                        return RedirectToAction("Index");
                    }
                }
                if (collection["trangthai"] == "Đã hủy" && dh.trangthai=="Đang giao hàng")
                {
                    List<ctdh> ctdh = con.ctdhs.Where(x => x.madh == id).ToList();
                    if (ctdh != null)
                    {
                        foreach(ctdh a in ctdh)
                        {
                            string mamh = a.mamh;
                            int soluong = a.sl;
                            mathang mh = con.mathangs.FirstOrDefault(x => x.mamh == mamh);
                            if (mh != null)
                            {
                                mh.soluong += soluong;
                                if(mh.soluong > 0)
                                    mh.tinhtrang = "Còn hàng";
                            }
                        }
                    }
                    dh.trangthai = collection["trangthai"];
                    con.SubmitChanges();
                    return RedirectToAction("Index");
                }
                string[] dsmh = collection.GetValues("mamh");
                string[] dssl = collection.GetValues("soluong");
                if (dh.trangthai == "Chờ xử lý")
                {
                    if (dsmh != null && dssl != null)
                    {
                        List<ctdh> tempdh = con.ctdhs.Where(x => x.madh == dh.madh).ToList();
                        foreach (ctdh a in tempdh)
                        {
                            if (!dsmh.Contains(a.mamh))
                            {
                                con.ctdhs.DeleteOnSubmit(a);
                            }
                        }
                        for (int i = 0; i < dsmh.Length; i++)
                        {
                            string mamh = dsmh[i];
                            int soluong = int.Parse(dssl[i]);
                            mathang mh = con.mathangs.FirstOrDefault(x => x.mamh == mamh);
                            if (mh != null)
                            {
                                ctdh ct = con.ctdhs.FirstOrDefault(x => x.madh == dh.madh && x.mamh == mh.mamh);
                                if (ct != null)
                                {
                                    int sltemp = ct.sl - soluong;
                                    ct.sl = soluong;
                                    int temp = mh.soluong + sltemp;
                                    ct.gb = mh.giaban;
                                    if (temp < 0)
                                    {
                                        TempData["ThongBao"] = $"Chỉnh sửa đơn hàng thất bại!! Số lượng vượt quá tồn kho";
                                        return RedirectToAction("Index");
                                    }
                                }
                                else
                                {
                                    ctdh newCT = new ctdh { madh = dh.madh, mamh = mh.mamh, sl = soluong, gb = mh.giaban };
                                    int temp = mh.soluong - soluong;
                                    if (temp < 0)
                                    {
                                        TempData["ThongBao"] = $"Chỉnh sửa đơn hàng thất bại!!! Số lượng vượt quá tồn kho";
                                        return RedirectToAction("Index");
                                    }
                                    con.ctdhs.InsertOnSubmit(newCT);
                                }
                            }
                        }
                    }
                    else
                    {
                        TempData["ThongBao"] = $"Chỉnh sửa đơn hàng {id} thất bại";
                        return RedirectToAction("Index");
                    }
                    con.SubmitChanges();
                }
                if ((collection["trangthai"] == "Đang giao hàng" || collection["trangthai"]=="Đã thanh toán") && dh.trangthai=="Chờ xử lý")
                {
                    List<ctdh> ctdh = con.ctdhs.Where(x => x.madh == id).ToList();
                    foreach (var item in ctdh)
                    {
                        mathang mh = con.mathangs.FirstOrDefault(x => x.mamh == item.mamh);
                        if (mh != null)
                        {                           
                            mh.soluong -= item.sl;
                            if(mh.soluong < 0)
                            {
                                TempData["ThongBao"] = $"Chỉnh sửa đơn hàng thất bại!!! Số lượng vượt quá tồn kho";
                                return RedirectToAction("Index");
                            }
                            else if (mh.soluong == 0)
                            {
                                mh.tinhtrang = "Hết hàng";
                            }
                        }
                    }
                }
                dh.trangthai = collection["trangthai"];
                con.SubmitChanges();
                TempData["ThongBao"] = $"Chỉnh sửa đơn hàng {id} thành công";
            }
            catch
            {
                TempData["ThongBao"] = $"Chỉnh sửa đơn hàng {id} thất bại";
            }
            return RedirectToAction("Index");
        }
    }
}