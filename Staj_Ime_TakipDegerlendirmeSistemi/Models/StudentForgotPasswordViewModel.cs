using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Staj_Ime_TakipDegerlendirmeSistemi.Models
{
    public class StudentForgotPasswordViewModel
    {

        [DisplayName("Email"), Required(ErrorMessage = "Email alanı gereklidir."), MinLength(3), MaxLength(30), DataType(DataType.EmailAddress), RegularExpression(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$", ErrorMessage = "Lütfen geçerli bir email adresi girin")]
        public string Email { get; set; }

        [DisplayName("Güncelleme Kodu"), Required(ErrorMessage = "Kod alanı gereklidir."), MinLength(3), MaxLength(30)]
        public string Kod { get; set; }

        [DisplayName("Şifre"), MinLength(3), MaxLength(50), Required(ErrorMessage = "Şifre alanı gereklidir.")]
        public string Sifre { get; set; }

        [DisplayName("Şifre (Tekrar)"), MinLength(3), MaxLength(50), Required(ErrorMessage = "Şifre(tekrar) alanı gereklidir."),Compare("Sifre",ErrorMessage ="Şifreler eşleşmiyor.")]
        public string SifreTekrar { get; set; }


    }
}