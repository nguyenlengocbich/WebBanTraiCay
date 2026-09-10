using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebBanTraiCay.Models
{
    public class BackupFile
    {
        public int STT { get; set; }
        public string FileName { get; set; }
        public DateTime CreationTime { get; set; }
        public long SizeKB { get; set; }
    }
}