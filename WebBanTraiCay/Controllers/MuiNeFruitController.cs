using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanTraiCay.Models;
using System.Net;
using System.Net.Mail;
using System.Transactions;
namespace WebBanTraiCay.Controllers
{
    public class MuiNeFruitController : BaseController
    {
        // GET: MuiNeFruit
        WebsiteQuanLyBanTraicayDataContext con = new WebsiteQuanLyBanTraicayDataContext(MatHangController.conn);
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Contact()
        {
            return View();
        }
        public ActionResult History()
        {
            List<donhang> dh = con.donhangs.Where(x => x.makh == Makh).OrderByDescending(x => x.ngayban).ToList();
            ViewBag.ctdh = con.ctdhs.ToList();
            return View(dh);
        }
        [HttpPost]
        public ActionResult XoaDonHangRac(string madh)
        {
            if (Makh == null)
                return RedirectToAction("Login", "Account");
            try
            {
                donhang dh = con.donhangs.FirstOrDefault(x => x.madh == madh && x.makh == Makh && x.trangthai == "Chờ xử lý" && x.pttt == "VietQR");

                if (dh != null)
                {
                    List<ctdh> ctdhs = con.ctdhs.Where(x => x.madh == madh).ToList();
                    con.ctdhs.DeleteAllOnSubmit(ctdhs);
                    con.donhangs.DeleteOnSubmit(dh);
                    con.SubmitChanges();
                    return Json(new { success = true});
                }
            }
            catch
            {
                return Json(new { success = false});
            }

            return Json(new { success = false });
        }
        public ActionResult Details(string id)
        {
            if (id == null) 
                return RedirectToAction("History");
            List<Sp_XemCtdhResult> ctdh = con.Sp_XemCtdh().Where(x => x.madh == id).ToList();
            return View(ctdh);
        }
        public ActionResult ThanhToan(string madh)
        {
            if (Makh == null) 
                return RedirectToAction("Login","Account");
            var dh = con.donhangs.FirstOrDefault(x => x.madh == madh && x.makh == Makh);
            if (dh == null || dh.trangthai != "Chờ xử lý"|| dh.pttt!="VietQR")
            {
                return RedirectToAction("Cart");
            }
            ViewBag.TongTien = dh.ctdhs.Sum(x => x.sl * x.gb);
            return View(dh);
        }
        [HttpPost]
        public ActionResult HoanTatOnline(string madh)
        {
            if (Makh == null)
                return RedirectToAction("Login", "Account");
            try
            {
                donhang dh = con.donhangs.FirstOrDefault(x => x.madh == madh && x.makh == Makh && x.trangthai == "Chờ xử lý" && x.pttt == "VietQR");
                if (dh != null)
                {
                    using (var dbContextTransaction = new System.Transactions.TransactionScope())
                    {
                        List<giohang> giohang = con.giohangs.Where(x => x.makh == Makh).ToList();
                        if (giohang.Any())
                        {
                            con.giohangs.DeleteAllOnSubmit(giohang);
                        }
                        dh.trangthai = "Chờ xử lý";
                        dh.ngayban = DateTime.Now;
                        con.SubmitChanges();
                        dbContextTransaction.Complete();
                        TempData["ThongBao"] = "Xác nhận chuyển khoản thành công! Chúng tôi sẽ kiểm tra và giao hàng sớm nhất.";
                        return RedirectToAction("Cart");
                    }
                }
                else
                {
                    TempData["ThongBao"] = "Đơn hàng không hợp lệ hoặc đã được xác nhận.";
                    return RedirectToAction("Cart");
                }
            }
            catch (Exception)
            {
                TempData["ThongBao"] = "Có lỗi xảy ra trong quá trình xử lý đơn hàng.";
                return RedirectToAction("Cart");
            }
        }
        [HttpPost]
        public ActionResult DatHang(string makh, string pttt)
        {
            List<giohang> gh = con.giohangs.Where(x => x.makh == makh).ToList();
            if (gh != null && gh.Count > 0)
            {
                try
                {
                    using (TransactionScope transaction = new TransactionScope())
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
                        dh.manv = null;
                        dh.makh = makh;
                        dh.ngayban = DateTime.Now.Date;
                        dh.trangthai = "Chờ xử lý";
                        dh.pttt = pttt;
                        con.donhangs.InsertOnSubmit(dh);
                        List<ctdh> ctdh = new List<ctdh>();
                        foreach (giohang a in gh)
                        {
                            mathang mh = con.mathangs.FirstOrDefault(x => x.mamh == a.mamh);
                            if (mh != null)
                            {
                                ctdh ct = new ctdh();
                                ct.madh = madh;
                                ct.mamh = a.mamh;
                                ct.sl = a.soluong;
                                ct.gb = mh.giaban;
                                ctdh.Add(ct);
                            }
                        }
                        con.ctdhs.InsertAllOnSubmit(ctdh);
                        if (pttt == "COD")
                        {
                            con.giohangs.DeleteAllOnSubmit(gh);
                            con.SubmitChanges();
                            transaction.Complete();
                            TempData["ThongBao"] = "Đặt hàng thành công!";
                            return RedirectToAction("Cart");
                        }
                        con.SubmitChanges();
                        transaction.Complete();
                        return RedirectToAction("ThanhToan", new { madh = madh });
                    }
                }
                catch
                {
                    TempData["ThongBao"] = "Đặt hàng thất bại!! Có lỗi xảy ra";
                }
            }
            return RedirectToAction("Cart");
        }
        [HttpPost]
        public ActionResult Contact(string email, string subject, string noidung)
        {
            var fromAddress = new MailAddress("MuiNeFruit@gmail.com", "MuiNe Fruit Contact");
            var toAddress = new MailAddress("bich123asd@gmail.com");
            string fromPassword = "yykn vxfk iebk xnzv";
            string body = $"Người gửi: {email}\nTiêu đề: {subject}\nNội dung: {noidung}";
            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };
            MailMessage message = new MailMessage(fromAddress, toAddress)
            {
                Subject = "[Liên hệ từ Website MuiNeFruit] " + subject,
                Body = body
            };
            smtp.Send(message);
            message.Dispose();
            smtp.Dispose();
            TempData["ThongBao"] = "Gửi tin nhắn thành công! Chúng tôi sẽ phản hồi sớm.";
            return RedirectToAction("Contact");
        }
        public ActionResult Cart()
        {
            khachhang kh = con.khachhangs.FirstOrDefault(x => x.makh == Makh);
            if (kh == null)
                kh = new khachhang();
            return View(kh);
        }
        public ActionResult MatHangDetail(string id)
        {
            mathang mh = con.mathangs.FirstOrDefault(x => x.mamh == id);
            ViewBag.loai = con.loaimathangs.FirstOrDefault(x => x.maloai == mh.maloai).tenloai;
            return View(mh);
        }
        public ActionResult MatHangRelated(string id)
        {
            List<mathang> mh = con.mathangs.Where(x => x.maloai == id).ToList();
            ViewBag.loai = con.loaimathangs.ToList();
            return PartialView(mh);
        }
        public ActionResult Shop()
        {
            List<mathang> mh = con.mathangs.ToList();
            ViewBag.loai = con.loaimathangs.ToList();
            return View(mh);
        }
        [HttpPost]
        public ActionResult Shop(string search)
        {
            List<mathang> mh = con.mathangs.ToList();
            ViewBag.loai = con.loaimathangs.ToList();
            ViewBag.Search = search;
            return View(mh);
        }
        public ActionResult FilterShop(FormCollection collection)
        {
            int pageSize = 6;
            int pageNumber = 1;
            if (!string.IsNullOrEmpty(collection["page"]))
            {
                pageNumber = int.Parse(collection["page"]);
            }
            List<mathang> mh = con.mathangs.ToList();
            if (!string.IsNullOrEmpty(collection["maloai"]))
            {
                mh = mh.Where(x => x.maloai == collection["maloai"]).ToList();
            }
            if (!string.IsNullOrEmpty(collection["maxPrice"]))
            {
                int max = int.Parse(collection["maxPrice"]);
                mh = mh.Where(x => x.giaban <= max).ToList();
            }
            if (!string.IsNullOrEmpty(collection["search"]))
            {
                mh = mh.Where(sp => sp.tenmh.ToLower().Contains(collection["search"].ToLower())).ToList();
            }
            switch (collection["sort"])
            {
                case "PriceAsc": mh = mh.OrderBy(x => x.giaban).ToList(); break;
                case "PriceDesc": mh = mh.OrderByDescending(x => x.giaban).ToList(); break;
                case "New": mh = mh.OrderByDescending(x => x.mamh).ToList(); break;
                default: mh = mh.OrderBy(x => x.mamh).ToList(); break;
            }
            int totalItems = mh.Count();
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewBag.CurrentPage = pageNumber;
            ViewBag.loai = con.loaimathangs.ToList();
            List<mathang> model = mh.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            return PartialView(model);
        }
        public ActionResult Service()
        {
            return PartialView();
        }
        [HttpPost]
        public ActionResult AddCart(string id, int sl)
        {
            mathang mh = con.mathangs.FirstOrDefault(x => x.mamh == id);
            if (mh == null)
            {
                return Json(new { success = false, message = "Sản phẩm không tồn tại" });
            }
            if (sl == 0 || sl < 0)
            {
                return Json(new { success = false, message = "Số lượng đặt phải lớn hơn 0" });
            }
            if (mh.soluong > 0)
            {
                giohang gh = con.giohangs.FirstOrDefault(x => x.mamh == id && x.makh == Makh);
                if (gh == null)
                {
                    gh = new giohang();
                    gh.makh = Makh;
                    gh.mamh = mh.mamh;
                    gh.soluong = sl;
                    gh.id = con.giohangs.OrderByDescending(x => x.mamh).FirstOrDefault() != null ? con.giohangs.OrderByDescending(x => x.mamh).FirstOrDefault().id + 1 : 1;
                    con.giohangs.InsertOnSubmit(gh);
                }
                else
                {
                    gh.soluong += sl;
                }
                con.SubmitChanges();
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, message = "Sản phẩm đã hết hàng" });
            }
        }
        public ActionResult Loadmore(int count, string loaiload)
        {
            List<mathang> mh = new List<mathang>();
            if (loaiload == "All")
            {
                mh = con.mathangs.Skip(count).Take(8).ToList();
                ViewBag.loai = con.loaimathangs.ToList();
                ViewBag.loaiload = "";
            }
            else if (loaiload == "New")
            {
                mh = con.mathangs.OrderByDescending(x => x.mamh).Skip(count).Take(8).ToList();
                ViewBag.loai = con.loaimathangs.ToList();
                ViewBag.loaiload = "new";
            }
            else
            {
                List<sp_GetBestSellingProductsResult> mhsale = con.sp_GetBestSellingProducts().Skip(count).Take(8).ToList();
                foreach (var a in mhsale)
                {
                    mathang sell = con.mathangs.FirstOrDefault(x => x.mamh == a.mamh);
                    mh.Add(sell);
                    ViewBag.loai = con.loaimathangs.ToList();
                    ViewBag.loaiload = "best";
                }
            }
            return PartialView(mh);
        }
        public ActionResult LoadCart()
        {
            ViewBag.mathang = con.mathangs.ToList();
            List<giohang> gh = con.giohangs.Where(x => x.makh == Makh).ToList();
            return PartialView(gh);
        }
        public ActionResult LoadCartPage()
        {
            ViewBag.mathang = con.mathangs.ToList();
            List<giohang> gh = con.giohangs.Where(x => x.makh == Makh).ToList();
            return PartialView(gh);
        }
        [HttpPost]
        public ActionResult UpdateQuantity(string id, int change)
        {
            giohang gh = con.giohangs.FirstOrDefault(x => x.mamh == id && x.makh == Makh);
            mathang mh = con.mathangs.FirstOrDefault(x => x.mamh == gh.mamh);
            if (gh != null)
            {
                gh.soluong += change;
                if (gh.soluong <= 0)
                {
                    con.giohangs.DeleteOnSubmit(gh);
                }
                if (gh.soluong > mh.soluong)
                    return Json(new { success = false, message = "Rất tiếc, sản phẩm này chỉ còn " + mh.soluong + " mặt hàng!" });
                con.SubmitChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Không tìm thấy sản phẩm" });
        }
        [HttpPost]
        public ActionResult Remove(string id)
        {
            giohang gh = con.giohangs.FirstOrDefault(x => x.mamh == id && x.makh == Makh);
            if (gh != null)
            {
                int qty = gh.soluong;
                con.giohangs.DeleteOnSubmit(gh);
                con.SubmitChanges();
                return Json(new { success = true, removedQty = qty });
            }
            return Json(new { success = false });
        }
        [HttpPost]
        public ActionResult SearchMH(string input)
        {
            List<mathang> mh = con.mathangs.Where(s => s.tenmh.Contains(input)).Take(5).ToList();
            return PartialView(mh);
        }
    }
}