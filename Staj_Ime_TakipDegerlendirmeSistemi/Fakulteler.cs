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
    using System.ComponentModel.DataAnnotations;

    public partial class Fakulteler
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Fakulteler()
        {
            this.Bolumler = new HashSet<Bolumler>();
        }

        [Required(ErrorMessage = "Fak�lte alan� bo� ge�ilemez")]
        public int ID { get; set; }
        public string FakulteAdi { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Bolumler> Bolumler { get; set; }
    }
}
