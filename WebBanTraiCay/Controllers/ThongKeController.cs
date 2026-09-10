using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanTraiCay.Models;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace WebBanTraiCay.Controllers
{
    [Authorize]
    public class ThongKeController : BaseController
    {
        WebsiteQuanLyBanTraicayDataContext con = new WebsiteQuanLyBanTraicayDataContext(MatHangController.conn);
        // GET: ThongKe
        public ActionResult Index()
        {
            if (Role != "admin" && Role != "nv")
            {
                return new HttpStatusCodeResult(403);
            }
            return View();
        }
        [HttpGet]
        public ActionResult DowloadNH(string id)
        {
            if (Role != "admin" && Role != "nv")
            {
                return new HttpStatusCodeResult(403);
            }
            ExcelPackage.License.SetNonCommercialPersonal("MuiNeFruit");
            using (ExcelPackage package = new ExcelPackage())
            {
                ExcelWorksheet ws = package.Workbook.Worksheets.Add("BaoCaoNhapHang");
                if (id == "nht")
                {
                    List<sp_baocaonhttResult> bcdt = con.sp_baocaonhtt().ToList();
                    ws.Cells[1, 1].Value = $"BÁO CÁO CHI TIÊU NHẬP HÀNG TRONG NĂM (THEO THÁNG)";
                    ws.Cells[1, 1, 1, 8].Merge = true;
                    ws.Cells[1, 1].Style.Font.Bold = true;
                    ws.Cells[1, 1].Style.Font.Size = 14;
                    ws.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells[2, 1].Value = "STT";
                    ws.Cells[2, 2].Value = "Mã Phiếu Nhập";
                    ws.Cells[2, 3].Value = "Ngày Nhập";
                    ws.Cells[2, 4].Value = "Nhà Cung Cấp";
                    ws.Cells[2, 5].Value = "Tên Mặt Hàng";
                    ws.Cells[2, 6].Value = "Số Lượng";
                    ws.Cells[2, 7].Value = "Đơn Giá";
                    ws.Cells[2, 8].Value = "Tổng Chi Phí";
                    using (var range = ws.Cells[2, 1, 2, 8])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    }
                    int row = 3;
                    int stt = 1;
                    int tong = 0;
                    foreach (var item in bcdt)
                    {
                        ws.Cells[row, 1].Value = stt++;
                        ws.Cells[row, 2].Value = item.mapn;
                        ws.Cells[row, 3].Value = item.ngaynhap.ToString("dd/MM/yyyy");
                        ws.Cells[row, 4].Value = item.tenncc;
                        ws.Cells[row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        ws.Cells[row, 5].Value = item.tenmh;
                        ws.Cells[row, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        ws.Cells[row, 6].Value = item.sln;
                        ws.Cells[row, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Cells[row, 7].Value = item.dgn;
                        ws.Cells[row, 7].Style.Numberformat.Format = "#,##0";
                        ws.Cells[row, 8].Value = item.TongChiTieu;
                        ws.Cells[row, 8].Style.Numberformat.Format = "#,##0";
                        using (var range = ws.Cells[row, 1, row, 3])
                        {
                            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        }
                        tong += item.TongChiTieu;
                        row++;
                    }
                    ws.Cells[row, 1].Value = "TỔNG CỘNG: " + tong.ToString("#,##0") + " VNĐ";
                    ws.Cells[row, 1, row, 8].Style.Font.Bold = true;
                    ws.Cells[row, 1, row, 8].Merge = true;
                    ws.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells.AutoFitColumns();
                    using (var range = ws.Cells[1, 1, row, 8])
                    {
                        range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    }
                }
                else if (id == "nhq")
                {
                    List<sp_baocaonhtqResult> bcdt = con.sp_baocaonhtq().ToList();
                    ws.Cells[1, 1].Value = $"BÁO CÁO CHI TIÊU NHẬP HÀNG TRONG NĂM (THEO QUÝ)";
                    ws.Cells[1, 1, 1, 9].Merge = true;
                    ws.Cells[1, 1].Style.Font.Bold = true;
                    ws.Cells[1, 1].Style.Font.Size = 14;
                    ws.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells[2, 1].Value = "STT";
                    ws.Cells[2, 2].Value = "Mã Phiếu Nhập";
                    ws.Cells[2, 3].Value = "Ngày Nhập";
                    ws.Cells[2, 4].Value = "Quý";
                    ws.Cells[2, 5].Value = "Nhà Cung Cấp";
                    ws.Cells[2, 6].Value = "Tên Mặt Hàng";
                    ws.Cells[2, 7].Value = "Số Lượng";
                    ws.Cells[2, 8].Value = "Đơn Giá";
                    ws.Cells[2, 9].Value = "Tổng Chi Phí";
                    using (var range = ws.Cells[2, 1, 2, 9])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    }
                    int row = 3;
                    int stt = 1;
                    int tong = 0;
                    foreach (var item in bcdt)
                    {
                        ws.Cells[row, 1].Value = stt++;
                        ws.Cells[row, 2].Value = item.mapn;
                        ws.Cells[row, 3].Value = item.ngaynhap.ToString("dd/MM/yyyy");
                        ws.Cells[row, 4].Value = item.Quy;
                        ws.Cells[row, 5].Value = item.tenncc;
                        ws.Cells[row, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        ws.Cells[row, 6].Value = item.tenmh;
                        ws.Cells[row, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        ws.Cells[row, 7].Value = item.sln;
                        ws.Cells[row, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Cells[row, 8].Value = item.dgn;
                        ws.Cells[row, 8].Style.Numberformat.Format = "#,##0";
                        ws.Cells[row, 9].Value = item.TongChiTieu;
                        ws.Cells[row, 9].Style.Numberformat.Format = "#,##0";
                        using (var range = ws.Cells[row, 1, row, 4])
                        {
                            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        }
                        tong += item.TongChiTieu;
                        row++;
                    }
                    ws.Cells[row, 1].Value = "TỔNG CỘNG: " + tong.ToString("#,##0") + " VNĐ";
                    ws.Cells[row, 1, row, 9].Style.Font.Bold = true;
                    ws.Cells[row, 1, row, 9].Merge = true;
                    ws.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells.AutoFitColumns();
                    using (var range = ws.Cells[1, 1, row, 9])
                    {
                        range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    }
                }
                else
                {
                    List<sp_baocaonhtnResult> bcdt = con.sp_baocaonhtn().ToList();
                    ws.Cells[1, 1].Value = $"BÁO CÁO CHI TIÊU NHẬP HÀNG TRONG 5 NĂM GẦN ĐÂY";
                    ws.Cells[1, 1, 1, 8].Merge = true;
                    ws.Cells[1, 1].Style.Font.Bold = true;
                    ws.Cells[1, 1].Style.Font.Size = 14;
                    ws.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells[2, 1].Value = "STT";
                    ws.Cells[2, 2].Value = "Mã Phiếu Nhập";
                    ws.Cells[2, 3].Value = "Ngày Nhập";
                    ws.Cells[2, 4].Value = "Nhà Cung Cấp";
                    ws.Cells[2, 5].Value = "Tên Mặt Hàng";
                    ws.Cells[2, 6].Value = "Số Lượng";
                    ws.Cells[2, 7].Value = "Đơn Giá";
                    ws.Cells[2, 8].Value = "Tổng Chi Phí";
                    using (var range = ws.Cells[2, 1, 2, 8])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    }
                    int row = 3;
                    int stt = 1;
                    int tong = 0;
                    foreach (var item in bcdt)
                    {
                        ws.Cells[row, 1].Value = stt++;
                        ws.Cells[row, 2].Value = item.mapn;
                        ws.Cells[row, 3].Value = item.ngaynhap.ToString("dd/MM/yyyy");
                        ws.Cells[row, 4].Value = item.tenncc;
                        ws.Cells[row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        ws.Cells[row, 5].Value = item.tenmh;
                        ws.Cells[row, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        ws.Cells[row, 6].Value = item.sln;
                        ws.Cells[row, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Cells[row, 7].Value = item.dgn;
                        ws.Cells[row, 7].Style.Numberformat.Format = "#,##0";
                        ws.Cells[row, 8].Value = item.TongChiTieu;
                        ws.Cells[row, 8].Style.Numberformat.Format = "#,##0";
                        using (var range = ws.Cells[row, 1, row, 3])
                        {
                            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        }
                        tong += item.TongChiTieu;
                        row++;
                    }
                    ws.Cells[row, 1].Value = "TỔNG CỘNG: " + tong.ToString("#,##0") + " VNĐ";
                    ws.Cells[row, 1, row, 8].Style.Font.Bold = true;
                    ws.Cells[row, 1, row, 8].Merge = true;
                    ws.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells.AutoFitColumns();
                    using (var range = ws.Cells[1, 1, row, 8])
                    {
                        range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    }
                }
                byte[] fileBytes = package.GetAsByteArray();
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"BaoCaoNhapHang_{id}_{DateTime.Now:yyyyMMdd}.xlsx");
            }
        }
        [HttpGet]
        public ActionResult DowloadDT(string id)
        {
            if (Role != "admin" && Role != "nv")
            {
                return new HttpStatusCodeResult(403);
            }
            ExcelPackage.License.SetNonCommercialPersonal("MuiNeFruit");
            using (ExcelPackage package = new ExcelPackage())
            {
                ExcelWorksheet ws = package.Workbook.Worksheets.Add("BaoCaoDoanhThu");
                if (id == "dtt")
                {
                    List<sp_baocaodtttResult> bcdt = con.sp_baocaodttt().ToList();
                    ws.Cells[1, 1].Value = $"BÁO CÁO DOANH THU TRONG NĂM (THEO THÁNG)";
                    ws.Cells[1, 1, 1, 9].Merge = true;
                    ws.Cells[1, 1].Style.Font.Bold = true;
                    ws.Cells[1, 1].Style.Font.Size = 14;
                    ws.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells[2, 1].Value = "STT";
                    ws.Cells[2, 2].Value = "Mã Đơn Hàng";
                    ws.Cells[2, 3].Value = "Ngày Bán";
                    ws.Cells[2, 4].Value = "Khách Hàng";
                    ws.Cells[2, 5].Value = "Nhân Viên";
                    ws.Cells[2, 6].Value = "Tên Mặt Hàng";
                    ws.Cells[2, 7].Value = "Số Lượng";
                    ws.Cells[2, 8].Value = "Đơn Giá";
                    ws.Cells[2, 9].Value = "Doanh Thu";
                    using (var range = ws.Cells[2, 1, 2, 9])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    }
                    int row = 3;
                    int stt = 1;
                    int tong = 0;
                    foreach (var item in bcdt)
                    {
                        ws.Cells[row, 1].Value = stt++;
                        ws.Cells[row, 2].Value = item.madh;
                        ws.Cells[row, 3].Value = item.ngayban.ToString("dd/MM/yyyy");
                        ws.Cells[row, 4].Value = item.tenkh;
                        ws.Cells[row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        ws.Cells[row, 5].Value = item.tennv;
                        ws.Cells[row, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        ws.Cells[row, 6].Value = item.tenmh;
                        ws.Cells[row, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        ws.Cells[row, 7].Value = item.sl;
                        ws.Cells[row, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Cells[row, 8].Value = item.gb;
                        ws.Cells[row, 8].Style.Numberformat.Format = "#,##0";
                        ws.Cells[row, 9].Value = item.doanhthu;
                        ws.Cells[row, 9].Style.Numberformat.Format = "#,##0";
                        using (var range = ws.Cells[row, 1, row, 3])
                        {
                            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        }
                        tong += item.doanhthu;
                        row++;
                    }
                    ws.Cells[row, 1].Value = "TỔNG CỘNG: " + tong.ToString("#,##0") + " VNĐ";
                    ws.Cells[row, 1, row, 9].Style.Font.Bold = true;
                    ws.Cells[row, 1, row, 9].Merge = true;
                    ws.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells.AutoFitColumns();
                    using (var range = ws.Cells[1, 1, row, 9])
                    {
                        range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    }
                }
                else if (id == "dtq")
                {
                    List<sp_baocaodttqResult> bcdt = con.sp_baocaodttq().ToList();
                    ws.Cells[1, 1].Value = $"BÁO CÁO DOANH THU TRONG NĂM (THEO QUÝ)";
                    ws.Cells[1, 1, 1, 10].Merge = true;
                    ws.Cells[1, 1].Style.Font.Bold = true;
                    ws.Cells[1, 1].Style.Font.Size = 14;
                    ws.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells[2, 1].Value = "STT";
                    ws.Cells[2, 2].Value = "Mã Đơn Hàng";
                    ws.Cells[2, 3].Value = "Ngày Bán";
                    ws.Cells[2, 4].Value = "Quý";
                    ws.Cells[2, 5].Value = "Khách Hàng";
                    ws.Cells[2, 6].Value = "Nhân Viên";
                    ws.Cells[2, 7].Value = "Tên Mặt Hàng";
                    ws.Cells[2, 8].Value = "Số Lượng";
                    ws.Cells[2, 9].Value = "Đơn Giá";
                    ws.Cells[2, 10].Value = "Doanh Thu";
                    using (var range = ws.Cells[2, 1, 2, 10])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    }
                    int row = 3;
                    int stt = 1;
                    int tong = 0;
                    foreach (var item in bcdt)
                    {
                        ws.Cells[row, 1].Value = stt++;
                        ws.Cells[row, 2].Value = item.madh;
                        ws.Cells[row, 3].Value = item.ngayban.ToString("dd/MM/yyyy");
                        ws.Cells[row, 4].Value = item.quy;
                        ws.Cells[row, 5].Value = item.tenkh;
                        ws.Cells[row, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        ws.Cells[row, 6].Value = item.tennv;
                        ws.Cells[row, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        ws.Cells[row, 7].Value = item.tenmh;
                        ws.Cells[row, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        ws.Cells[row, 8].Value = item.sl;
                        ws.Cells[row, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Cells[row, 9].Value = item.gb;
                        ws.Cells[row, 9].Style.Numberformat.Format = "#,##0";
                        ws.Cells[row, 10].Value = item.doanhthu;
                        ws.Cells[row, 10].Style.Numberformat.Format = "#,##0";
                        using (var range = ws.Cells[row, 1, row, 4])
                        {
                            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        }
                        tong += item.doanhthu;
                        row++;
                    }
                    ws.Cells[row, 1].Value = "TỔNG CỘNG: " + tong.ToString("#,##0") + " VNĐ";
                    ws.Cells[row, 1, row, 10].Style.Font.Bold = true;
                    ws.Cells[row, 1, row, 10].Merge = true;
                    ws.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells.AutoFitColumns();
                    using (var range = ws.Cells[1, 1, row, 10])
                    {
                        range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    }
                }
                else
                {
                    List<sp_baocaodttnResult> bcdt = con.sp_baocaodttn().ToList();
                    ws.Cells[1, 1].Value = $"BÁO CÁO DOANH THU TRONG 5 NĂM GẦN ĐÂY";
                    ws.Cells[1, 1, 1, 9].Merge = true;
                    ws.Cells[1, 1].Style.Font.Bold = true;
                    ws.Cells[1, 1].Style.Font.Size = 14;
                    ws.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells[2, 1].Value = "STT";
                    ws.Cells[2, 2].Value = "Mã Đơn Hàng";
                    ws.Cells[2, 3].Value = "Ngày Bán";
                    ws.Cells[2, 4].Value = "Khách Hàng";
                    ws.Cells[2, 5].Value = "Nhân Viên";
                    ws.Cells[2, 6].Value = "Tên Mặt Hàng";
                    ws.Cells[2, 7].Value = "Số Lượng";
                    ws.Cells[2, 8].Value = "Đơn Giá";
                    ws.Cells[2, 9].Value = "Doanh Thu";
                    using (var range = ws.Cells[2, 1, 2, 9])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    }
                    int row = 3;
                    int stt = 1;
                    int tong = 0;
                    foreach (var item in bcdt)
                    {
                        ws.Cells[row, 1].Value = stt++;
                        ws.Cells[row, 2].Value = item.madh;
                        ws.Cells[row, 3].Value = item.ngayban.ToString("dd/MM/yyyy");
                        ws.Cells[row, 4].Value = item.tenkh;
                        ws.Cells[row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        ws.Cells[row, 5].Value = item.tennv;
                        ws.Cells[row, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        ws.Cells[row, 6].Value = item.tenmh;
                        ws.Cells[row, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        ws.Cells[row, 7].Value = item.sl;
                        ws.Cells[row, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Cells[row, 8].Value = item.gb;
                        ws.Cells[row, 8].Style.Numberformat.Format = "#,##0";
                        ws.Cells[row, 9].Value = item.doanhthu;
                        ws.Cells[row, 9].Style.Numberformat.Format = "#,##0";
                        using (var range = ws.Cells[row, 1, row, 3])
                        {
                            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        }
                        tong += item.doanhthu;
                        row++;
                    }
                    ws.Cells[row, 1].Value = "TỔNG CỘNG: " + tong.ToString("#,##0") + " VNĐ";
                    ws.Cells[row, 1, row, 9].Style.Font.Bold = true;
                    ws.Cells[row, 1, row, 9].Merge = true;
                    ws.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells.AutoFitColumns();
                    using (var range = ws.Cells[1, 1, row, 9])
                    {
                        range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    }
                }
                byte[] fileBytes = package.GetAsByteArray();
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"BaoCaoDoanhThu_{id}_{DateTime.Now:yyyyMMdd}.xlsx");
            }

        }
        public ActionResult TKTK()
        {
            if (Role != "admin" && Role != "nv")
            {
                return new HttpStatusCodeResult(403);
            }
            List<mathang> mhtemp = con.mathangs.ToList();
            int slt = 0;
            foreach (mathang a in mhtemp)
                slt += a.soluong;
            ViewBag.slt = slt;
            int slnm = con.sp_slmhnhap().First().sln.Value;
            ViewBag.slnm = slnm;
            int hh = con.sp_slmhhh().First().hethang.Value;
            ViewBag.hh = hh;
            List<sp_top5slmhResult> top5 = con.sp_top5slmh().ToList();
            List<string> tenmh = new List<string>();
            List<int> soluongton = new List<int>();
            foreach (var item in top5)
            {
                tenmh.Add(item.tenmh.ToString());
                soluongton.Add(item.soluong);
            }
            ViewBag.tenmh = JsonConvert.SerializeObject(tenmh);
            ViewBag.soluongton = JsonConvert.SerializeObject(soluongton);
            List<sp_slttlResult> slttl = con.sp_slttl().ToList();
            List<string> tenloai = new List<string>();
            List<int> soluong = new List<int>();
            foreach (var item in slttl)
            {
                tenloai.Add(item.tenloai.ToString());
                soluong.Add(item.soluong.Value);
            }
            ViewBag.pieloai = JsonConvert.SerializeObject(tenloai);
            ViewBag.piesll = JsonConvert.SerializeObject(soluong);
            List<string> ltk = new List<string>();
            HashSet<string> loaimatHang = new HashSet<string>();
            Dictionary<string, List<int>> slton = new Dictionary<string, List<int>>();
            Dictionary<int, Dictionary<string, int>> rows = new Dictionary<int, Dictionary<string, int>>();
            List<sp_tonkholkResult> tonkho = con.sp_tonkholk().ToList();
            foreach (var item in tonkho)
            {
                int thang = item.thang;
                string ten = item.tenloai;
                int sl = item.tonkholuyke ?? 0;
                if (ten != null)
                {
                    if (!rows.ContainsKey(thang))
                        rows[thang] = new Dictionary<string, int>();
                    rows[thang][ten] = sl;
                    loaimatHang.Add(ten);
                }
            }
            List<string> thangList = new List<string>();
            foreach (var mh in loaimatHang)
            {
                slton[mh] = new List<int>();
            }
            for (int i = 1; i <= 12; i++)
            {
                thangList.Add("Tháng " + i);
                foreach (var mh in loaimatHang)
                {
                    if (rows.ContainsKey(i) && rows[i].ContainsKey(mh))
                        slton[mh].Add(rows[i][mh]);
                    else
                        slton[mh].Add(0);
                }
            }
            ltk = thangList;
            var series = new List<object>();
            foreach (var mh in slton)
            {
                series.Add(new { name = mh.Key, data = mh.Value });
            }
            ViewBag.ltk = JsonConvert.SerializeObject(ltk);
            ViewBag.series = JsonConvert.SerializeObject(series);
            return View();
        }
        public ActionResult TKNH()
        {
            if (Role != "admin" && Role != "nv")
            {
                return new HttpStatusCodeResult(403);
            }
            int tong = 0, tongsln = 0;
            foreach (phieunhap pn in con.phieunhaps.Where(x => x.ngaynhap.Month == DateTime.Now.Month && x.ngaynhap.Year == DateTime.Now.Year).ToList())
            {
                foreach (ctpn a in con.ctpns.Where(x => x.mapn == pn.mapn).ToList())
                    tong += a.dgn * a.sln;
            }
            ViewBag.ttn = tong;
            foreach (phieunhap pn in con.phieunhaps.Where(x => x.ngaynhap.Month == DateTime.Now.Month && x.ngaynhap.Year == DateTime.Now.Year).ToList())
            {
                foreach (ctpn a in con.ctpns.Where(x => x.mapn == pn.mapn).ToList())
                    tongsln += a.sln;
            }
            ViewBag.tsln = tongsln;
            ViewBag.slnh = con.phieunhaps.Where(x => x.ngaynhap.Month == DateTime.Now.Month && x.ngaynhap.Year == DateTime.Now.Year).ToList().Count;
            ViewBag.ncc = con.nhacungcaps.ToList().Count;
            List<sp_Top5MatHangNhapNhieuNhatResult> top5 = con.sp_Top5MatHangNhapNhieuNhat().ToList();
            List<string> tenmh = new List<string>();
            List<int> soluong = new List<int>();
            foreach (var item in top5)
            {
                tenmh.Add(item.tenmh.ToString());
                soluong.Add(item.TongSoLuongNhap.Value);
            }
            ViewBag.tenmh = JsonConvert.SerializeObject(tenmh);
            ViewBag.soluong = JsonConvert.SerializeObject(soluong);
            List<sp_SoLuongNhapHang_TheoNhaCungCapResult> lmh = con.sp_SoLuongNhapHang_TheoNhaCungCap().ToList();
            List<string> tenncc = new List<string>();
            List<int> soluongnh = new List<int>();
            foreach (var item in lmh)
            {
                tenncc.Add(item.tenncc.ToString());
                soluongnh.Add(item.TongSoLuongNhap.Value);
            }
            ViewBag.pieloai = JsonConvert.SerializeObject(tenncc);
            ViewBag.piesll = JsonConvert.SerializeObject(soluongnh);
            return View();
        }
        [HttpPost]
        public ActionResult TKNH(string id)
        {
            if (Role != "admin" && Role != "nv")
            {
                return new HttpStatusCodeResult(403);
            }
            List<string> ltk = new List<string>();
            HashSet<string> nhacc = new HashSet<string>();
            Dictionary<string, List<int>> chitieunhap = new Dictionary<string, List<int>>();
            Dictionary<int, Dictionary<string, int>> rows = new Dictionary<int, Dictionary<string, int>>();
            if (id == "nht")
            {
                List<sp_ThongKeChiTieuNhapHangTheoThangResult> data = con.sp_ThongKeChiTieuNhapHangTheoThang().ToList();
                foreach (var item in data)
                {
                    int thang = item.Thang.Value;
                    string ten = item.tenncc;
                    int sl = item.TongChiTieu;
                    if (ten != null)
                    {
                        if (!rows.ContainsKey(thang))
                            rows[thang] = new Dictionary<string, int>();
                        rows[thang][ten] = sl;
                        nhacc.Add(ten);
                    }
                }
                List<string> thangList = new List<string>();
                foreach (var mh in nhacc)
                {
                    chitieunhap[mh] = new List<int>();
                }
                for (int i = 1; i <= 12; i++)
                {
                    thangList.Add("Tháng " + i);
                    foreach (var mh in nhacc)
                    {
                        if (rows.ContainsKey(i) && rows[i].ContainsKey(mh))
                            chitieunhap[mh].Add(rows[i][mh]);
                        else
                            chitieunhap[mh].Add(0);
                    }
                }
                ltk = thangList;
            }
            else if (id == "nhq")
            {
                List<sp_ThongKeChiTieuNhapHangTheoQuyResult> data = con.sp_ThongKeChiTieuNhapHangTheoQuy().ToList();
                foreach (var item in data)
                {
                    int quy = item.Quy.Value;
                    string ten = item.tenncc;
                    int sl = item.TongChiTieu;
                    if (ten != null)
                    {
                        if (!rows.ContainsKey(quy))
                            rows[quy] = new Dictionary<string, int>();
                        rows[quy][ten] = sl;
                        nhacc.Add(ten);
                    }
                }
                List<string> quyList = new List<string>();
                foreach (var mh in nhacc)
                {
                    chitieunhap[mh] = new List<int>();
                }
                for (int i = 1; i <= 4; i++)
                {
                    quyList.Add("Quý " + i);
                    foreach (var mh in nhacc)
                    {
                        if (rows.ContainsKey(i) && rows[i].ContainsKey(mh))
                            chitieunhap[mh].Add(rows[i][mh]);
                        else
                            chitieunhap[mh].Add(0);
                    }
                }
                ltk = quyList;
            }
            else
            {
                List<sp_ThongKeChiTieuNhapHangTheo5NamGanNhatResult> data = con.sp_ThongKeChiTieuNhapHangTheo5NamGanNhat().ToList();
                foreach (var item in data)
                {
                    int nam = item.Nam.Value;
                    string ten = item.tenncc;
                    int sl = item.TongChiTieu;
                    if (ten != null)
                    {
                        if (!rows.ContainsKey(nam))
                            rows[nam] = new Dictionary<string, int>();
                        rows[nam][ten] = sl;
                        nhacc.Add(ten);
                    }
                }
                List<string> namList = new List<string>();
                foreach (var mh in nhacc)
                {
                    chitieunhap[mh] = new List<int>();
                }
                int datenow = DateTime.Now.Year - 4;
                for (int i = datenow; i <= datenow + 4; i++)
                {
                    namList.Add("Năm " + i);
                    foreach (var mh in nhacc)
                    {
                        if (rows.ContainsKey(i) && rows[i].ContainsKey(mh))
                            chitieunhap[mh].Add(rows[i][mh]);
                        else
                            chitieunhap[mh].Add(0);
                    }
                }
                ltk = namList;
            }
            var series = new List<object>();
            foreach (var mh in chitieunhap)
            {
                series.Add(new { name = mh.Key, data = mh.Value });
            }
            return Json(new { ltk = ltk, series = series });
        }
        public ActionResult TKDH()
        {
            if (Role != "admin" && Role != "nv")
            {
                return new HttpStatusCodeResult(403);
            }
            int slt = con.sldhd().First().slt.Value;
            ViewBag.slt = slt;
            int sltt = con.sldhtt().First().sln.Value;
            ViewBag.sltt = sltt;
            int sldh = con.sldhdh().First().huy.Value;
            ViewBag.sldh = sldh;
            List<sp_Top5MatHang_ThangNayResult> top5 = con.sp_Top5MatHang_ThangNay().ToList();
            List<string> tenmh = new List<string>();
            List<int> soluongdat = new List<int>();
            foreach (var item in top5)
            {
                tenmh.Add(item.tenmh.ToString());
                soluongdat.Add(item.soluongdat.Value);
            }
            ViewBag.tenmh = JsonConvert.SerializeObject(tenmh);
            ViewBag.soluongdat = JsonConvert.SerializeObject(soluongdat);
            List<sp_LoaiMatHang_ThangNayResult> lmh = con.sp_LoaiMatHang_ThangNay().ToList();
            List<string> tenloai = new List<string>();
            List<int> soluong = new List<int>();
            foreach (var item in lmh)
            {
                tenloai.Add(item.tenloai.ToString());
                soluong.Add(item.soluong.Value);
            }
            ViewBag.pieloai = JsonConvert.SerializeObject(tenloai);
            ViewBag.piesll = JsonConvert.SerializeObject(soluong);
            return View();
        }
        [HttpPost]
        public ActionResult TKDH(string id)
        {
            if (Role != "admin" && Role != "nv")
            {
                return new HttpStatusCodeResult(403);
            }
            List<string> ltk = new List<string>();
            HashSet<string> matHang = new HashSet<string>();
            Dictionary<string, List<int>> sldonhang = new Dictionary<string, List<int>>();
            Dictionary<int, Dictionary<string, int>> rows = new Dictionary<int, Dictionary<string, int>>();
            if (id == "dht")
            {
                List<sp_ThongKeDHTheoThangResult> data = con.sp_ThongKeDHTheoThang().ToList();
                foreach (var item in data)
                {
                    int thang = item.thang;
                    string ten = item.tenloai;
                    int sl = item.soluongdh ?? 0;
                    if (ten != null)
                    {
                        if (!rows.ContainsKey(thang))
                            rows[thang] = new Dictionary<string, int>();
                        rows[thang][ten] = sl;
                        matHang.Add(ten);
                    }
                }
                List<string> thangList = new List<string>();
                foreach (var mh in matHang)
                {
                    sldonhang[mh] = new List<int>();
                }
                for (int i = 1; i <= 12; i++)
                {
                    thangList.Add("Tháng " + i);
                    foreach (var mh in matHang)
                    {
                        if (rows.ContainsKey(i) && rows[i].ContainsKey(mh))
                            sldonhang[mh].Add(rows[i][mh]);
                        else
                            sldonhang[mh].Add(0);
                    }
                }
                ltk = thangList;
            }
            else if (id == "dhq")
            {
                List<sp_ThongKeDHTheoQuyResult> data = con.sp_ThongKeDHTheoQuy().ToList();
                foreach (var item in data)
                {
                    int quy = item.quy;
                    string ten = item.tenloai;
                    int sl = item.soluongdh ?? 0;
                    if (ten != null)
                    {
                        if (!rows.ContainsKey(quy))
                            rows[quy] = new Dictionary<string, int>();
                        rows[quy][ten] = sl;
                        matHang.Add(ten);
                    }
                }
                List<string> quyList = new List<string>();
                foreach (var mh in matHang)
                {
                    sldonhang[mh] = new List<int>();
                }
                for (int i = 1; i <= 4; i++)
                {
                    quyList.Add("Quý " + i);
                    foreach (var mh in matHang)
                    {
                        if (rows.ContainsKey(i) && rows[i].ContainsKey(mh))
                            sldonhang[mh].Add(rows[i][mh]);
                        else
                            sldonhang[mh].Add(0);
                    }
                }
                ltk = quyList;
            }
            else
            {
                List<sp_ThongKeDHTheo5NamGanNhatResult> data = con.sp_ThongKeDHTheo5NamGanNhat().ToList();
                foreach (var item in data)
                {
                    int nam = item.nam.Value;
                    string ten = item.tenloai;
                    int sl = item.soluongdh ?? 0;
                    if (ten != null)
                    {
                        if (!rows.ContainsKey(nam))
                            rows[nam] = new Dictionary<string, int>();
                        rows[nam][ten] = sl;
                        matHang.Add(ten);
                    }
                }
                List<string> namList = new List<string>();
                foreach (var mh in matHang)
                {
                    sldonhang[mh] = new List<int>();
                }
                int datenow = DateTime.Now.Year - 4;
                for (int i = datenow; i <= datenow + 4; i++)
                {
                    namList.Add("Năm " + i);
                    foreach (var mh in matHang)
                    {
                        if (rows.ContainsKey(i) && rows[i].ContainsKey(mh))
                            sldonhang[mh].Add(rows[i][mh]);
                        else
                            sldonhang[mh].Add(0);
                    }
                }
                ltk = namList;
            }
            var series = new List<object>();
            foreach (var mh in sldonhang)
            {
                series.Add(new { name = mh.Key, data = mh.Value });
            }
            return Json(new { ltk = ltk, series = series });
        }
        public ActionResult TKDT()
        {
            if (Role != "admin" && Role != "nv")
            {
                return new HttpStatusCodeResult(403);
            }
            int dtt = con.sp_dtt().First().tongtien != null ? con.sp_dtt().First().tongtien.Value : 0;
            ViewBag.dtt = dtt;
            int pnt = con.sp_pnt().First().tongtien != null ? con.sp_pnt().First().tongtien.Value : 0;
            ViewBag.pnt = pnt;
            ViewBag.lnt = dtt - pnt;
            List<sp_top5mhdtResult> top5 = con.sp_top5mhdt().ToList();
            List<string> tenmh = new List<string>();
            List<int> doanhthumh = new List<int>();
            foreach (var item in top5)
            {
                tenmh.Add(item.tenmh.ToString());
                doanhthumh.Add(item.doanhthu.Value);
            }
            ViewBag.tenmh = JsonConvert.SerializeObject(tenmh);
            ViewBag.doanhthumh = JsonConvert.SerializeObject(doanhthumh);
            List<sp_doanhthu_theoloai_thangResult> lmh = con.sp_doanhthu_theoloai_thang().ToList();
            List<string> tenloai = new List<string>();
            List<int> doanhthu = new List<int>();
            foreach (var item in lmh)
            {
                tenloai.Add(item.tenloai.ToString());
                doanhthu.Add(item.doanhthu.Value);
            }
            ViewBag.pieloai = JsonConvert.SerializeObject(tenloai);
            ViewBag.piesll = JsonConvert.SerializeObject(doanhthu);
            return View();
        }
        [HttpPost]
        public ActionResult TKDT(string id)
        {
            if (Role != "admin" && Role != "nv")
            {
                return new HttpStatusCodeResult(403);
            }
            List<string> ltk = new List<string>();
            List<int> doanhthu = new List<int>();
            Dictionary<int, int> rows = new Dictionary<int, int>();
            if (id == "dtt")
            {
                List<sp_doanhthu_theothangResult> data = con.sp_doanhthu_theothang().ToList();
                foreach (var item in data)
                {
                    int thang = item.thang;
                    int dt = item.doanhthu;
                    rows[thang] = dt;
                }
                for (int i = 1; i <= 12; i++)
                {
                    ltk.Add("Tháng " + i);
                    doanhthu.Add(rows.ContainsKey(i) ? rows[i] : 0);
                }
            }
            else if (id == "dtq")
            {
                List<sp_doanhthu_theoquyResult> data = con.sp_doanhthu_theoquy().ToList();
                foreach (var item in data)
                {
                    int quy = item.quy;
                    int dt = item.doanhthu;
                    rows[quy] = dt;
                }
                for (int i = 1; i <= 4; i++)
                {
                    ltk.Add("Quý " + i);
                    doanhthu.Add(rows.ContainsKey(i) ? rows[i] : 0);
                }
            }
            else
            {
                List<sp_DoanhThu5NamGanNhatResult> data = con.sp_DoanhThu5NamGanNhat().ToList();
                foreach (var item in data)
                {
                    int nam = item.nam.Value;
                    int dt = item.doanhthu;
                    rows[nam] = dt;
                }
                int namHienTai = DateTime.Now.Year;
                for (int i = namHienTai - 4; i <= namHienTai; i++)
                {
                    ltk.Add("Năm " + i);
                    doanhthu.Add(rows.ContainsKey(i) ? rows[i] : 0);
                }
            }
            var series = new List<object> { new { name = "Doanh thu", data = doanhthu } };
            return Json(new { ltk = ltk, series = series });
        }

    }
}