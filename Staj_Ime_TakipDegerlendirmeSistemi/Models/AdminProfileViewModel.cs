using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Staj_Ime_TakipDegerlendirmeSistemi.Models
{
    public class AdminProfileViewModel
    {
        public Kullanicilar admin { get; set; }

        [DisplayName("Yeni Şifre")]
        public string sifre { get; set; }
    }
}