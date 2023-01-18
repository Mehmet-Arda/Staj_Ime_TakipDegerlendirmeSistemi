using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace Staj_Ime_TakipDegerlendirmeSistemi.Models
{
    public class StudentBasvuruOlusturViewModel
    {
        public Kullanicilar ogrenci { get; set; }
        public BasvuruTuru basvuruTuru { get; set; }
        public Donemler basvuruDonem { get; set; }


        [DisplayName("Firma Adı"),Required(ErrorMessage ="Lütfen Firma Adını giriniz")]
        public string firmaAdi { get; set; }

        [Required(ErrorMessage ="Lütfen başvuru pdf'ini yükleyiniz")]
        public HttpPostedFileBase basvuruPdf { get; set; }


        public SelectList ddlbasvuruTuru{ get; set; }
        public SelectList ddlbasvuruDonem { get; set; }

    }
}