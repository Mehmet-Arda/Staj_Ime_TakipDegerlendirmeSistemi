using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Staj_Ime_TakipDegerlendirmeSistemi.Models
{
    public class AdminIndexViewModel
    {
        public int toplamKullanıcıSayısı { get; set; }

        public int toplamOgrenciAdet { get; set; }
        public int toplamOgretmenAdet { get; set; }
        public int toplamKomisyonOgretmenAdet { get; set; }
        public int toplamYoneticiAdet { get; set; }


        public int toplamBasvuruAdet { get; set; }

        public int tamamlanmisBasvuruAdet { get; set; }
        public int devamEdenBasvuruAdet { get; set; }
        public int reddedilenBasvuruAdet { get; set; }


    }
}