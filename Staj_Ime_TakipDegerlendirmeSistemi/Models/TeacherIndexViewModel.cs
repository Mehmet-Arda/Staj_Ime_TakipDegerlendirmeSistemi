using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Staj_Ime_TakipDegerlendirmeSistemi.Models
{
    public class TeacherIndexViewModel
    {
        public Kullanicilar ogretmen { get; set; }
        public string rol { get; set; }

        public List<Kullanicilar> atanabilirOgretmenler { get; set; }


        public int atanmisOgrenciAdet { get; set; }

        public List<Basvurular> basvurular { get; set; }

        public List<Basvurular> onaylananTumBasvurular { get; set; }
        public List<Basvurular> beklemedeTumBasvurular { get; set; }
        public List<Basvurular> reddedilenTumBasvurular { get; set; }

        public int onaylananTumBasvurularCount { get; set; }
        public int beklemedeTumBasvurularCount { get; set; }
        public int reddedilenTumBasvurularCount { get; set; }

        public List<Basvurular> onaylananAtanmisBasvurular { get; set; }

        public int onaylananAtanmisBasvurularCount { get; set; }

        public int degerlendirilenBasvuruAdet { get; set; }
        public int degerlendirilmemisBasvuruAdet { get; set; }

        public List<Basvurular> stajIAtanmisBasvurular { get; set; }
        public List<Basvurular> stajIIAtanmisBasvurular { get; set; }
        public List<Basvurular> imeAtanmisBasvurular { get; set; }


        public TeacherPaginationViewModel paginationViewModel { get; set; }

        public TeacherPaginationOnlyViewModel paginationOnlyViewModel { get; set; }



    }
}