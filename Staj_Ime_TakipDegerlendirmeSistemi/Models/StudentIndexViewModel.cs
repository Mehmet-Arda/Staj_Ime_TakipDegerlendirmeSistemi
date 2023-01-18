using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Staj_Ime_TakipDegerlendirmeSistemi.Models
{
    public class StudentIndexViewModel
    {
        public Kullanicilar ogrenci { get; set; }
        public List<Basvurular> basvurular { get; set; }
        
        public List<Basvurular> onaylananBasvurular { get; set; }
        public List<Basvurular> beklemedeBasvurular { get; set; }
        public List<Basvurular> reddedilenBasvurular { get; set; }
      

    }
}