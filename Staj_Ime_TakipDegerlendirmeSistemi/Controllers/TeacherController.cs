using Staj_Ime_TakipDegerlendirmeSistemi.Filters;
using Staj_Ime_TakipDegerlendirmeSistemi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace Staj_Ime_TakipDegerlendirmeSistemi.Controllers
{
    public class TeacherController : Controller
    {
        Entities1 db = new Entities1();

        [TeacherAuthFilter]
        public ActionResult Index()
        {
            ViewBag.result = TempData["result"];
            ViewBag.resultbg = TempData["resultbg"];

            var page = 1;
            var limit = 4;
            var startlimit = (page * limit) - limit;



            var basvuruCount = db.Basvurular.Where(x => x.BasvuruDurumu == 2).Count();
            var totalPageNumber = Convert.ToInt32(Math.Ceiling((double)basvuruCount / (double)limit));



            //ViewBag.rol = Session["rol"];
            //ViewBag.teacher = Session["teacher"];

            Kullanicilar ogretmen = (Kullanicilar)Session["teacher"];
            //Kullanicilar ogretmen = db.Kullanicilar.Where(x => x.Email == "arnold@gmail.com" && x.Sifre == "sifre").FirstOrDefault();

            var basvuruOnlyCount = db.Basvurular.Where(x => x.BasvuruDurumu == 1 && x.DegerlendirenOgretmen == ogretmen.ID).Count();
            var totalPageOnlyNumber = Convert.ToInt32(Math.Ceiling((double)basvuruOnlyCount / (double)limit));


            var atanabilirOgretmenler = db.Kullanicilar.Where(x => (x.Rol == 3 || x.Rol == 4) && x.isActive == true).ToList();
            TeacherIndexViewModel model = new TeacherIndexViewModel()
            {
                ogretmen = ogretmen,
                atanabilirOgretmenler = atanabilirOgretmenler,
                rol = ogretmen.Rol.ToString(),

                basvurular = db.Basvurular.ToList(),

                onaylananTumBasvurular = db.Basvurular.Where(x => x.BasvuruDurumu == 1).OrderBy(x => x.BasvuranOgrenci).Take(limit).ToList(),
                beklemedeTumBasvurular = db.Basvurular.Where(x => x.BasvuruDurumu == 2).OrderBy(x => x.BasvuranOgrenci).Take(limit).ToList(),
                reddedilenTumBasvurular = db.Basvurular.Where(x => x.BasvuruDurumu == 3).OrderBy(x => x.BasvuranOgrenci).Take(limit).ToList(),
                atanmisOgrenciAdet = db.Basvurular.Select(x => new { ogrenci = x.BasvuranOgrenci, ogretmen = x.DegerlendirenOgretmen }).Where(x => x.ogretmen == ogretmen.ID).Distinct().Count(),



                onaylananTumBasvurularCount = db.Basvurular.Where(x => x.BasvuruDurumu == 1).ToList().Count(),
                beklemedeTumBasvurularCount = db.Basvurular.Where(x => x.BasvuruDurumu == 2).ToList().Count(),
                reddedilenTumBasvurularCount = db.Basvurular.Where(x => x.BasvuruDurumu == 3).ToList().Count(),


                onaylananAtanmisBasvurularCount = db.Basvurular.Where(x => x.BasvuruDurumu == 1 && x.DegerlendirenOgretmen == ogretmen.ID).Count(),
                degerlendirilenBasvuruAdet = db.Basvurular.Where(x => x.DegerlendirenOgretmen == ogretmen.ID && x.BasariNotu != null).Count(),
                degerlendirilmemisBasvuruAdet = db.Basvurular.Where(x => x.DegerlendirenOgretmen == ogretmen.ID && x.BasariNotu == null).Count(),


                onaylananAtanmisBasvurular = db.Basvurular.Where(x => x.BasvuruDurumu == 1 && x.DegerlendirenOgretmen == ogretmen.ID).OrderBy(x => x.BasvuranOgrenci).Take(limit).ToList(),
                stajIAtanmisBasvurular = db.Basvurular.Where(x => x.BasvuruDurumu == 1 && x.BasvuruTur == 1 && x.DegerlendirenOgretmen == ogretmen.ID).OrderBy(x => x.BasvuranOgrenci).Take(limit).ToList(),
                stajIIAtanmisBasvurular = db.Basvurular.Where(x => x.BasvuruDurumu == 1 && x.BasvuruTur == 2 && x.DegerlendirenOgretmen == ogretmen.ID).OrderBy(x => x.BasvuranOgrenci).Take(limit).ToList(),
                imeAtanmisBasvurular = db.Basvurular.Where(x => x.BasvuruDurumu == 1 && x.BasvuruTur == 3 && x.DegerlendirenOgretmen == ogretmen.ID).OrderBy(x => x.BasvuranOgrenci).Take(limit).ToList(),


                paginationViewModel = new TeacherPaginationViewModel()
                {
                    page = 1,
                    numeration = 1,
                    totalPageNumber = totalPageNumber
                },

                paginationOnlyViewModel = new TeacherPaginationOnlyViewModel()
                {
                    page = 1,
                    numeration = 1,
                    totalPageNumber = totalPageOnlyNumber
                }




            };
            return View(model);



        }


        public ActionResult Login()
        {
            ViewBag.result = TempData["result"];
            ViewBag.resultbg = "bg-success";

            return View();
        }


        [HttpPost]
        public ActionResult Login(Kullanicilar ogretmen)
        {
            var md5sifre = Crypto.Hash(ogretmen.Sifre, algorithm: "md5");
            var deger = db.Kullanicilar.Where(x => x.Email == ogretmen.Email && x.Sifre == md5sifre && (x.Rol == 3 || x.Rol == 4) && x.isActive == true).FirstOrDefault();

            ModelState.Remove("Adi");
            ModelState.Remove("Soyadi");
            ModelState.Remove("Sinifi");
            ModelState.Remove("OkulSicilNo");


            if (ModelState.IsValid && deger == null)
            {

                ViewBag.result = "Girilen Email veya şifre yanlış.";
                ViewBag.resultbg = "bg-danger";
            }

            if (ModelState.IsValid && deger != null)
            {
                Session["teacher"] = deger;
                Session["rol"] = deger.Rol;
                return RedirectToAction("Index");
            }


            return View();
        }

        [HttpPost]
        [TeacherAuthFilter]
        public PartialViewResult GetApplicationsByParams(string durum, string tur)
        {
            var limit = 4;
            List<Basvurular> basvurular = db.Basvurular.ToList();
            switch (durum)
            {
                case "1":
                    switch (tur)
                    {
                        case "1":
                            basvurular = db.Basvurular.Where(x => x.BasvuruDurumu == 1).OrderBy(x => x.BasvuranOgrenci).Take(limit).ToList();
                            return PartialView("_TeacherIndexTablePartialView", basvurular);


                        case "2":
                            basvurular = db.Basvurular.Where(x => x.BasvuruDurumu == 1 && x.BasvuruTur == 1).OrderBy(x => x.BasvuranOgrenci).Take(limit).ToList();
                            return PartialView("_TeacherIndexTablePartialView", basvurular);

                        case "3":
                            basvurular = db.Basvurular.Where(x => x.BasvuruDurumu == 1 && x.BasvuruTur == 2).OrderBy(x => x.BasvuranOgrenci).Take(limit).ToList();
                            return PartialView("_TeacherIndexTablePartialView", basvurular);

                        case "4":
                            basvurular = db.Basvurular.Where(x => x.BasvuruDurumu == 1 && x.BasvuruTur == 3).OrderBy(x => x.BasvuranOgrenci).Take(limit).ToList();
                            return PartialView("_TeacherIndexTablePartialView", basvurular);
                    }
                    break;

                case "2":
                    switch (tur)
                    {
                        case "1":
                            basvurular = db.Basvurular.Where(x => x.BasvuruDurumu == 2).OrderBy(x => x.BasvuranOgrenci).Take(limit).ToList();
                            return PartialView("_TeacherIndexTablePartialView", basvurular);

                        case "2":
                            basvurular = db.Basvurular.Where(x => x.BasvuruDurumu == 2 && x.BasvuruTur == 1).OrderBy(x => x.BasvuranOgrenci).Take(limit).ToList();
                            return PartialView("_TeacherIndexTablePartialView", basvurular);

                        case "3":
                            basvurular = db.Basvurular.Where(x => x.BasvuruDurumu == 2 && x.BasvuruTur == 2).OrderBy(x => x.BasvuranOgrenci).Take(limit).ToList();
                            return PartialView("_TeacherIndexTablePartialView", basvurular);

                        case "4":
                            basvurular = db.Basvurular.Where(x => x.BasvuruDurumu == 2 && x.BasvuruTur == 3).OrderBy(x => x.BasvuranOgrenci).Take(limit).ToList();
                            return PartialView("_TeacherIndexTablePartialView", basvurular);
                    }
                    break;

                case "3":
                    switch (tur)
                    {
                        case "1":
                            basvurular = db.Basvurular.Where(x => x.BasvuruDurumu == 3).OrderBy(x => x.BasvuranOgrenci).Take(limit).ToList();
                            return PartialView("_TeacherIndexTablePartialView", basvurular);

                        case "2":
                            basvurular = db.Basvurular.Where(x => x.BasvuruDurumu == 3 && x.BasvuruTur == 1).OrderBy(x => x.BasvuranOgrenci).Take(limit).ToList();
                            return PartialView("_TeacherIndexTablePartialView", basvurular);

                        case "3":
                            basvurular = db.Basvurular.Where(x => x.BasvuruDurumu == 3 && x.BasvuruTur == 2).OrderBy(x => x.BasvuranOgrenci).Take(limit).ToList();
                            return PartialView("_TeacherIndexTablePartialView", basvurular);

                        case "4":
                            basvurular = db.Basvurular.Where(x => x.BasvuruDurumu == 3 && x.BasvuruTur == 3).OrderBy(x => x.BasvuranOgrenci).Take(limit).ToList();
                            return PartialView("_TeacherIndexTablePartialView", basvurular);
                    }
                    break;

            }


            return PartialView("_TeacherIndexTablePartialView", basvurular);
        }


        [HttpPost]
        [TeacherAuthFilter]
        public PartialViewResult GetApplicationsByPage(int page, int durum, int tur)
        {
            List<Basvurular> basvurular = db.Basvurular.ToList();
            var basvuruCount = 0;

            var limit = 4;
            var startlimit = (page * limit) - limit;

            switch (tur)
            {
                case 1:
                    basvuruCount = db.Basvurular.Where(x => x.BasvuruDurumu == durum).ToList().Count();
                    basvurular = db.Basvurular.Where(x => x.BasvuruDurumu == durum).OrderBy(x => x.BasvuranOgrenci).Skip(startlimit).Take(limit).ToList();
                    break;
                case 2:
                    basvuruCount = db.Basvurular.Where(x => x.BasvuruDurumu == durum && x.BasvuruTur == 1).ToList().Count();
                    basvurular = db.Basvurular.Where(x => x.BasvuruDurumu == durum && x.BasvuruTur == 1).OrderBy(x => x.BasvuranOgrenci).Skip(startlimit).Take(limit).ToList();
                    break;

                case 3:
                    basvuruCount = db.Basvurular.Where(x => x.BasvuruDurumu == durum && x.BasvuruTur == 2).ToList().Count();
                    basvurular = db.Basvurular.Where(x => x.BasvuruDurumu == durum && x.BasvuruTur == 2).OrderBy(x => x.BasvuranOgrenci).Skip(startlimit).Take(limit).ToList();
                    break;
                case 4:
                    basvuruCount = db.Basvurular.Where(x => x.BasvuruDurumu == durum && x.BasvuruTur == 3).ToList().Count();
                    basvurular = db.Basvurular.Where(x => x.BasvuruDurumu == durum && x.BasvuruTur == 3).OrderBy(x => x.BasvuranOgrenci).Skip(startlimit).Take(limit).ToList();
                    break;

            }


            var totalPageNumber = Convert.ToInt32(Math.Ceiling((double)basvuruCount / (double)limit));

            TeacherPaginationViewModel model = new TeacherPaginationViewModel()
            {
                basvurular = basvurular,
                numeration = startlimit + 1,

            };

            return PartialView("_TeacherIndexTablePaginationPartialView", model);
        }


        [HttpPost]
        [TeacherAuthFilter]
        public PartialViewResult GetOnlyApplicationsByPage(int page, int durum, int tur)
        {
            Kullanicilar ogretmen = (Kullanicilar)Session["teacher"];
            List<Basvurular> basvurular = db.Basvurular.ToList();
            var basvuruCount = 0;

            var limit = 4;
            var startlimit = (page * limit) - limit;

            switch (tur)
            {
                case 1:
                    basvuruCount = db.Basvurular.Where(x => x.BasvuruDurumu == durum && x.DegerlendirenOgretmen == ogretmen.ID).ToList().Count();
                    basvurular = db.Basvurular.Where(x => x.BasvuruDurumu == durum && x.DegerlendirenOgretmen == ogretmen.ID).OrderBy(x => x.BasvuranOgrenci).Skip(startlimit).Take(limit).ToList();
                    break;
                case 2:
                    basvuruCount = db.Basvurular.Where(x => x.BasvuruDurumu == durum && x.BasvuruTur == 1 && x.DegerlendirenOgretmen == ogretmen.ID).ToList().Count();
                    basvurular = db.Basvurular.Where(x => x.BasvuruDurumu == durum && x.BasvuruTur == 1 && x.DegerlendirenOgretmen == ogretmen.ID).OrderBy(x => x.BasvuranOgrenci).Skip(startlimit).Take(limit).ToList();
                    break;

                case 3:
                    basvuruCount = db.Basvurular.Where(x => x.BasvuruDurumu == durum && x.BasvuruTur == 2 && x.DegerlendirenOgretmen == ogretmen.ID).ToList().Count();
                    basvurular = db.Basvurular.Where(x => x.BasvuruDurumu == durum && x.BasvuruTur == 2 && x.DegerlendirenOgretmen == ogretmen.ID).OrderBy(x => x.BasvuranOgrenci).Skip(startlimit).Take(limit).ToList();
                    break;
                case 4:
                    basvuruCount = db.Basvurular.Where(x => x.BasvuruDurumu == durum && x.BasvuruTur == 3 && x.DegerlendirenOgretmen == ogretmen.ID).ToList().Count();
                    basvurular = db.Basvurular.Where(x => x.BasvuruDurumu == durum && x.BasvuruTur == 3 && x.DegerlendirenOgretmen == ogretmen.ID).OrderBy(x => x.BasvuranOgrenci).Skip(startlimit).Take(limit).ToList();
                    break;

            }


            var totalPageNumber = Convert.ToInt32(Math.Ceiling((double)basvuruCount / (double)limit));

            TeacherPaginationOnlyViewModel model = new TeacherPaginationOnlyViewModel()
            {
                basvurular = basvurular,
                numeration = startlimit + 1,

            };

            return PartialView("_TeacherIndexTablePaginationOnlyPartialView", model);
        }



        [HttpPost]
        [TeacherAuthFilter]
        public PartialViewResult GetPaginationByPage(int page, int durum, int tur)
        {
            var basvuruCount = 0;
            List<Basvurular> basvurular = db.Basvurular.ToList();
            var limit = 4;
            var startlimit = (page * limit) - limit;

            switch (tur)
            {
                case 1:
                    basvuruCount = db.Basvurular.Where(x => x.BasvuruDurumu == durum).ToList().Count();
                    basvurular = db.Basvurular.Where(x => x.BasvuruDurumu == durum).OrderBy(x => x.BasvuranOgrenci).Skip(startlimit).Take(limit).ToList();
                    break;
                case 2:
                    basvuruCount = db.Basvurular.Where(x => x.BasvuruDurumu == durum && x.BasvuruTur == 1).ToList().Count();
                    basvurular = db.Basvurular.Where(x => x.BasvuruDurumu == durum && x.BasvuruTur == 1).OrderBy(x => x.BasvuranOgrenci).Skip(startlimit).Take(limit).ToList();
                    break;

                case 3:
                    basvuruCount = db.Basvurular.Where(x => x.BasvuruDurumu == durum && x.BasvuruTur == 2).ToList().Count();
                    basvurular = db.Basvurular.Where(x => x.BasvuruDurumu == durum && x.BasvuruTur == 2).OrderBy(x => x.BasvuranOgrenci).Skip(startlimit).Take(limit).ToList();
                    break;
                case 4:
                    basvuruCount = db.Basvurular.Where(x => x.BasvuruDurumu == durum && x.BasvuruTur == 3).ToList().Count();
                    basvurular = db.Basvurular.Where(x => x.BasvuruDurumu == durum && x.BasvuruTur == 3).OrderBy(x => x.BasvuranOgrenci).Skip(startlimit).Take(limit).ToList();
                    break;

            }


            var totalPageNumber = Convert.ToInt32(Math.Ceiling((double)basvuruCount / (double)limit));

            TeacherPaginationViewModel model = new TeacherPaginationViewModel()
            {
                page = page,
                totalPageNumber = totalPageNumber

            };


            return PartialView("_TeacherIndexPaginationPartialView", model);



        }


        [HttpPost]
        [TeacherAuthFilter]
        public PartialViewResult GetOnlyPaginationByPage(int page, int durum, int tur)
        {
            Kullanicilar ogretmen = (Kullanicilar)Session["teacher"];
            var basvuruCount = 0;
            List<Basvurular> basvurular = db.Basvurular.ToList();
            var limit = 4;
            var startlimit = (page * limit) - limit;

            switch (tur)
            {
                case 1:
                    basvuruCount = db.Basvurular.Where(x => x.BasvuruDurumu == durum && x.DegerlendirenOgretmen == ogretmen.ID).ToList().Count();
                    basvurular = db.Basvurular.Where(x => x.BasvuruDurumu == durum && x.DegerlendirenOgretmen == ogretmen.ID).OrderBy(x => x.BasvuranOgrenci).Skip(startlimit).Take(limit).ToList();
                    break;
                case 2:
                    basvuruCount = db.Basvurular.Where(x => x.BasvuruDurumu == durum && x.BasvuruTur == 1 && x.DegerlendirenOgretmen == ogretmen.ID).ToList().Count();
                    basvurular = db.Basvurular.Where(x => x.BasvuruDurumu == durum && x.BasvuruTur == 1 && x.DegerlendirenOgretmen == ogretmen.ID).OrderBy(x => x.BasvuranOgrenci).Skip(startlimit).Take(limit).ToList();
                    break;

                case 3:
                    basvuruCount = db.Basvurular.Where(x => x.BasvuruDurumu == durum && x.BasvuruTur == 2 && x.DegerlendirenOgretmen == ogretmen.ID).ToList().Count();
                    basvurular = db.Basvurular.Where(x => x.BasvuruDurumu == durum && x.BasvuruTur == 2 && x.DegerlendirenOgretmen == ogretmen.ID).OrderBy(x => x.BasvuranOgrenci).Skip(startlimit).Take(limit).ToList();
                    break;
                case 4:
                    basvuruCount = db.Basvurular.Where(x => x.BasvuruDurumu == durum && x.BasvuruTur == 3 && x.DegerlendirenOgretmen == ogretmen.ID).ToList().Count();
                    basvurular = db.Basvurular.Where(x => x.BasvuruDurumu == durum && x.BasvuruTur == 3 && x.DegerlendirenOgretmen == ogretmen.ID).OrderBy(x => x.BasvuranOgrenci).Skip(startlimit).Take(limit).ToList();
                    break;

            }


            var totalPageNumber = Convert.ToInt32(Math.Ceiling((double)basvuruCount / (double)limit));

            TeacherPaginationOnlyViewModel model = new TeacherPaginationOnlyViewModel()
            {
                page = page,
                totalPageNumber = totalPageNumber

            };


            return PartialView("_TeacherIndexPaginationOnlyPartialView", model);



        }



        [HttpPost]
        [TeacherAuthFilter]
        public PartialViewResult GetStudentInfoByModal(int id)
        {
            var ogrenci = db.Kullanicilar.Find(id);

            return PartialView("_TeacherIndexStudentInfoPartialView", ogrenci);
        }

        [HttpPost]
        [TeacherAuthFilter]
        public ActionResult ApproveApplication(int atananOgretmen, int ogrenciBasvuru)
        {

            var basvuru = db.Basvurular.Find(ogrenciBasvuru);

            basvuru.BasvuruDurumu = 1;
            basvuru.DegerlendirenOgretmen = atananOgretmen;

            basvuru.Kullanicilar1 = db.Kullanicilar.Find(atananOgretmen);
            basvuru.BasvuruDurum = db.BasvuruDurum.Find(1);
            db.SaveChanges();

            TempData["result"] = "Başvuru başarıyla onaylanmıştır";
            TempData["resultbg"] = "bg-success";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [TeacherAuthFilter]
        public ActionResult RejectApplication(int ogrenciBasvuru)
        {
            var basvuru = db.Basvurular.Find(ogrenciBasvuru);

            basvuru.BasvuruDurumu = 3;
            basvuru.BasvuruDurum = db.BasvuruDurum.Find(3);
            db.SaveChanges();

            TempData["result"] = "Başvuru reddedilmiştir";
            TempData["resultbg"] = "bg-danger";
            return RedirectToAction("Index");

        }

        [TeacherAuthFilter]
        public ActionResult TeacherProfile()
        {
            Kullanicilar teacher = (Kullanicilar)Session["teacher"];

            ViewBag.result = TempData["success"];
            ViewBag.resultbg = "bg-success";

            var deger = db.Kullanicilar.Where(x => x.Email == teacher.Email && x.isActive == true && (x.Rol == 4 || x.Rol == 3)).FirstOrDefault();

            TeacherProfileViewModel teacherProfileViewModel = new TeacherProfileViewModel()
            {
                ogretmen = deger,
                sifre = ""
            };

            return View(teacherProfileViewModel);


        }


        [HttpPost]
        [TeacherAuthFilter]
        public ActionResult TeacherProfile(TeacherProfileViewModel model, HttpPostedFileBase profilResmi)
        {
            var teacherOldValues = db.Kullanicilar.Find(model.ogretmen.ID);

            if (profilResmi != null)
            {

                var fileExtension = profilResmi.ContentType.Split('/')[1];
                var teacherName = teacherOldValues.Adi.Replace(" ", "");
                var imageName = teacherOldValues.OkulSicilNo + "_" + teacherName.ToLower() + "_" + teacherOldValues.Soyadi.ToLower() + "." + fileExtension;

                profilResmi.SaveAs(Path.Combine(Server.MapPath("~/uploads/images/teachers"), imageName));
                model.ogretmen.Fotograf = imageName;
            }

            ModelState.Remove("ogretmen.OkulSicilNo");
            ModelState.Remove("ogretmen.Sinifi");
            ModelState.Remove("ogretmen.Adi");
            ModelState.Remove("ogretmen.Soyadi");
            ModelState.Remove("ogretmen.Sifre");





            Kullanicilar t = db.Kullanicilar.Where(x => x.Email == model.ogretmen.Email && (x.Rol == 4 || x.Rol == 3) && x.ID != teacherOldValues.ID && x.isActive == true).FirstOrDefault();

            if (ModelState.IsValid && t == null)
            {
                int deger;
                int kontrol = 0;
                if (model.ogretmen.Fotograf != null)
                {
                    teacherOldValues.Fotograf = model.ogretmen.Fotograf;
                    kontrol = 1;
                }

                //student.Adi = model.Adi;
                //student.Soyadi = model.Soyadi;

                teacherOldValues.Email = model.ogretmen.Email;

                if (model.sifre != null)
                {
                    teacherOldValues.Sifre = Crypto.Hash(model.sifre, algorithm: "md5");
                }

                //student.TCNo = model.TCNo;

                teacherOldValues.Telefon = model.ogretmen.Telefon;

                //student.OkulSicilNo = model.OkulSicilNo;
                //teacherOldValues.Rol = 5;
                //teacherOldValues.isActive = true;


                deger = db.SaveChanges();




                if (deger == 1 || kontrol == 1)
                {
                    Session["teacher"] = teacherOldValues;
                    TempData["success"] = "Profiliniz başarıyla güncellenmiştir.";

                    return RedirectToAction("TeacherProfile");
                }

            }
            else
            {
                if (!ModelState.IsValid)
                {

                    teacherOldValues.Email = model.ogretmen.Email;
                    //teacherOldValues.Sifre = model.sifre;
                    teacherOldValues.Telefon = model.ogretmen.Telefon;

                    TeacherProfileViewModel teacherProfileViewModel = new TeacherProfileViewModel()
                    {
                        ogretmen = teacherOldValues,
                        sifre = ""
                    };

                    return View(teacherProfileViewModel);

                }
                else if (t != null)
                {

                    ViewBag.result = "Girilen bilgilere göre kayıtlı öğretmen bulunmaktadır.";
                    ViewBag.resultbg = "bg-danger";

                    teacherOldValues.Email = model.ogretmen.Email;
                    //teacherOldValues.Sifre = model.sifre;
                    teacherOldValues.Telefon = model.ogretmen.Telefon;

                    TeacherProfileViewModel teacherProfileViewModel = new TeacherProfileViewModel()
                    {
                        ogretmen = teacherOldValues,
                        sifre = ""
                    };

                    return View(teacherProfileViewModel);
                }
            }


            return RedirectToAction("TeacherProfile");
        }

        [HttpPost]
        [TeacherAuthFilter]
        public ActionResult RateApplication(int basvuruid, string score)
        {

            db.Basvurular.Find(basvuruid).BasariNotu = Convert.ToByte(score);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        [TeacherAuthFilter]
        public ActionResult Logout()
        {
            //Session.Clear();
            Session.Remove("teacher");
            return RedirectToAction("Login");
        }
    }
}