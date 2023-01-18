using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Staj_Ime_TakipDegerlendirmeSistemi.Models
{
    public class StajI_StajIIViewModel
    {
        public Kullanicilar ogrenci { get; set; }

        [DisplayName("Ad Soyad")]
        public string AdSoyad { get; set; }

        [DisplayName("Bölüm")]
        public string Bolum { get; set; }

        [DisplayName("Numara")]
        public string Numara { get; set; }

        [DisplayName("TCNo")]
        public string TCNo { get; set; }

        [DisplayName("Ev/Tel Gsm")]
        public string EvTelGsm { get; set; }

        [DisplayName("E-posta")]
        public string EPosta { get; set; }

        [DisplayName("Gün")]
        public string Gun0 { get; set; }

        [DisplayName("Ay")]
        public string Ay0 { get; set; }

        [DisplayName("Yıl")]
        public string Yil0 { get; set; }

        [DisplayName("Gün")]
        public string Gun1 { get; set; }

        [DisplayName("Ay")]
        public string Ay1 { get; set; }

        [DisplayName("Yıl")]
        public string Yil1 { get; set; }



        [DisplayName("İl")]
        public string Il { get; set; }

        [DisplayName("İlçe")]
        public string Ilce { get; set; }

        [DisplayName("Posta Kodu")]
        public string PostaKodu { get; set; }

        [DisplayName("Gün")]
        public string StajBaslangicGun { get; set; }

        [DisplayName("Ay")]
        public string StajBaslangicAy { get; set; }

        [DisplayName("Yıl")]
        public string StajBaslangicYil { get; set; }

        [DisplayName("Gün")]
        public string StajBitisGun { get; set; }

        [DisplayName("Ay")]
        public string StajBitisAy { get; set; }

        [DisplayName("Yıl")]
        public string StajBitisYil { get; set; }



        [DisplayName("Resmi Adı")]
        public string KurumResmiAd { get; set; }

        [DisplayName("Faaliyet Alanı")]
        public string KurumFaaliyetAlani { get; set; }

        [DisplayName("İl")]
        public string KurumIl { get; set; }

        [DisplayName("İlçe")]
        public string KurumIlce { get; set; }

        [DisplayName("Posta Kodu")]
        public string KurumPostaKodu { get; set; }

        [DisplayName("Telefon")]
        public string KurumTelefon { get; set; }

        [DisplayName("Faks")]
        public string KurumFaks { get; set; }

        [DisplayName("E-posta")]
        public string KurumEPosta { get; set; }


        public string check1 { get; set; }
        public string check2 { get; set; }
        public string check3 { get; set; }
        public string check4 { get; set; }
        public string check5 { get; set; }
        public string check6 { get; set; }

    }
}