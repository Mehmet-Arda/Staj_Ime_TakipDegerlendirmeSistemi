//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Staj_Ime_TakipDegerlendirmeSistemi
{
    using System;
    using System.Collections.Generic;
    
    public partial class KomisyonOgretmen
    {
        public int ID { get; set; }
        public int Ogretmen { get; set; }
        public int Komisyon { get; set; }
    
        public virtual Komisyonlar Komisyonlar { get; set; }
        public virtual Kullanicilar Kullanicilar { get; set; }
    }
}
