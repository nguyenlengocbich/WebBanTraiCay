using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Sdk.Sfc;
using Microsoft.SqlServer.Management.Smo;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanTraiCay.Models;

namespace WebBanTraiCay.Controllers
{
    [Authorize]
    public class HomeController : BaseController
    {
        WebsiteQuanLyBanTraicayDataContext con = new WebsiteQuanLyBanTraicayDataContext(MatHangController.conn);
        public ActionResult Index()
        {
            if (Role != "admin" && Role!="nv")
            {
                return new HttpStatusCodeResult(403);
            }
            int thangHienTai = DateTime.Now.Month;
            int namHienTai = DateTime.Now.Year;
            var listDoanhThu = con.sp_doanhthu_theothang().ToList();
            var dtThangNay = listDoanhThu.FirstOrDefault(x => x.thang == thangHienTai);
            int doanhthu = 0;
            if(dtThangNay != null)
            {
                doanhthu = dtThangNay.doanhthu;
            }
            var dondanggiao = con.sp_DemDonHangDangGiao().FirstOrDefault();
            int dongiao = 0;
            if (dondanggiao != null)
            {
                dongiao = dondanggiao.SoLuongdondanggiao.Value;
            }
            var sldh = con.sp_DonHangTrongTuan().FirstOrDefault();
            int sldontuannay = 0;
            if(sldh != null)
            {
                sldontuannay = sldh.slt.Value;
            }
            var hethang = con.sp_slmhhh().FirstOrDefault();
            int slhethang = 0;
            if (hethang != null)
            {
                slhethang = hethang.hethang.Value;
            }
            ViewBag.doanhthuthang = doanhthu;
            ViewBag.sldondanggiao = dongiao;
            ViewBag.sldonmoi = sldontuannay;
            ViewBag.slhethang = slhethang;
            var dsdonhangtuan = con.sp_DSDonHangTrongTuan().ToList();
            return View(dsdonhangtuan);
        }
        public ActionResult BackupRestore()
        {
            if (Role != "admin")
            {
                return new HttpStatusCodeResult(403);
            }
            string tempPath = Server.MapPath("~/TempBackups");
            var backupFiles = new List<BackupFile>();
            if (Directory.Exists(tempPath))
            {
                var files = Directory.GetFiles(tempPath, "*.bak").Select(f => new FileInfo(f)).OrderByDescending(f => f.CreationTime);
                int stt = 1;
                foreach (var file in files)
                {
                    backupFiles.Add(new BackupFile { STT = stt, FileName = file.Name, CreationTime = file.CreationTime, SizeKB = file.Length / 1024 });
                    stt++;
                }
            }
            return View(backupFiles);
        }
        [HttpPost]
        public ActionResult BackupDatabase()
        {
            if (Role != "admin")
            {
                return new HttpStatusCodeResult(403);
            }
            string tempPath = Server.MapPath("~/TempBackups");
            string databaseName = "qlchtraicay";
            string fileName = $"{databaseName}_{DateTime.Now:ddMMyyyy_HHmmss}";
            fileName += ".bak";
            string fullPath = Path.Combine(tempPath, fileName);
            SqlConnection con = new SqlConnection(MatHangController.conn);
            string query = $@"BACKUP DATABASE [{databaseName}] TO DISK = '{fullPath}' WITH NOFORMAT, NOINIT, NAME = N'{databaseName}-Full Database Backup', SKIP, NOREWIND, NOUNLOAD, STATS = 10";
            SqlCommand cmd = new SqlCommand(query, con);
            con.Open();
            cmd.ExecuteNonQuery();
            if (con != null)
                con.Close();
            TempData["SuccessMessage"] = $"Tạo bản sao lưu cho Database {databaseName} thành công!";
            return RedirectToAction("BackupRestore");
        }
        [HttpGet]
        public ActionResult DownloadBackup(string filename)
        {
            if (Role != "admin")
            {
                return new HttpStatusCodeResult(403);
            }
            string tempPath = Server.MapPath("~/TempBackups");
            string fullPath = Path.Combine(tempPath, filename);
            if (!System.IO.File.Exists(fullPath))
            {
                TempData["ErrorMessage"] = "Không tìm thấy file backup.";
                return RedirectToAction("BackupRestore");
            }
            byte[] fileBytes = System.IO.File.ReadAllBytes(fullPath);
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, filename);
        }
        [HttpPost]
        public ActionResult DeleteBackup(string fileName)
        {
            if (Role != "admin")
            {
                return new HttpStatusCodeResult(403);
            }
            string tempPath = Server.MapPath("~/TempBackups");
            string fullPath = Path.Combine(tempPath, fileName);
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
                TempData["SuccessMessage"] = $"Đã xóa file backup {fileName} thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy file cần xóa.";
            }

            return RedirectToAction("BackupRestore");
        }
        [HttpPost]
        public ActionResult RestoreDatabase(string fileName)
        {
            if (Role != "admin")
            {
                return new HttpStatusCodeResult(403);
            }
            string connectionString = MatHangController.conn;
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionString);
            string databaseName = builder.InitialCatalog;
            string tempPath = Server.MapPath("~/TempBackups");
            string fullPath = Path.Combine(tempPath, fileName);
            if (!System.IO.File.Exists(fullPath))
            {
                TempData["ErrorMessage"] = "Không tìm thấy file backup cần phục hồi.";
                return RedirectToAction("BackupRestore");
            }
            string masterConnectionString = $"Data Source={builder.DataSource};Initial Catalog=master;Integrated Security=True;Encrypt=False";
            using (SqlConnection connection = new SqlConnection(masterConnectionString))
            {
                connection.Open();
                string singleUserQuery = $"ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE";
                using (SqlCommand command = new SqlCommand(singleUserQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
                string restoreQuery = $@"RESTORE DATABASE [{databaseName}] FROM DISK = '{fullPath}'  WITH FILE = 1, NOUNLOAD, REPLACE";
                using (SqlCommand command = new SqlCommand(restoreQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
                string multiUserQuery = $"ALTER DATABASE [{databaseName}] SET MULTI_USER";
                using (SqlCommand command = new SqlCommand(multiUserQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
            TempData["SuccessMessage"] = $"Phục hồi database {databaseName} từ file {fileName} thành công!";
            return RedirectToAction("BackupRestore");
        }
    }
}
