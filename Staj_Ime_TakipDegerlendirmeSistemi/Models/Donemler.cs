using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Staj_Ime_TakipDegerlendirmeSistemi.Models
{
    public class Donemler
    {
        [Required(ErrorMessage = "Lütfen Başvuru dönemi seçiniz")]
        public int ID { get; set; }
        public string donemAdi { get; set; }
    }
}