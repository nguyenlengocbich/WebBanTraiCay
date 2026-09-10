using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebBanTraiCay.Models
{
	public class AccountKH
	{
        [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
        public string hovaten { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        public string sodienthoai { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        public string diachi { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        public string tendn { get; set; }
        [StringLength(12, MinimumLength = 8, ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự")]
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        public string matkhau { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập xác nhận mật khảu")]
        public string ConfirmPassword { get; set; }
    }
}