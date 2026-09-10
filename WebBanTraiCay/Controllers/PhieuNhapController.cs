using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Web;
using System.Web.Mvc;
using WebBanTraiCay.Models;

namespace WebBanTraiCay.Controllers
{
    [Authorize]
    public class PhieuNhapController : BaseController
    {
        // GET: PhieuNhap
        WebsiteQuanLyBanTraicayDataContext con = new WebsiteQuanLyBanTraicayDataContext(MatHangController.conn);
        public ActionResult Index()
        {
            if (Role != "admin" && Role != "nv")
            {
                return new HttpStatusCodeResult(403);
            }
            List<Sp_XemPNResult> dsphieunhap = con.Sp_XemPN().ToList();
            return View(dsphieunhap);
        }
        public ActionResult SearchPN(FormCollection collection)
        {
            if (Role != "admin" && Role != "nv")
            {
                return new HttpStatusCodeResult(403);
            }
            List<Sp_XemPNResult> DsPN = con.Sp_XemPN().ToList();
            if (collection.Count == 0)
            {
                DsPN = con.Sp_XemPN().ToList();
            }
            if (!string.IsNullOrEmpty(collection["txtsearch"]))
            {
                DsPN = DsPN.Where(sp => sp.tenncc.ToLower().Contains(collection["txtsearch"].ToLower())).ToList();
            }
            if (!string.IsNullOrEmpty(collection["tennv"]))
            {
                DsPN = DsPN.Where(sp => sp.tennv.ToLower().Contains(collection["tennv"].ToLower())).ToList();
            }
            DateTime from, to;
            if (DateTime.TryParse(collection["dateFrom"], out from) && DateTime.TryParse(collection["dateTo"], out to))
                DsPN = DsPN.Where(p => p.ngaynhap >= from && p.ngaynhap <= to).ToList();
            else if (DateTime.TryParse(collection["dateFrom"], out from))
                DsPN = DsPN.Where(p => p.ngaynhap >= from).ToList();
            else if (DateTime.TryParse(collection["dateTo"], out to))
                DsPN = DsPN.Where(p => p.ngaynhap <= to).ToList();
            return PartialView(DsPN);
        }
        public ActionResult Details(string id)
        {
            if (Role != "admin" && Role != "nv")
            {
                return new HttpStatusCodeResult(403);
            }
            List<Sp_XemCtpnResult> ctpn = con.Sp_XemCtpn().Where(x => x.mapn == id).ToList();
            return View(ctpn);
        }
        public ActionResult Delete(string id)
        {
            if (Role != "admin")
            {
                return new HttpStatusCodeResult(403);
            }
            phieunhap dh = con.phieunhaps.FirstOrDefault(sp => sp.mapn == id);
            if (dh != null)
            {
                List<ctpn> ctpn = con.ctpns.Where(x => x.mapn == id).ToList();
                if (ctpn != null)
                {
                    foreach (ctpn a in ctpn)
                    {
                        mathang mh = con.mathangs.FirstOrDefault(x => x.mamh == a.mamh);
                        mh.soluong -= a.sln;
                        if (mh.soluong <= 0)
                            mh.tinhtrang = "Hết hàng";
                        con.ctpns.DeleteOnSubmit(a);
                    }
                }
                con.phieunhaps.DeleteOnSubmit(dh);
                con.SubmitChanges();
            }
            return RedirectToAction("Index");
        }
        public ActionResult Edit(string id)
        {
            if (Role != "admin" && Role != "nv")
            {
                return new HttpStatusCodeResult(403);
            }
            List<nhacungcap> ncc = con.nhacungcaps.ToList();
            ViewBag.ncc = new SelectList(ncc, "mancc", "tenncc", ncc.FirstOrDefault().mancc);
            List<mathang> mathang = con.mathangs.ToList();
            ViewBag.mathang = mathang;
            nhanvien nv = con.nhanviens.FirstOrDefault(x => x.manv == Manv.ToString());
            ViewBag.nv = nv.tennv;
            ViewBag.ctpn = con.Sp_XemCtpn().Where(x => x.mapn == id).ToList();
            phieunhap dh = con.phieunhaps.FirstOrDefault(sp => sp.mapn == id);
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
                phieunhap dh = con.phieunhaps.FirstOrDefault(sp => sp.mapn == id);
                dh.ngaynhap = DateTime.Parse(collection["ngaynhap"]).Date;
                dh.mancc = collection["mancc"];
                string[] dsmh = collection.GetValues("mamh");
                string[] dssl = collection.GetValues("soluong");
                string[] dongia = collection.GetValues("dongia");
                if (dsmh != null && dssl != null && dongia != null)
                {
                    List<ctpn> temp = con.ctpns.Where(x => x.mapn == dh.mapn).ToList();
                    foreach (ctpn a in temp)
                    {
                        if (!dsmh.Contains(a.mamh))
                        {
                            con.ctpns.DeleteOnSubmit(a);
                        }
                    }
                    for (int i = 0; i < dsmh.Length; i++)
                    {
                        string mamh = dsmh[i];
                        int soluong = int.Parse(dssl[i]);
                        int dgn = int.Parse(dongia[i]);
                        mathang mh = con.mathangs.FirstOrDefault(x => x.mamh == mamh);
                        if (mh != null)
                        {
                            ctpn ct = con.ctpns.FirstOrDefault(x => x.mapn == dh.mapn && x.mamh == mh.mamh);
                            if (ct != null)
                            {
                                int sltemp = ct.sln - soluong;
                                ct.sln = soluong;
                                ct.dgn = dgn;
                                mh.soluong -= sltemp;
                                if (mh.soluong <= 0)
                                    mh.tinhtrang = "Hết hàng";
                            }
                            else
                            {
                                ctpn newCT = new ctpn { mapn = dh.mapn, mamh = mh.mamh, sln = soluong, dgn = dgn };
                                mh.soluong += soluong;
                                if (mh.soluong > 0)
                                    mh.tinhtrang = "Còn hàng";
                                con.ctpns.InsertOnSubmit(newCT);
                            }
                        }
                    }
                }
                else
                {
                    TempData["ThongBao"] = $"Chỉnh sửa phiếu nhập {id} thất bại";
                    return RedirectToAction("Index");
                }
                con.SubmitChanges();
                TempData["ThongBao"] = $"Chỉnh sửa phiếu nhập {id} thành công";
            }
            catch
            {
                TempData["ThongBao"] = $"Chỉnh sửa phiếu nhập {id} thất bại";
            }
            return RedirectToAction("Index");
        }
        public ActionResult Create()
        {
            if (Role != "admin" && Role != "nv")
            {
                return new HttpStatusCodeResult(403);
            }
            List<nhacungcap> ncc = con.nhacungcaps.ToList();
            ViewBag.ncc = new SelectList(ncc, "mancc", "tenncc", ncc.FirstOrDefault().mancc);
            List<mathang> mathang = con.mathangs.ToList();
            ViewBag.mathang = mathang;
            nhanvien nv = con.nhanviens.FirstOrDefault(x => x.manv == Manv.ToString());
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
            nhanvien nv = con.nhanviens.FirstOrDefault(x => x.manv == Manv);
            if (collection.Count > 0)
            {
                try
                {
                    phieunhap pnmax = con.phieunhaps.OrderByDescending(x => x.mapn).FirstOrDefault();
                    phieunhap pn = new phieunhap();
                    string mapn = "";
                    if (pnmax == null)
                    {
                        mapn = "PN001";
                    }
                    else
                    {
                        int temp = int.Parse(pnmax.mapn.Substring(2, 3)) + 1;
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
                        mapn = String.Concat(pnmax.mapn.Substring(0, 2), s);
                    }
                    pn.mapn = mapn;
                    pn.ngaynhap = DateTime.Now.Date;
                    pn.mancc = collection["mancc"];
                    pn.manv = Manv;  
                    string[] dsmh = collection.GetValues("mamh");
                    string[] dssl = collection.GetValues("soluong");
                    string[] dongia = collection.GetValues("dongia");
                    for (int i = 0; i < dsmh.Length; i++)
                    {
                        string mamh = dsmh[i];
                        int soluong = int.Parse(dssl[i]);
                        int dgn = int.Parse(dongia[i]);
                        mathang mh = con.mathangs.FirstOrDefault(x => x.mamh == mamh);
                        if (mh != null)
                        {
                            ctpn ct = new ctpn();
                            ct.mapn = mapn;
                            ct.mamh = mamh;
                            ct.sln = soluong;
                            ct.dgn = dgn;
                            mh.soluong += soluong;
                            if (mh.soluong > 0)
                                mh.tinhtrang = "Còn hàng";
                            con.ctpns.InsertOnSubmit(ct);
                        }
                    }
                    con.phieunhaps.InsertOnSubmit(pn);
                    TempData["ThongBao"] = $"Đã thêm phiếu nhập {mapn} vào Database thành công";
                    con.SubmitChanges();
                }
                catch
                {
                    TempData["ThongBao"] = $"Thêm phiếu nhập thất bại";
                }
                return RedirectToAction("Index");
            }
            return View();
        }
    }
}