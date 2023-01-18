using Staj_Ime_TakipDegerlendirmeSistemi.Filters;
using Staj_Ime_TakipDegerlendirmeSistemi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace Staj_Ime_TakipDegerlendirmeSistemi.Controllers
{
    public class AdminController : Controller
    {
        Entities1 db = new Entities1();

        // GET: Admin

        [AuthFilter]
        public ActionResult Index()
        {
            ViewBag.admin = Session["admin"];

            AdminIndexViewModel model = new AdminIndexViewModel()
            {
                toplamBasvuruAdet = db.Basvurular.Count(),
                devamEdenBasvuruAdet = db.Basvurular.Where(x => (x.BasvuruDurumu == 2 || x.BasvuruDurumu==1) && x.BasariNotu==null).Count(),
                tamamlanmisBasvuruAdet = db.Basvurular.Where(x => x.BasvuruDurumu == 1 && x.BasariNotu != null).Count(),
                reddedilenBasvuruAdet = db.Basvurular.Where(x => x.BasvuruDurumu == 3).Count(),

                toplamKullanıcıSayısı=db.Kullanicilar.Where(x=>x.isActive==true).Count(),
                toplamOgrenciAdet = db.Kullanicilar.Where(x => x.Rol == 5 && x.isActive == true).Count(),
                toplamOgretmenAdet = db.Kullanicilar.Where(x => (x.Rol == 3 || x.Rol == 4) && x.isActive == true).Count(),
                toplamYoneticiAdet = db.Kullanicilar.Where(x => (x.Rol == 1 || x.Rol == 2) && x.isActive == true).Count(),
                
                toplamKomisyonOgretmenAdet=db.Kullanicilar.Where(x=>x.Rol==3 && x.isActive == true).Count()

            };

            return View(model);

        }

        [HttpGet]
        public ActionResult Login()
        {

            return View();
        }

        [HttpPost]
        public ActionResult Login(Kullanicilar yonetici)
        {
            string md5sifre = Crypto.Hash(yonetici.Sifre, algorithm: "md5");
            var deger = db.Kullanicilar.Where(x => x.Email == yonetici.Email && x.Sifre == md5sifre && (x.Rol == 1 || x.Rol == 2) && x.isActive == true).FirstOrDefault();


            ModelState.Remove("Adi");
            ModelState.Remove("Soyadi");
            ModelState.Remove("Sinifi");
            ModelState.Remove("OkulSicilNo");


            if (ModelState.IsValid && deger == null)
            {
                ModelState.AddModelError("", "Girilen bilgilere göre bir yönetici bulunamadı.");
                ViewBag.result = "Girilen bilgilere göre bir yönetici bulunamadı.";
            }

            if (ModelState.IsValid && deger != null)
            {
                Session["admin"] = deger;
                return RedirectToAction("Index");
            }


            return View();

        }


        [HttpGet][AuthFilter]
        public ActionResult Students()
        {
            var page = 1;
            var limit = 4;
            var startlimit = (page * limit) - limit;

            var students = db.Kullanicilar.Where(x => x.Rol == 5 && x.isActive == true).OrderBy(x => x.OkulSicilNo).Skip(startlimit).Take(limit).ToList();

            var studentCount = db.Kullanicilar.Where(x => x.Rol == 5 && x.isActive == true).Count();
            var totalPageNumber = Convert.ToInt32(Math.Ceiling((double)studentCount / (double)limit));

            UsersViewModel studentsViewModel = new UsersViewModel()
            {
                users = students,
                page = 1,
                totalPageNumber = totalPageNumber,
                numeration = 1
            };

            ViewBag.result = TempData["success"];


            return View(studentsViewModel);
        }

        [AuthFilter]
        public ActionResult StudentAdd()
        {

            StudentAddViewModel studentAddViewModel = new StudentAddViewModel()
            {
                ddlfakulteData = new SelectList(db.Fakulteler.ToList(), "ID", "FakulteAdi"),
                ddlbolumData = new SelectList(db.Bolumler.ToList(), "ID", "BolumAdi"),
                ddlsinifData = new SelectList(db.Siniflar.ToList(), "ID", "Sinif"),
            };


            ViewBag.result = TempData["success"];

            return View(studentAddViewModel);
        }


        [HttpGet]
        public JsonResult GetBolumByFakulteID(int id)
        {

            int fakulteId = id;

            var sonuc = db.Bolumler.Select(x => new { bolumID = x.ID, bolumAdi = x.BolumAdi, fakulte = x.Fakulte }).Where(x => x.fakulte == fakulteId);


            return Json(sonuc.ToList(), JsonRequestBehavior.AllowGet);

        }


        [HttpPost][AuthFilter]
        public ActionResult StudentAdd(StudentAddViewModel model, HttpPostedFileBase profilResmi)
        {

            if (profilResmi != null)
            {

                if (Directory.Exists(Server.MapPath("~/files")) == false)
                {
                    Directory.CreateDirectory(Server.MapPath("~/files/uploads/images/students"));
                }

                var fileExtension = profilResmi.ContentType.Split('/')[1];
                var ogrName = model.ogr.Adi.Replace(" ", "");
                var imageName = model.ogr.OkulSicilNo + "_" + ogrName.ToLower() + "_" + model.ogr.Soyadi.ToLower() + "." + fileExtension;

                profilResmi.SaveAs(Path.Combine(Server.MapPath("~/uploads/images/students"), imageName));
                model.ogr.Fotograf = imageName;
            }


            Bolumler bolum = db.Bolumler.Find(model.bolum.ID);
            model.ogr.Bolumler = bolum;

            Roller rol = db.Roller.Find(5);
            model.ogr.Roller = rol;

            Siniflar sinif = db.Siniflar.Find(model.sinif.ID);
            model.ogr.Siniflar = sinif;


            var sifre = Crypto.Hash(Crypto.GenerateSalt(),algorithm:"md5");
            model.ogr.Sifre = sifre;
            model.ogr.Bolum = model.bolum.ID;
            model.ogr.Sinifi = model.sinif.ID;
            model.ogr.KayitTarihi = DateTime.Now;
            model.ogr.isActive = true;
            model.ogr.Rol = 5;


            //ModelState.Remove("Basvurular");
            //ModelState.Remove("Bolumler");
            //ModelState.Remove("TCNo");

            ModelState.Remove("ogr.Sifre");
            ModelState.Remove("ogr.Sinifi");



            var i = ModelState.IsValid;

            var t = db.Kullanicilar.Where(x => (x.OkulSicilNo == model.ogr.OkulSicilNo || x.Email == model.ogr.Email) && x.Rol == 5).FirstOrDefault();

            if (ModelState.IsValid && t == null)
            {
                db.Kullanicilar.Add(model.ogr);
                int deger = db.SaveChanges();

                if (deger == 1)
                {

                    TempData["success"] = "Başarıyla yeni öğrenci kaydı eklenmiştir.";

                    return RedirectToAction("StudentAdd");
                }

            }
            else
            {
                if (!ModelState.IsValid)
                {
                    return View();
                }
                else if (t != null)
                {
                    //ModelState.AddModelError("", "Girilen bilgilere göre kayıtlı öğrenci bulunmaktadır.");

                    ViewBag.error = "Girilen bilgilere göre kayıtlı öğrenci bulunmaktadır.";


                    StudentAddViewModel studentAddViewModel = new StudentAddViewModel()
                    {
                        ddlfakulteData = new SelectList(db.Fakulteler.ToList(), "ID", "FakulteAdi", model.fakulte.ID),
                        ddlbolumData = new SelectList(db.Bolumler.Where(x => x.Fakulte == model.fakulte.ID).ToList(), "ID", "BolumAdi", model.bolum.ID),
                        ddlsinifData = new SelectList(db.Siniflar.ToList(), "ID", "Sinif", model.sinif.ID),
                    };
                    studentAddViewModel.ogr = model.ogr;
                    ViewBag.secilibolum = model.bolum.ID;
                    return View(studentAddViewModel);
                }
            }


            return RedirectToAction("StudentAdd");
        }


        [AuthFilter]
        public ActionResult StudentUpdate(int id)
        {
            Kullanicilar ogrenci = db.Kullanicilar.Find(id);
            var deger = db.Bolumler.Where(x => x.ID == ogrenci.Bolum).FirstOrDefault();

            StudentAddViewModel studentAddViewModel = new StudentAddViewModel()
            {
                ddlfakulteData = new SelectList(db.Fakulteler.ToList(), "ID", "FakulteAdi",deger.Fakulte),
                ddlbolumData = new SelectList(db.Bolumler.Where(x => x.Fakulte == deger.Fakulte).ToList(), "ID", "BolumAdi", ogrenci.Bolum),
                ddlsinifData = new SelectList(db.Siniflar.ToList(), "ID", "Sinif", ogrenci.Sinifi),
                sifre=""
            };

            studentAddViewModel.ogr = ogrenci;

            ViewBag.secilibolum = ogrenci.Bolum;


            TempData["ogrenci"] = ogrenci;

            return View(studentAddViewModel);
        }

        [HttpPost][AuthFilter]
        public ActionResult StudentUpdate(StudentAddViewModel model, HttpPostedFileBase profilResmi)
        {
            //Kullanicilar ogrenciEskiVeri = (Kullanicilar)TempData["ogrenci"];

            Kullanicilar ogrenciEskiVeri = db.Kullanicilar.Find(model.ogr.ID);

            if (profilResmi != null)
            {

                var fileExtension = profilResmi.ContentType.Split('/')[1];
                var ogrName = model.ogr.Adi.Replace(" ", "");
                var imageName = model.ogr.OkulSicilNo + "_" + ogrName.ToLower() + "_" + model.ogr.Soyadi.ToLower() + "." + fileExtension;

                profilResmi.SaveAs(Path.Combine(Server.MapPath("~/uploads/images/students"), imageName));
                model.ogr.Fotograf = imageName;
            }


            Bolumler bolum = db.Bolumler.Find(model.bolum.ID);
            model.ogr.Bolumler = bolum;

            Siniflar sinif = db.Siniflar.Find(model.sinif.ID);
            model.ogr.Siniflar = sinif;


            model.ogr.Bolum = model.bolum.ID;
            model.ogr.Sinifi = model.sinif.ID;
            //model.ogr.KayitTarihi = DateTime.Now;
            model.ogr.isActive = true;
            model.ogr.Rol = 5;

            //ModelState.Remove("ogr.Sifre");
            ModelState.Remove("ogr.Sinifi");
            ModelState.Remove("ogr.Sifre");


            var i = ModelState.IsValid;

            var ogrenci = db.Kullanicilar.Find(ogrenciEskiVeri.ID);


            Kullanicilar t = db.Kullanicilar.Where(x => (x.OkulSicilNo == model.ogr.OkulSicilNo || x.Email == model.ogr.Email) && x.Rol == 5 && x.ID != ogrenci.ID).FirstOrDefault();

            if (ModelState.IsValid && t == null)
            {
                int kontrol = 0;
                if (model.ogr.Fotograf != null)
                {
                    ogrenci.Fotograf = model.ogr.Fotograf;
                    kontrol = 1;
                }

                ogrenci.Adi = model.ogr.Adi;
                ogrenci.Soyadi = model.ogr.Soyadi;
                ogrenci.Email = model.ogr.Email;

                if (model.sifre!=null)
                {
                    ogrenci.Sifre = Crypto.Hash(model.sifre, algorithm: "md5");
                }
                
                ogrenci.Sinifi = model.sinif.ID;
                ogrenci.TCNo = model.ogr.TCNo;
                ogrenci.Telefon = model.ogr.Telefon;
                ogrenci.OkulSicilNo = model.ogr.OkulSicilNo;
                ogrenci.Bolum = model.bolum.ID;


                int deger = db.SaveChanges();

                if (deger == 1 || kontrol == 1)
                {

                    TempData["success"] = "Öğrenci başarıyla güncellenmiştir.";

                    return RedirectToAction("Students");
                }

            }
            else
            {
                if (!ModelState.IsValid)
                {
                    // ViewBag.error = "Girilen bilgilere göre kayıtlı öğrenci bulunmaktadır.";

                    var deger = db.Kullanicilar.Find();
                    StudentAddViewModel studentAddViewModel = new StudentAddViewModel()
                    {
                        ddlfakulteData = new SelectList(db.Fakulteler.ToList(), "ID", "FakulteAdi", model.fakulte.ID),
                        ddlbolumData = new SelectList(db.Bolumler.Where(x => x.Fakulte == model.fakulte.ID).ToList(), "ID", "BolumAdi", model.bolum.ID),
                        ddlsinifData = new SelectList(db.Siniflar.ToList(), "ID", "Sinif", model.sinif.ID),
                        sifre=""
                    };
                    studentAddViewModel.ogr = model.ogr;
                    ViewBag.secilibolum = model.bolum.ID;

                    studentAddViewModel.ogr.Fotograf = ogrenci.Fotograf;

                    return View(studentAddViewModel);

                }
                else if (t != null)
                {
                    //ModelState.AddModelError("", "Girilen bilgilere göre kayıtlı öğrenci bulunmaktadır.");

                    ViewBag.error = "Girilen bilgilere göre kayıtlı öğrenci bulunmaktadır.";


                    StudentAddViewModel studentAddViewModel = new StudentAddViewModel()
                    {
                        ddlfakulteData = new SelectList(db.Fakulteler.ToList(), "ID", "FakulteAdi", model.fakulte.ID),
                        ddlbolumData = new SelectList(db.Bolumler.Where(x => x.Fakulte == model.fakulte.ID).ToList(), "ID", "BolumAdi", model.bolum.ID),
                        ddlsinifData = new SelectList(db.Siniflar.ToList(), "ID", "Sinif", model.sinif.ID),
                        sifre = ""
                    };

                    studentAddViewModel.ogr = model.ogr;
                    ViewBag.secilibolum = model.bolum.ID;
                    studentAddViewModel.ogr.Fotograf = ogrenci.Fotograf;

                    return View(studentAddViewModel);
                }
            }


            return RedirectToAction("Students");


        }


        [HttpPost][AuthFilter]
        public PartialViewResult UserDelete(int id, string page)
        {
            var user = db.Kullanicilar.Find(id);
            user.isActive = false;
            db.SaveChanges();


            var limit = 4;
            var startlimit = (Convert.ToInt32(page) * limit) - limit;
            List<Kullanicilar> users;
            int userCount;

            if (user.Rol == 4 || user.Rol == 3)
            {
                users = db.Kullanicilar.Where(x => (x.Rol == 4 || x.Rol == 3) && x.isActive == true).OrderBy(x => x.OkulSicilNo).Skip(startlimit).Take(limit).ToList();

                userCount = db.Kullanicilar.Where(x => (x.Rol == 4 || x.Rol == 3) && x.isActive == true).Count();
            }
            else
            {
                users = db.Kullanicilar.Where(x => x.Rol == user.Rol && x.isActive == true).OrderBy(x => x.OkulSicilNo).Skip(startlimit).Take(limit).ToList();

                userCount = db.Kullanicilar.Where(x => x.Rol == user.Rol && x.isActive == true).Count();
            }


            var totalPageNumber = Convert.ToInt32(Math.Ceiling((double)userCount / (double)limit));

            UsersViewModel usersViewModel = new UsersViewModel()
            {
                users = users,
                page = Convert.ToInt32(page),
                totalPageNumber = totalPageNumber,
                numeration = (startlimit + 1)
            };

            if (user.Rol == 4 || user.Rol == 3)
            {
                return PartialView("_TeachersTablePartialView", usersViewModel);
            }
            else
            {
                return PartialView("_StudentsTablePartialView", usersViewModel);
            }


        }

        [HttpPost][AuthFilter]
        public PartialViewResult GetUsersBySearch(string search, int rol)
        {
            if (rol == 4)
            {
                var s = search.ToLower();
                var users = db.Kullanicilar.Where(x => (x.Adi.ToLower().Contains(s) ||
                x.Soyadi.ToLower().Contains(s) ||
                x.Email.ToLower().Contains(s) ||
                x.OkulSicilNo.Contains(s) ||
                x.TCNo.Contains(s) ||
                x.Bolumler.BolumAdi.ToLower().Contains(s))
                && (x.Rol == 4 || x.Rol == 3) && x.isActive == true
                ).OrderBy(x => x.OkulSicilNo).ToList();



                UsersViewModel usersViewModel = new UsersViewModel()
                {
                    users = users,
                    page = 1,
                    numeration = 1
                };


                return PartialView("_TeachersTablePartialView", usersViewModel);
            }
            else
            {
                var s = search.ToLower();
                var users = db.Kullanicilar.Where(x => (x.Adi.ToLower().Contains(s) ||
                x.Soyadi.ToLower().Contains(s) ||
                x.Email.ToLower().Contains(s) ||
                x.OkulSicilNo.Contains(s) ||
                x.TCNo.Contains(s) ||
                x.Bolumler.BolumAdi.ToLower().Contains(s))
                && x.Rol == rol && x.isActive == true
                ).OrderBy(x => x.OkulSicilNo).ToList();



                UsersViewModel usersViewModel = new UsersViewModel()
                {
                    users = users,
                    page = 1,
                    numeration = 1
                };


                return PartialView("_StudentsTablePartialView", usersViewModel);
            }

        }

        [HttpPost][AuthFilter]
        public PartialViewResult GetUsersByPage(string page, int rol)
        {

            if (rol == 4)
            {
                var limit = 4;
                var startlimit = (Convert.ToInt32(page) * limit) - limit;

                var users = db.Kullanicilar.Where(x => (x.Rol == 4 || x.Rol == 3) && x.isActive == true).OrderBy(x => x.OkulSicilNo).Skip(startlimit).Take(limit).ToList();

                var userCount = db.Kullanicilar.Where(x => (x.Rol == 4 || x.Rol == 3) && x.isActive == true).Count();
                var totalPageNumber = Convert.ToInt32(Math.Ceiling((double)userCount / (double)limit));

                UsersViewModel usersViewModel = new UsersViewModel()
                {
                    users = users,
                    page = Convert.ToInt32(page),
                    totalPageNumber = totalPageNumber,
                    numeration = (startlimit + 1)
                };

                return PartialView("_TeachersTablePartialView", usersViewModel);
            }
            else
            {
                var limit = 4;
                var startlimit = (Convert.ToInt32(page) * limit) - limit;

                var users = db.Kullanicilar.Where(x => x.Rol == rol && x.isActive == true).OrderBy(x => x.OkulSicilNo).Skip(startlimit).Take(limit).ToList();

                var userCount = db.Kullanicilar.Where(x => x.Rol == rol && x.isActive == true).Count();
                var totalPageNumber = Convert.ToInt32(Math.Ceiling((double)userCount / (double)limit));

                UsersViewModel usersViewModel = new UsersViewModel()
                {
                    users = users,
                    page = Convert.ToInt32(page),
                    totalPageNumber = totalPageNumber,
                    numeration = (startlimit + 1)
                };

                return PartialView("_StudentsTablePartialView", usersViewModel);
            }

        }

        [HttpPost][AuthFilter]
        public PartialViewResult GetPaginationByPage(string page, int rol)
        {
            if (rol == 4)
            {
                var limit = 4;
                var userCount = db.Kullanicilar.Where(x => (x.Rol == 4 || x.Rol == 3) && x.isActive == true).Count();
                var totalPageNumber = Convert.ToInt32(Math.Ceiling((double)userCount / (double)limit));

                UsersViewModel usersViewModel = new UsersViewModel()
                {
                    page = Convert.ToInt32(page),
                    totalPageNumber = totalPageNumber
                };
                return PartialView("_StudentsPaginationPartialView", usersViewModel);
            }
            else
            {
                var limit = 4;
                var userCount = db.Kullanicilar.Where(x => x.Rol == rol && x.isActive == true).Count();
                var totalPageNumber = Convert.ToInt32(Math.Ceiling((double)userCount / (double)limit));

                UsersViewModel usersViewModel = new UsersViewModel()
                {
                    page = Convert.ToInt32(page),
                    totalPageNumber = totalPageNumber
                };
                return PartialView("_StudentsPaginationPartialView", usersViewModel);
            }

        }

        [AuthFilter]
        public ActionResult AdminProfile()
        {
            Kullanicilar admin = (Kullanicilar)Session["admin"];

            ViewBag.result = TempData["success"];

            var deger = db.Kullanicilar.Where(x => x.Email == admin.Email && x.Sifre == admin.Sifre && (x.Rol == 1 || x.Rol == 2)).FirstOrDefault();
            AdminProfileViewModel adminProfileViewModel = new AdminProfileViewModel() {admin=deger,sifre="" };
            
            return View(adminProfileViewModel);
        }

        [HttpPost][AuthFilter]
        public ActionResult AdminProfile(AdminProfileViewModel model, HttpPostedFileBase profilResmi, int id)
        {
            if (profilResmi != null)
            {

                var fileExtension = profilResmi.ContentType.Split('/')[1];
                var adminName = model.admin.Adi.Replace(" ", "");
                var imageName = model.admin.OkulSicilNo + "_" + adminName.ToLower() + "_" + model.admin.Soyadi.ToLower() + "." + fileExtension;

                profilResmi.SaveAs(Path.Combine(Server.MapPath("~/uploads/images/administrator"), imageName));
                model.admin.Fotograf = imageName;
            }

            ModelState.Remove("admin.OkulSicilNo");
            ModelState.Remove("admin.Sinifi");
            ModelState.Remove("admin.Sifre");

            var i = ModelState.IsValid;

            var admin = db.Kullanicilar.Find(id);


            Kullanicilar t = db.Kullanicilar.Where(x => (x.OkulSicilNo == model.admin.OkulSicilNo || x.Email == model.admin.Email) && (x.Rol == 1 || x.Rol == 2) && x.ID != admin.ID).FirstOrDefault();

            if (ModelState.IsValid && t == null)
            {
                int deger;
                int kontrol = 0;

                if (model.admin.Fotograf != null)
                {
                    admin.Fotograf = model.admin.Fotograf;
                    kontrol = 1;
                }

                admin.Adi = model.admin.Adi;
                admin.Soyadi = model.admin.Soyadi;
                admin.Email = model.admin.Email;

                if (model.sifre!=null)
                {
                    admin.Sifre = Crypto.Hash(model.sifre, algorithm: "md5");
                }
               
                admin.TCNo = model.admin.TCNo;
                admin.Telefon = model.admin.Telefon;
                admin.OkulSicilNo = model.admin.OkulSicilNo;
                admin.Rol = 1;
                admin.isActive = true;


                deger = db.SaveChanges();




                if (deger == 1 || kontrol == 1)
                {
                    Session["admin"] = admin;

                    TempData["success"] = "Profiliniz başarıyla güncellenmiştir.";

                    return RedirectToAction("AdminProfile");
                }

            }
            else
            {
                if (!ModelState.IsValid)
                {
                    var deger = db.Kullanicilar.Find(id);

                    deger.Adi = model.admin.Adi;
                    deger.Soyadi = model.admin.Soyadi;
                    deger.Email = model.admin.Email;
                    deger.OkulSicilNo = model.admin.OkulSicilNo;
                    deger.Telefon = model.admin.Telefon;
                    deger.TCNo = model.admin.TCNo;
                   
                    
                    AdminProfileViewModel adminProfileViewModel = new AdminProfileViewModel() { admin = deger, sifre = "" };

                    return View(adminProfileViewModel);
                   

                }
                else if (t != null)
                {

                    ViewBag.error = "Girilen bilgilere göre kayıtlı yönetici bulunmaktadır.";

                    var deger = db.Kullanicilar.Find(id);

                    if (model.admin.Fotograf != null)
                    {
                        deger.Fotograf = model.admin.Fotograf;
                    }

                    deger.Adi = model.admin.Adi;
                    deger.Soyadi = model.admin.Soyadi;
                    deger.Email = model.admin.Soyadi;
                    deger.OkulSicilNo = model.admin.Soyadi;
                    deger.Telefon = model.admin.Soyadi;
                    deger.TCNo = model.admin.Soyadi;


                    AdminProfileViewModel adminProfileViewModel = new AdminProfileViewModel() { admin = deger, sifre = "" };

                    return View(adminProfileViewModel);

                   
                }
            }


            return RedirectToAction("AdminProfile");
        }

        [AuthFilter]
        public ActionResult Logout()
        {
            //Session.Clear();
            Session.Remove("admin");
            return RedirectToAction("Login");
        }



        [HttpGet][AuthFilter]
        public ActionResult Teachers()
        {
            var page = 1;
            var limit = 4;
            var startlimit = (page * limit) - limit;

            var teachers = db.Kullanicilar.Where(x => (x.Rol == 4 || x.Rol == 3) && x.isActive == true).OrderBy(x => x.OkulSicilNo).Skip(startlimit).Take(limit).ToList();

            var teacherCount = db.Kullanicilar.Where(x => (x.Rol == 4 || x.Rol == 3) && x.isActive == true).Count();
            var totalPageNumber = Convert.ToInt32(Math.Ceiling((double)teacherCount / (double)limit));

            UsersViewModel teacherViewModel = new UsersViewModel()
            {
                users = teachers,
                page = 1,
                totalPageNumber = totalPageNumber,
                numeration = 1
            };

            ViewBag.result = TempData["success"];


            return View(teacherViewModel);

        }


        [AuthFilter]
        public ActionResult TeacherAdd()
        {

            TeacherAddViewModel teacherAddViewModel = new TeacherAddViewModel()
            {
                ddlfakulteData = new SelectList(db.Fakulteler.ToList(), "ID", "FakulteAdi"),
                ddlbolumData = new SelectList(db.Bolumler.ToList(), "ID", "BolumAdi"),
                ddlkomisyonData = new SelectList(db.Komisyonlar.ToList(), "ID", "KomisyonAdi"),
            };


            ViewBag.result = TempData["success"];

            return View(teacherAddViewModel);

        }

        [HttpPost][AuthFilter]
        public ActionResult TeacherAdd(TeacherAddViewModel model, HttpPostedFileBase profilResmi)
        {

            if (profilResmi != null)
            {

                if (Directory.Exists(Server.MapPath("~/files")) == false)
                {
                    Directory.CreateDirectory(Server.MapPath("~/files/uploads/images/teachers"));
                }

                var fileExtension = profilResmi.ContentType.Split('/')[1];
                var ogrName = model.teacher.Adi.Replace(" ", "");
                var imageName = model.teacher.OkulSicilNo + "_" + ogrName.ToLower() + "_" + model.teacher.Soyadi.ToLower() + "." + fileExtension;

                profilResmi.SaveAs(Path.Combine(Server.MapPath("~/uploads/images/teachers"), imageName));
                model.teacher.Fotograf = imageName;
            }


            Bolumler bolum = db.Bolumler.Find(model.bolum.ID);
            model.teacher.Bolumler = bolum;

            

            var stajKomisyon = db.Komisyonlar.Where(x => x.KomisyonAdi == "Staj ve IME Komisyonu").FirstOrDefault();

            if (model.komisyon.ID == stajKomisyon.ID)
            {
                Roller rol = db.Roller.Find(3);
                model.teacher.Roller = rol;
                model.teacher.Rol = 3;
            }
            else
            {
                Roller rol = db.Roller.Find(4);
                model.teacher.Roller = rol;
                model.teacher.Rol = 4;
            }


            //var sifre = Crypto.Hash(Crypto.GenerateSalt(), algorithm: "md5");

            model.teacher.Sifre = Crypto.Hash(model.teacher.Sifre, algorithm: "md5");
            model.teacher.Bolum = model.bolum.ID;

            model.teacher.KayitTarihi = DateTime.Now;
            model.teacher.isActive = true;


            //ModelState.Remove("teacher.Sifre");
            ModelState.Remove("teacher.Sinifi");



            var i = ModelState.IsValid;

            var t = db.Kullanicilar.Where(x => (x.OkulSicilNo == model.teacher.OkulSicilNo || x.Email == model.teacher.Email) && (x.Rol == 3 || x.Rol == 4)).FirstOrDefault();

            if (ModelState.IsValid && t == null)
            {
                var teacher = db.Kullanicilar.Add(model.teacher);
                int deger = db.SaveChanges();

                if (deger == 1)
                {
                    if (model.komisyon.ID != null)
                    {
                        KomisyonOgretmen komisyonOgretmen = new KomisyonOgretmen()
                        {
                            Komisyon = model.komisyon.ID.Value,
                            Ogretmen = model.teacher.ID,
                            Komisyonlar = db.Komisyonlar.Find(model.komisyon.ID),
                            Kullanicilar = teacher
                        };
                        db.KomisyonOgretmen.Add(komisyonOgretmen);
                        db.SaveChanges();

                    }

                    TempData["success"] = "Başarıyla yeni öğretmen kaydı eklenmiştir.";

                    return RedirectToAction("TeacherAdd");
                }

            }
            else
            {
                if (!ModelState.IsValid)
                {
                    if (model.komisyon.ID != null)
                    {
                        TeacherAddViewModel teacherAddViewModel = new TeacherAddViewModel()
                        {
                            ddlfakulteData = new SelectList(db.Fakulteler.ToList(), "ID", "FakulteAdi", model.fakulte.ID),
                            ddlbolumData = new SelectList(db.Bolumler.Where(x => x.Fakulte == model.fakulte.ID).ToList(), "ID", "BolumAdi", model.bolum.ID),
                            ddlkomisyonData = new SelectList(db.Komisyonlar.ToList(), "ID", "KomisyonAdi", model.komisyon.ID),
                        };
                        teacherAddViewModel.teacher = model.teacher;
                        ViewBag.secilibolum = model.bolum.ID;

                        return View(teacherAddViewModel);
                    }
                    else
                    {
                        TeacherAddViewModel teacherAddViewModel = new TeacherAddViewModel()
                        {
                            ddlfakulteData = new SelectList(db.Fakulteler.ToList(), "ID", "FakulteAdi", model.fakulte.ID),
                            ddlbolumData = new SelectList(db.Bolumler.Where(x => x.Fakulte == model.fakulte.ID).ToList(), "ID", "BolumAdi", model.bolum.ID),
                            ddlkomisyonData = new SelectList(db.Komisyonlar.ToList(), "ID", "KomisyonAdi"),
                        };
                        teacherAddViewModel.teacher = model.teacher;
                        ViewBag.secilibolum = model.bolum.ID;

                        return View(teacherAddViewModel);
                    }

                }
                else if (t != null)
                {
                    //ModelState.AddModelError("", "Girilen bilgilere göre kayıtlı öğrenci bulunmaktadır.");

                    ViewBag.error = "Girilen bilgilere göre kayıtlı öğretmen bulunmaktadır.";

                    if (model.komisyon.ID != null)
                    {
                        TeacherAddViewModel teacherAddViewModel = new TeacherAddViewModel()
                        {
                            ddlfakulteData = new SelectList(db.Fakulteler.ToList(), "ID", "FakulteAdi", model.fakulte.ID),
                            ddlbolumData = new SelectList(db.Bolumler.Where(x => x.Fakulte == model.fakulte.ID).ToList(), "ID", "BolumAdi", model.bolum.ID),
                            ddlkomisyonData = new SelectList(db.Komisyonlar.ToList(), "ID", "KomisyonAdi", model.komisyon.ID),
                        };
                        teacherAddViewModel.teacher = model.teacher;
                        ViewBag.secilibolum = model.bolum.ID;

                        return View(teacherAddViewModel);
                    }
                    else
                    {
                        TeacherAddViewModel teacherAddViewModel = new TeacherAddViewModel()
                        {
                            ddlfakulteData = new SelectList(db.Fakulteler.ToList(), "ID", "FakulteAdi", model.fakulte.ID),
                            ddlbolumData = new SelectList(db.Bolumler.Where(x => x.Fakulte == model.fakulte.ID).ToList(), "ID", "BolumAdi", model.bolum.ID),
                            ddlkomisyonData = new SelectList(db.Komisyonlar.ToList(), "ID", "KomisyonAdi"),
                        };
                        teacherAddViewModel.teacher = model.teacher;
                        ViewBag.secilibolum = model.bolum.ID;

                        return View(teacherAddViewModel);
                    }


                }
            }


            return RedirectToAction("TeacherAdd");
        }


        [AuthFilter]
        public ActionResult TeacherUpdate(int id)
        {

            Kullanicilar teacher = db.Kullanicilar.Find(id);
            var deger = db.Bolumler.Where(x => x.ID == teacher.Bolum).FirstOrDefault();
            KomisyonOgretmen komisyonOgretmen = db.KomisyonOgretmen.Where(x => x.Ogretmen == teacher.ID).FirstOrDefault();

            if (teacher.Rol == 3)
            {
                TeacherAddViewModel teacherAddViewModel = new TeacherAddViewModel()
                {
                    ddlfakulteData = new SelectList(db.Fakulteler.ToList(), "ID", "FakulteAdi", deger.Fakulte),
                    ddlbolumData = new SelectList(db.Bolumler.Where(x => x.Fakulte == deger.Fakulte).ToList(), "ID", "BolumAdi", teacher.Bolum),
                    ddlkomisyonData = new SelectList(db.Komisyonlar.ToList(), "ID", "KomisyonAdi", komisyonOgretmen.Komisyon),
                    sifre=""
                };

                teacherAddViewModel.teacher = teacher;

                ViewBag.secilibolum = teacher.Bolum;


                TempData["teacher"] = teacher;

                return View(teacherAddViewModel);
            }
            else
            {
                TeacherAddViewModel teacherAddViewModel = new TeacherAddViewModel()
                {
                    ddlfakulteData = new SelectList(db.Fakulteler.ToList(), "ID", "FakulteAdi", deger.Fakulte),
                    ddlbolumData = new SelectList(db.Bolumler.Where(x => x.Fakulte == deger.Fakulte).ToList(), "ID", "BolumAdi", teacher.Bolum),
                    ddlkomisyonData = new SelectList(db.Komisyonlar.ToList(), "ID", "KomisyonAdi"),
                    sifre=""
                };

                teacherAddViewModel.teacher = teacher;

                ViewBag.secilibolum = teacher.Bolum;


                TempData["teacher"] = teacher;

                return View(teacherAddViewModel);
            }


        }

        [HttpPost][AuthFilter]
        public ActionResult TeacherUpdate(TeacherAddViewModel model, HttpPostedFileBase profilResmi)
        {
            //Kullanicilar ogretmenEskiVeri = (Kullanicilar)TempData["teacher"];

            Kullanicilar ogretmenEskiVeri = db.Kullanicilar.Find(model.teacher.ID);

            if (profilResmi != null)
            {

                var fileExtension = profilResmi.ContentType.Split('/')[1];
                var teacherName = model.teacher.Adi.Replace(" ", "");
                var imageName = model.teacher.OkulSicilNo + "_" + teacherName.ToLower() + "_" + model.teacher.Soyadi.ToLower() + "." + fileExtension;

                profilResmi.SaveAs(Path.Combine(Server.MapPath("~/uploads/images/teachers"), imageName));
                model.teacher.Fotograf = imageName;
            }


            Bolumler bolum = db.Bolumler.Find(model.bolum.ID);
            model.teacher.Bolumler = bolum;

            //Siniflar sinif = db.Siniflar.Find(model.sinif.ID);
            //model.teacher.Siniflar = sinif;


            model.teacher.Bolum = model.bolum.ID;
            //model.teacher.Sinifi = model.sinif.ID;
            //model.teacher.KayitTarihi = DateTime.Now;
            model.teacher.isActive = true;

            if (model.komisyon.ID != null)
            {
                model.teacher.Rol = 3;
            }
            else
            {
                model.teacher.Rol = 4;
            }


            //ModelState.Remove("ogr.Sifre");

            ModelState.Remove("teacher.Sinifi");
            ModelState.Remove("teacher.Sifre");


            var i = ModelState.IsValid;

            var ogretmen = db.Kullanicilar.Find(ogretmenEskiVeri.ID);


            Kullanicilar t = db.Kullanicilar.Where(x => (x.OkulSicilNo == model.teacher.OkulSicilNo || x.Email == model.teacher.Email) && (x.Rol == 4 || x.Rol == 3) && x.ID != ogretmen.ID).FirstOrDefault();

            if (ModelState.IsValid && t == null)
            {
                int kontrol = 0;
                if (model.teacher.Fotograf != null)
                {
                    ogretmen.Fotograf = model.teacher.Fotograf;
                    kontrol = 1;
                }

                ogretmen.Adi = model.teacher.Adi;
                ogretmen.Soyadi = model.teacher.Soyadi;
                ogretmen.Email = model.teacher.Email;

                if (model.sifre!=null)
                {
                    ogretmen.Sifre = Crypto.Hash(model.sifre, algorithm: "md5");
                }
                
                //ogretmen.Sinifi = model.sinif.ID;
                ogretmen.TCNo = model.teacher.TCNo;
                ogretmen.Telefon = model.teacher.Telefon;
                ogretmen.OkulSicilNo = model.teacher.OkulSicilNo;
                ogretmen.Bolum = model.bolum.ID;



                if (model.komisyon.ID != null)
                {
                    var komisyonOgretmen = db.KomisyonOgretmen.Where(x => x.Ogretmen == ogretmen.ID).FirstOrDefault();

                    if (komisyonOgretmen != null)
                    {
                        komisyonOgretmen.Ogretmen = ogretmen.ID;
                        komisyonOgretmen.Komisyon = model.komisyon.ID.Value;
                        komisyonOgretmen.Komisyonlar = db.Komisyonlar.Find(model.komisyon.ID);
                        komisyonOgretmen.Kullanicilar = ogretmen;
                    }
                    else
                    {
                        KomisyonOgretmen komisyonOgretmen1 = new KomisyonOgretmen()
                        {
                            Komisyon = model.komisyon.ID.Value,
                            Ogretmen = ogretmen.ID,
                            Komisyonlar = db.Komisyonlar.Find(model.komisyon.ID),
                            Kullanicilar = ogretmen
                        };
                        db.KomisyonOgretmen.Add(komisyonOgretmen1);
                    }


                    ogretmen.Rol = 3;

                }
                else
                {

                    var komisyonOgretmen = db.KomisyonOgretmen.Where(x => x.Ogretmen == ogretmen.ID).FirstOrDefault();
                    if (komisyonOgretmen != null)
                    {
                        db.KomisyonOgretmen.Remove(komisyonOgretmen);
                    }

                    ogretmen.Rol = 4;

                }


                int deger = db.SaveChanges();

                if (deger > 0 || kontrol == 1)
                {

                    TempData["success"] = "Öğretmen başarıyla güncellenmiştir.";

                    return RedirectToAction("Teachers");
                }

            }
            else
            {
                if (!ModelState.IsValid)
                {
                    // ViewBag.error = "Girilen bilgilere göre kayıtlı öğretmen bulunmaktadır.";

                    if (model.komisyon.ID != null)
                    {
                        TeacherAddViewModel teacherAddViewModel = new TeacherAddViewModel()
                        {
                            ddlfakulteData = new SelectList(db.Fakulteler.ToList(), "ID", "FakulteAdi", model.fakulte.ID),
                            ddlbolumData = new SelectList(db.Bolumler.Where(x => x.Fakulte == model.fakulte.ID).ToList(), "ID", "BolumAdi", model.bolum.ID),
                            ddlkomisyonData = new SelectList(db.Komisyonlar.ToList(), "ID", "KomisyonAdi", model.komisyon.ID),
                            sifre=""
                        };
                        teacherAddViewModel.teacher = model.teacher;
                        ViewBag.secilibolum = model.bolum.ID;
                        teacherAddViewModel.teacher.Fotograf = ogretmen.Fotograf;

                        return View(teacherAddViewModel);
                    }
                    else
                    {
                        TeacherAddViewModel teacherAddViewModel = new TeacherAddViewModel()
                        {
                            ddlfakulteData = new SelectList(db.Fakulteler.ToList(), "ID", "FakulteAdi", model.fakulte.ID),
                            ddlbolumData = new SelectList(db.Bolumler.Where(x => x.Fakulte == model.fakulte.ID).ToList(), "ID", "BolumAdi", model.bolum.ID),
                            ddlkomisyonData = new SelectList(db.Komisyonlar.ToList(), "ID", "KomisyonAdi"),
                            sifre=""
                        };
                        teacherAddViewModel.teacher = model.teacher;
                        ViewBag.secilibolum = model.bolum.ID;
                        teacherAddViewModel.teacher.Fotograf = ogretmen.Fotograf;

                        return View(teacherAddViewModel);
                    }



                }
                else if (t != null)
                {
                    //ModelState.AddModelError("", "Girilen bilgilere göre kayıtlı öğrenci bulunmaktadır.");

                    ViewBag.error = "Girilen bilgilere göre kayıtlı öğretmen bulunmaktadır.";


                    if (model.komisyon.ID != null)
                    {
                        TeacherAddViewModel teacherAddViewModel = new TeacherAddViewModel()
                        {
                            ddlfakulteData = new SelectList(db.Fakulteler.ToList(), "ID", "FakulteAdi", model.fakulte.ID),
                            ddlbolumData = new SelectList(db.Bolumler.Where(x => x.Fakulte == model.fakulte.ID).ToList(), "ID", "BolumAdi", model.bolum.ID),
                            ddlkomisyonData = new SelectList(db.Komisyonlar.ToList(), "ID", "KomisyonAdi", model.komisyon.ID),
                            sifre=""
                        };
                        teacherAddViewModel.teacher = model.teacher;
                        ViewBag.secilibolum = model.bolum.ID;
                        teacherAddViewModel.teacher.Fotograf = ogretmen.Fotograf;

                        return View(teacherAddViewModel);
                    }
                    else
                    {
                        TeacherAddViewModel teacherAddViewModel = new TeacherAddViewModel()
                        {
                            ddlfakulteData = new SelectList(db.Fakulteler.ToList(), "ID", "FakulteAdi", model.fakulte.ID),
                            ddlbolumData = new SelectList(db.Bolumler.Where(x => x.Fakulte == model.fakulte.ID).ToList(), "ID", "BolumAdi", model.bolum.ID),
                            ddlkomisyonData = new SelectList(db.Komisyonlar.ToList(), "ID", "KomisyonAdi"),
                            sifre=""
                        };
                        teacherAddViewModel.teacher = model.teacher;
                        ViewBag.secilibolum = model.bolum.ID;
                        teacherAddViewModel.teacher.Fotograf = ogretmen.Fotograf;

                        return View(teacherAddViewModel);
                    }
                }
            }


            return RedirectToAction("Teachers");
        }
    }
}
