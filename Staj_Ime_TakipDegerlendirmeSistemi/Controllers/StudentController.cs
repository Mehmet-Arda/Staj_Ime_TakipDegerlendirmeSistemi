using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Staj_Ime_TakipDegerlendirmeSistemi.Models;
using System.Web.Helpers;
using System.Net.Mail;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using iTextSharp;
using iTextSharp.text.pdf;
using Staj_Ime_TakipDegerlendirmeSistemi.Filters;

namespace Staj_Ime_TakipDegerlendirmeSistemi.Controllers
{
    public static class PdfGenerateFromTemplate
    {
        public static byte[] PdfGenerateTemplate(
        string pdfJsonData, string pdfPath)
        {
            PdfReader pdfReader = new PdfReader(pdfPath);
            MemoryStream stream = new MemoryStream();
            PdfStamper stamper = new PdfStamper(pdfReader,
            stream);
            AcroFields pdfFormFields = stamper.AcroFields;
            var pdfDeserializeJsonData =
            JsonConvert.DeserializeObject<
            Dictionary<string, string>>(pdfJsonData);
            foreach (var pdfDeserializeJsonDataItem in
            pdfDeserializeJsonData)
            {
                pdfFormFields.SetField(
                pdfDeserializeJsonDataItem.Key,
                pdfDeserializeJsonDataItem.Value);
            }
            stamper.FormFlattening = true;
            stamper.Close();
            pdfReader.Close();

            stream.Flush();
            stream.Close();
            return stream.ToArray();

        }
    }

    public class StudentController : Controller
    {
        Entities1 db = new Entities1();

        public ActionResult Login()
        {
            ViewBag.result = TempData["result"];
            ViewBag.resultbg = "bg-success";

            return View();
        }

        [HttpPost]
        public ActionResult Login(Kullanicilar ogrenci)
        {
            var md5sifre = Crypto.Hash(ogrenci.Sifre, algorithm: "md5"); 
            var deger = db.Kullanicilar.Where(x => x.Email == ogrenci.Email && x.Sifre == md5sifre && x.Rol == 5 && x.isActive == true).FirstOrDefault();


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
                Session["student"] = deger;
                return RedirectToAction("Index");
            }


            return View();
        }


        public ActionResult StudentForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public JsonResult StudentForgotPasswordSendEmail(string email)
        {
            string result;
            var ogrenci = db.Kullanicilar.Where(x => x.Email == email && x.Rol == 5).FirstOrDefault();
            if (ogrenci == null)
            {
                result = "";
                return Json(result);
            }
            else
            {

                string kod = Crypto.GenerateSalt(8);
                kod = Crypto.Hash(kod, algorithm: "md5");
                kod = kod.Substring(3, 6);

                SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
                client.EnableSsl = true;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential("m.arda.yuksel@gmail.com", "zkxhcovvjadtjial");
                MailMessage msgObj = new MailMessage();
                msgObj.To.Add(email);
                msgObj.From = new MailAddress("m.arda.yuksel@gmail.com");
                msgObj.Subject = "Şifre Yenileme";
                msgObj.Body = $"Doğrulama kodunuz: {kod}";
                client.Send(msgObj);

                result = "Doğrulama email'i başarıyla gönderilmiştir.";
                //string[] res = new string[] { result, kod };
                TempData["kodEmail"] = new List<string> { kod, email };


                return Json(result);


            }


        }

        [HttpPost]
        public ActionResult StudentForgotPassword(StudentForgotPasswordViewModel deger)
        {
            var kodEmail = (List<string>)TempData["kodEmail"];


            if (deger.Kod != kodEmail[0].ToString())
            {
                ModelState.AddModelError("Kod", "Girilen kod eşleşmiyor.");
                return View();
            }
            else if (kodEmail[1] != deger.Email)
            {
                ModelState.AddModelError("Email", "Girilen email farklı.");
                return View();
            }
            else
            {
                var ogrenci = db.Kullanicilar.Where(x => x.Email == deger.Email && x.Rol == 5).FirstOrDefault();
                ogrenci.Sifre = Crypto.Hash(deger.Sifre, algorithm: "md5");
                db.SaveChanges();

                TempData["result"] = "Şifreniz başarıyla güncellenmiştir.";
                return RedirectToAction("Login");
            }


        }

        [StudentAuthFilter]
        public ActionResult Index()
        {
            if ((string)TempData["result"] == "success")
            {
                ViewBag.result = "Belgeler başarıyla yüklenmiştir";
                ViewBag.resultbg = "bg-success";
            }

            Kullanicilar ogrenci = (Kullanicilar)Session["student"];
            //Kullanicilar ogrenci = db.Kullanicilar.Where(x => x.Email == "m.arda.yuksel@gmail.com" && x.Rol == 5).FirstOrDefault();
            StudentIndexViewModel model = new StudentIndexViewModel()
            {
                ogrenci = ogrenci,
                basvurular = db.Basvurular.Where(x => x.BasvuranOgrenci == ogrenci.ID).ToList(),
                onaylananBasvurular = db.Basvurular.Where(x => x.BasvuranOgrenci == ogrenci.ID && x.BasvuruDurumu == 1).ToList(),
                beklemedeBasvurular = db.Basvurular.Where(x => x.BasvuranOgrenci == ogrenci.ID && x.BasvuruDurumu == 2).ToList(),
                reddedilenBasvurular = db.Basvurular.Where(x => x.BasvuranOgrenci == ogrenci.ID && x.BasvuruDurumu == 3).ToList(),
            };
            return View(model);
        }

        [StudentAuthFilter]
        public ActionResult StudentProfile()
        {
            Kullanicilar student = (Kullanicilar)Session["student"];

            ViewBag.result = TempData["success"];
            ViewBag.resultbg = "bg-success";

            var deger = db.Kullanicilar.Where(x => x.Email == student.Email && x.Rol == 5).FirstOrDefault();
            StudentProfileViewModel studentProfileViewModel = new StudentProfileViewModel()
            {
                ogrenci=deger,
                sifre=""
            };
            return View(studentProfileViewModel);

        }

        [HttpPost]
        [StudentAuthFilter]
        public ActionResult StudentProfile(StudentProfileViewModel model, HttpPostedFileBase profilResmi)
        {
            var studentOldValues = db.Kullanicilar.Find(model.ogrenci.ID);

            if (profilResmi != null)
            {

                var fileExtension = profilResmi.ContentType.Split('/')[1];
                var studentName = studentOldValues.Adi.Replace(" ", "");
                var imageName = studentOldValues.OkulSicilNo + "_" + studentName.ToLower() + "_" + studentOldValues.Soyadi.ToLower() + "." + fileExtension;

                profilResmi.SaveAs(Path.Combine(Server.MapPath("~/uploads/images/students"), imageName));
                model.ogrenci.Fotograf = imageName;
            }

            ModelState.Remove("ogrenci.OkulSicilNo");
            ModelState.Remove("ogrenci.Sinifi");
            ModelState.Remove("ogrenci.Adi");
            ModelState.Remove("ogrenci.Soyadi");
            ModelState.Remove("ogrenci.Sifre");






            Kullanicilar t = db.Kullanicilar.Where(x => x.Email == model.ogrenci.Email && x.Rol == 5 && x.ID != studentOldValues.ID).FirstOrDefault();

            if (ModelState.IsValid && t == null)
            {
                int deger;
                int kontrol = 0;
                if (model.ogrenci.Fotograf != null)
                {
                    studentOldValues.Fotograf = model.ogrenci.Fotograf;
                    kontrol = 1;
                }

                //student.Adi = model.Adi;
                //student.Soyadi = model.Soyadi;
                studentOldValues.Email = model.ogrenci.Email;

                if (model.sifre!=null)
                {
                    studentOldValues.Sifre = Crypto.Hash(model.sifre, algorithm: "md5");
                }
              
                //student.TCNo = model.TCNo;
                studentOldValues.Telefon = model.ogrenci.Telefon;
                //student.OkulSicilNo = model.OkulSicilNo;
                studentOldValues.Rol = 5;
                studentOldValues.isActive = true;


                deger = db.SaveChanges();




                if (deger == 1 || kontrol == 1)
                {
                    Session["student"] = studentOldValues;
                    TempData["success"] = "Profiliniz başarıyla güncellenmiştir.";

                    return RedirectToAction("StudentProfile");
                }

            }
            else
            {
                if (!ModelState.IsValid)
                {
                    studentOldValues.Email = model.ogrenci.Email;
                    //studentOldValues.Sifre = model.sifre;
                    studentOldValues.Telefon = model.ogrenci.Telefon;

                    StudentProfileViewModel studentProfileViewModel = new StudentProfileViewModel()
                    {
                        ogrenci = studentOldValues,
                        sifre = ""
                    };

                    return View(studentProfileViewModel);

                }
                else if (t != null)
                {

                    ViewBag.result = "Girilen bilgilere göre kayıtlı öğrenci bulunmaktadır.";
                    ViewBag.resultbg = "bg-danger";

                    studentOldValues.Email = model.ogrenci.Email;
                    //studentOldValues.Sifre = model.sifre;
                    studentOldValues.Telefon = model.ogrenci.Telefon;

                    StudentProfileViewModel studentProfileViewModel = new StudentProfileViewModel()
                    {
                        ogrenci = studentOldValues,
                        sifre = ""
                    };

                    return View(studentProfileViewModel);
                }
            }


            return RedirectToAction("StudentProfile");
        }

        [StudentAuthFilter]
        public ActionResult StajI_StajIIForm()
        {
            StajI_StajIIViewModel model = new StajI_StajIIViewModel();
            //var ogrenci = db.Kullanicilar.Find(2);
            var ogrenci = (Kullanicilar)Session["student"];
            model.ogrenci = ogrenci;

            model.AdSoyad = ogrenci.Adi + " " + ogrenci.Soyadi;
            model.EPosta = ogrenci.Email;
            model.Bolum = ogrenci.Bolumler.BolumAdi;
            model.Numara = ogrenci.OkulSicilNo;
            model.TCNo = ogrenci.TCNo;

            return View(model);
        }

        [HttpPost]
        [StudentAuthFilter]
        public ActionResult StajI_StajIIForm(StajI_StajIIViewModel model)
        {

            var pdfVal = model;
            pdfVal.Gun1 = model.Gun0;
            pdfVal.Ay1 = model.Ay0;
            pdfVal.Yil1 = model.Yil0;

            var serializeObject = JsonConvert.SerializeObject(pdfVal);
            var pdfPath = Server.MapPath("~/files/StajDocs/StajBasvuruKabulFormu.pdf");

            var pdfByte = PdfGenerateFromTemplate.PdfGenerateTemplate(serializeObject, pdfPath);

            System.Web.HttpContext.Current.Response.Clear();
            MemoryStream ms = new MemoryStream(pdfByte);

            var adSoyad = pdfVal.AdSoyad.Replace(" ", "").ToLower();

            System.Web.HttpContext.Current.Response.ContentType = "application/pdf";
            System.Web.HttpContext.Current.Response.AddHeader("content-disposition", "attachment;filename=" + adSoyad + "_StajBasvuruKabulFormu" + ".pdf");


            System.Web.HttpContext.Current.Response.Buffer = true;
            ms.WriteTo(System.Web.HttpContext.Current.Response.OutputStream);

            System.Web.HttpContext.Current.Response.End();

            return RedirectToAction("StajI_StajIIForm");




        }

        [StudentAuthFilter]
        public ActionResult BasvuruOlustur()
        {
            ViewBag.result = TempData["result"];
            ViewBag.resultbg = "bg-success";


            //var ogrenci = db.Kullanicilar.Find(26);

            var ogrenci = (Kullanicilar)Session["student"];

            List<Donemler> donemler = new List<Donemler>{
                new Donemler(){ID=1,donemAdi="2022-2023 Güz"},
                new Donemler(){ID=2,donemAdi="2022-2023 Bahar"},
                new Donemler(){ID=3,donemAdi="2022-2023 Yaz"},
                new Donemler(){ID=4,donemAdi="2023-2024 Güz"},
                new Donemler(){ID=5,donemAdi="2023-2024 Bahar"},
                new Donemler(){ID=6,donemAdi="2023-2024 Yaz"}

            };


            StudentBasvuruOlusturViewModel model = new StudentBasvuruOlusturViewModel()
            {
                ddlbasvuruDonem = new SelectList(donemler, "ID", "donemAdi"),
                ddlbasvuruTuru = new SelectList(db.BasvuruTuru.ToList(), "ID", "BasvuruAdi"),
                ogrenci = ogrenci

            };
            return View(model);
        }


        [HttpPost]
        [StudentAuthFilter]
        public ActionResult BasvuruOlustur(StudentBasvuruOlusturViewModel model)
        {
            var ogrenci = db.Kullanicilar.Where(x => x.ID == model.ogrenci.ID && x.Rol == 5).FirstOrDefault();
            model.ogrenci = ogrenci;

            List<Donemler> donemler = new List<Donemler>{
                new Donemler(){ID=1,donemAdi="2022-2023 Güz"},
                new Donemler(){ID=2,donemAdi="2022-2023 Bahar"},
                new Donemler(){ID=3,donemAdi="2022-2023 Yaz"},
                new Donemler(){ID=4,donemAdi="2023-2024 Güz"},
                new Donemler(){ID=5,donemAdi="2023-2024 Bahar"},
                new Donemler(){ID=6,donemAdi="2023-2024 Yaz"}
                };


            var deger = db.Basvurular.Where(x => x.BasvuruTur == model.basvuruTuru.ID && x.BasvuranOgrenci == model.ogrenci.ID && (x.BasvuruDurumu == 1 || x.BasvuruDurumu == 2)).FirstOrDefault();
            if (deger != null)
            {

                model.ddlbasvuruDonem = new SelectList(donemler, "ID", "donemAdi", model.basvuruDonem.ID);
                model.ddlbasvuruTuru = new SelectList(db.BasvuruTuru.ToList(), "ID", "BasvuruAdi", model.basvuruTuru.ID);


                ViewBag.result = "Aynı türde bir başvuru bulunmaktadır";
                ViewBag.resultbg = "bg-danger";

                return View(model);

            }
            else
            {

                var basvuruTarih = DateTime.Now;

                Basvurular basvuru = new Basvurular()
                {
                    BasvuranOgrenci = model.ogrenci.ID,
                    BasvuruDurumu = 2,
                    BasvuruDurum = db.BasvuruDurum.Find(2),
                    BasvuruTur = model.basvuruTuru.ID,
                    BasvuruTuru = db.BasvuruTuru.Find(model.basvuruTuru.ID),
                    Donem = donemler.ElementAt((model.basvuruDonem.ID - 1)).donemAdi,
                    BasvuruTarih = basvuruTarih,
                    Firma = model.firmaAdi
                };

                var basvuruDbEntity = db.Basvurular.Add(basvuru);
                db.SaveChanges();

                var fileExtension = "";
                var ogrName = "";
                var fileName = "";

                if (model.basvuruPdf.ContentType == "application/vnd.openxmlformats-officedocument.wordprocessingml.document")
                {
                    ogrName = model.ogrenci.Adi.Replace(" ", "");
                    fileName = model.ogrenci.OkulSicilNo + "_" + ogrName.ToLower() + "_" + model.ogrenci.Soyadi.ToLower() + "_StajBasvuruKabulFormu" + "." + "docx";
                }
                else
                {
                    fileExtension = model.basvuruPdf.ContentType.Split('/')[1];
                    ogrName = model.ogrenci.Adi.Replace(" ", "");
                    fileName = model.ogrenci.OkulSicilNo + "_" + ogrName.ToLower() + "_" + model.ogrenci.Soyadi.ToLower() + "_StajBasvuruKabulFormu" + "." + fileExtension;

                }


                switch (model.basvuruTuru.ID)
                {
                    case 1:
                        model.basvuruPdf.SaveAs(Path.Combine(Server.MapPath("~/uploads/applications/StajI"), fileName));
                        break;
                    case 2:
                        model.basvuruPdf.SaveAs(Path.Combine(Server.MapPath("~/uploads/applications/StajII"), fileName));
                        break;
                    case 3:
                        model.basvuruPdf.SaveAs(Path.Combine(Server.MapPath("~/uploads/applications/Ime"), fileName));
                        break;
                }

                var basvuruID = db.Basvurular.Where(x => x.BasvuranOgrenci == model.ogrenci.ID && x.BasvuruTarih == basvuruTarih).FirstOrDefault();

                var t = fileName.Length;

                Belgeler belge = new Belgeler()
                {
                    BelgeAdresi = fileName,
                    Basvurular = basvuruDbEntity,
                    Basvuru = basvuruDbEntity.ID,
                    BelgeTuru = 1,
                    BelgeTurleri = db.BelgeTurleri.Find(1)
                };


                db.Belgeler.Add(belge);
                db.SaveChanges();

                TempData["result"] = "Başvurunuz başarıyla oluşturulmuştur";
                return RedirectToAction("BasvuruOlustur");
            }

        }



        [StudentAuthFilter]
        public ActionResult SendApplicationDocuments(int ogrenciBasvuru, HttpPostedFileBase document0, HttpPostedFileBase document1)
        {
            var basvuru = db.Basvurular.Find(ogrenciBasvuru);
            var basvuranOgrenci = db.Basvurular.Find(ogrenciBasvuru).Kullanicilar;

            if (document0 != null)
            {

                var fileExtension = "";
                var studentName = "";
                var doc0Name = "";

                if (document0.ContentType == "application/vnd.openxmlformats-officedocument.wordprocessingml.document")
                {
                    studentName = basvuranOgrenci.Adi.Replace(" ", "");
                    doc0Name = basvuranOgrenci.OkulSicilNo + "_" + studentName.ToLower() + "_" + basvuranOgrenci.Soyadi.ToLower() + "_StajDegerlendirmeFormu" + "." + "docx";
                }
                else
                {
                    fileExtension = document0.ContentType.Split('/')[1];
                    studentName = basvuranOgrenci.Adi.Replace(" ", "");
                    doc0Name = basvuranOgrenci.OkulSicilNo + "_" + studentName.ToLower() + "_" + basvuranOgrenci.Soyadi.ToLower() + "_StajDegerlendirmeFormu" + "." + fileExtension;

                }

                //var fileExtension = document0.ContentType.Split('/')[1];
                //var studentName = basvuranOgrenci.Adi.Replace(" ", "");
                //var doc0Name = basvuranOgrenci.OkulSicilNo + "_" + studentName.ToLower() + "_" + basvuranOgrenci.Soyadi.ToLower() + "_StajDegerlendirmeFormu" + "." + fileExtension;

                if (basvuru.BasvuruTur == 1)
                {
                    document0.SaveAs(Path.Combine(Server.MapPath("~/uploads/applications/StajI"), doc0Name));
                }
                else if (basvuru.BasvuruTur == 2)
                {
                    document0.SaveAs(Path.Combine(Server.MapPath("~/uploads/applications/StajII"), doc0Name));
                }
                else if (basvuru.BasvuruTur == 3)
                {
                    document0.SaveAs(Path.Combine(Server.MapPath("~/uploads/applications/Ime"), doc0Name));

                }

                Belgeler belge = new Belgeler();
                belge.BelgeAdresi = doc0Name;
                belge.BelgeTuru = 2;
                belge.Basvuru = ogrenciBasvuru;

                db.Belgeler.Add(belge);

            }


            if (document1 != null)
            {

                var fileExtension = "";
                var studentName = "";
                var doc1Name = "";

                if (document1.ContentType == "application/vnd.openxmlformats-officedocument.wordprocessingml.document")
                {
                    studentName = basvuranOgrenci.Adi.Replace(" ", "");
                    doc1Name = basvuranOgrenci.OkulSicilNo + "_" + studentName.ToLower() + "_" + basvuranOgrenci.Soyadi.ToLower() + "_StajRaporu" + "." + "docx";
                }
                else
                {
                    fileExtension = document1.ContentType.Split('/')[1];
                    studentName = basvuranOgrenci.Adi.Replace(" ", "");
                    doc1Name = basvuranOgrenci.OkulSicilNo + "_" + studentName.ToLower() + "_" + basvuranOgrenci.Soyadi.ToLower() + "_StajRaporu" + "." + fileExtension;

                }


                if (basvuru.BasvuruTur == 1)
                {
                    document1.SaveAs(Path.Combine(Server.MapPath("~/uploads/applications/StajI"), doc1Name));
                }
                else if (basvuru.BasvuruTur == 2)
                {
                    document1.SaveAs(Path.Combine(Server.MapPath("~/uploads/applications/StajII"), doc1Name));
                }
                else if (basvuru.BasvuruTur == 3)
                {
                    document1.SaveAs(Path.Combine(Server.MapPath("~/uploads/applications/Ime"), doc1Name));

                }

                Belgeler belge = new Belgeler();
                belge.BelgeAdresi = doc1Name;
                belge.BelgeTuru = 3;
                belge.Basvuru = ogrenciBasvuru;

                db.Belgeler.Add(belge);


            }

            db.SaveChanges();

            TempData["result"] = "success";
            return RedirectToAction("Index");
        }

        [StudentAuthFilter]
        public ActionResult Logout()
        {
            //Session.Clear();
            Session.Remove("student");
            return RedirectToAction("Login");
        }
    }
}