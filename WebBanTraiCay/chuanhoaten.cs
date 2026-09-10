using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;

namespace WebBanTraiCay
{
	public class chuanhoaten
	{
        public static string Chuanhoaten(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;
            string normalized = input.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();
            foreach (char c in normalized)
            {
                var unicodeCategory = Char.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }
            string noDiacritics = sb.ToString().Normalize(NormalizationForm.FormC);
            noDiacritics = noDiacritics.Replace('đ', 'd');
            return noDiacritics.Replace(" ", "");
        }
    }
}