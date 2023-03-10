using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Staj_Ime_TakipDegerlendirmeSistemi.Models
{
    public class TeacherAddViewModel
    {
        public Kullanicilar teacher { get; set; }
        public Komisyonlar komisyon { get; set; }
        public Fakulteler fakulte { get; set; }
        public Bolumler bolum { get; set; }

        [DisplayName("Yeni Şifre")]
        public string sifre { get; set; }

        public SelectList ddlfakulteData { get; set; }
        public SelectList ddlbolumData { get; set; }
        public SelectList ddlkomisyonData { get; set; }

    }
}