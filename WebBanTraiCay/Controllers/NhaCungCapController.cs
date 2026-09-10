using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using WebBanTraiCay.Models;

namespace WebBanTraiCay.Controllers
{
    [Authorize]
    public class NhaCungCapController : BaseController
    {
        // GET: NhaCungCap
        WebsiteQuanLyBanTraicayDataContext con = new WebsiteQuanLyBanTraicayDataContext(MatHangController.conn);
        public ActionResult Index()
        {
            if (Role != "admin" && Role != "nv")
            {
                return new HttpStatusCodeResult(403);
            }
            List<nhacungcap> dsncc = con.nhacungcaps.ToList();
            return View(dsncc);
        }
        public ActionResult SearchNCC(FormCollection collection)
        {
            if (Role != "admin" && Role != "nv")
            {
                return new HttpStatusCodeResult(403);
            }
            List<nhacungcap> ncc = con.nhacungcaps.ToList();
            if (collection.Count == 0)
            {
                ncc = con.nhacungcaps.ToList();
            }
            if (!string.IsNullOrEmpty(collection["txtsearch"]))
            {
                ncc = ncc.Where(sp => sp.tenncc.ToLower().Contains(collection["txtsearch"].ToLower())).ToList();
            }
            if (!string.IsNullOrEmpty(collection["txtdiachi"]))
            {
                ncc = ncc.Where(sp => sp.diachi.ToLower().Contains(collection["txtdiachi"].ToLower())).ToList();
            }
            return PartialView(ncc);
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
                    nhacungcap nccmax = con.nhacungcaps.OrderByDescending(x => x.mancc).FirstOrDefault();
                    nhacungcap ncc = new nhacungcap();
                    string mancc = "";
                    if (nccmax == null)
                    {
                        mancc = "NCC01";
                    }
                    else
                    {
                        int temp = int.Parse(nccmax.mancc.Substring(3, 2)) + 1;
                        string s = "";
                        if (temp < 10)
                        {
                            s += "0" + temp;
                        }

                        else
                        {
                            s += temp;
                        }
                        mancc = String.Concat(nccmax.mancc.Substring(0, 3), s);
                    }
                    ncc.mancc = mancc;
                    ncc.tenncc = Request.Form["tenncc"];
                    ncc.diachi = Request.Form["diachi"];
                    ncc.dienthoai = Request.Form["dienthoai"];
                    if(ncc.tenncc == null || ncc.diachi == null || ncc.dienthoai == null)
                    {
                        TempData["ThongBao"] = $"Thêm nhà cung cấp vào Database thất bại!! Dữ liệu bạn nhập thiếu!";
                        return RedirectToAction("Index");
                    }
                    con.nhacungcaps.InsertOnSubmit(ncc);
                    con.SubmitChanges();
                    TempData["ThongBao"] = $"Thêm nhà cung cấp {mancc} vào Database thành công";
                }
                catch
                {
                    TempData["ThongBao"] = $"Thêm nhà cung cấp vào Database thất bại";
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
            nhacungcap ncc = con.nhacungcaps.FirstOrDefault(sp => sp.mancc == id);
            try
            {
                if (ncc != null)
                {
                    con.nhacungcaps.DeleteOnSubmit(ncc);
                    con.SubmitChanges();
                }
            }
            catch
            {
                TempData["Xoa"] = $"Xóa thất bại!!! Nhà cung cấp {id} đã thực hiện giao dịch";
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
                nhacungcap ncc = con.nhacungcaps.FirstOrDefault(sp => sp.mancc == id);
                if (Request.Form.Count == 0)
                {
                    return View(ncc);
                }
                ncc.tenncc = Request.Form["tenncc"];
                ncc.diachi = Request.Form["diachi"];
                ncc.dienthoai = Request.Form["dienthoai"];
                if (ncc.tenncc == null || ncc.diachi == null || ncc.dienthoai == null)
                {
                    TempData["ThongBao"] = $"Chỉnh sửa nhà cung cấp {id} thất bại!! Dữ liệu bạn nhập thiếu!";
                    return RedirectToAction("Index");
                }
                con.SubmitChanges();
                TempData["ThongBao"] = $"Chỉnh sửa nhà cung cấp {id} thành công";
            }
            catch
            {
                TempData["ThongBao"] = $"Chỉnh sửa nhà cung cấp {id} thất bại";
            }
            return RedirectToAction("Index");
        }
        public ActionResult Details(string id)
        {
            if (Role != "admin" && Role != "nv")
            {
                return new HttpStatusCodeResult(403);
            }
            nhacungcap ncc = con.nhacungcaps.FirstOrDefault(sp => sp.mancc == id);
            return View(ncc);
        }
    }
}