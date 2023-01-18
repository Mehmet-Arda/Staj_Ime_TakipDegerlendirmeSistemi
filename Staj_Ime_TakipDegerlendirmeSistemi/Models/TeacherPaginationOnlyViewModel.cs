using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Staj_Ime_TakipDegerlendirmeSistemi.Models
{
    public class TeacherPaginationOnlyViewModel
    {
        public List<Basvurular> basvurular { get; set; }

        public int page { get; set; }
        public int totalPageNumber { get; set; }
        public int numeration { get; set; }
    }
}