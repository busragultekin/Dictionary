﻿using BLL;
using Entity;
using Entity.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace MVCSozluk.Controllers
{
    public class MembersController : Controller
    {
        UnitOfWork _uw = new UnitOfWork();
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult LogOff()
        {
            HttpContext.GetOwinContext().Authentication.SignOut();
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(Person person, string Pass, HttpPostedFileBase img)
        {
            UserStore<Person> store = new UserStore<Person>(UnitOfWork.Create()); //managera ulaşabilmek için kullanılıyor dbsetleri ayarlayan kısım
            UserManager<Person> manager = new UserManager<Person>(store);//update, şifre oluşturma, şifreyi unutup baştan oluşturma, her şeyi yapabilir manager
            var result = manager.Create(person, Pass);
            if (result.Succeeded)
            {
                //resim varsa kişinin id si ile kaydet //anasayfaya ya da logine yönlendir//Uploads/Members/
                if (img != null)
                {
                    string path = Server.MapPath("/Uploads/Members/");
                    img.SaveAs(path + person.Id + ".jpg");

                    person.HasPhoto = true;
                    manager.Update(person);
                }

                return RedirectToAction("Index", "Home");
            }
            else
            {
                //View içine hataları gönder //hataları uygun bir html ile göster
                ViewBag.Errors = result.Errors;
            }

            return View();
        }

        public ActionResult _LoginModal()
        {
            return View();
        }

        public JsonResult Login(LoginViewModel info)
        {
            //1.Sign in Managera ulaş
            ApplicationSignInManager signInManager = HttpContext.GetOwinContext().Get<ApplicationSignInManager>();

            //2.Giriş yapmayı dene (result döner)
            SignInStatus result = signInManager.PasswordSignIn(info.Email, info.Password, true, false);

            //3.Sonucu döndür
            switch (result)
            {
                case SignInStatus.Success:
                    return Json(new { success = true });
                case SignInStatus.Failure:
                    return Json(new { success = false }); ;
            }
            return Json(new { success = false });
        }

        public ActionResult MyAccount()
        {
            string uid = User.Identity.GetUserId();
            Person person = _uw.db.Users.Find(uid);

            if (person.HasPhoto)
            {
                ViewBag.Photo = "/Uploads/Members/" + uid + ".jpg";
            }
            MyAccountViewModel vm = new MyAccountViewModel();
            vm.Email = person.Email;
            vm.PhoneNumber = person.PhoneNumber;
            return View(vm);
        }

        [HttpPost]
        public ActionResult MyAccount(MyAccountViewModel info, HttpPostedFileBase imgFile)
        {
            UserStore<Person> store = new UserStore<Person>(UnitOfWork.Create());
            UserManager<Person> manager = new UserManager<Person>(store);

            string uid = User.Identity.GetUserId();
            Person person = manager.FindById(uid);
            person.Email = info.Email;
            person.PhoneNumber = info.PhoneNumber;

            if (imgFile != null)
            {
                string path = Server.MapPath("/Uploads/Members/");
                string old = path + person.Id + ".jpg";
                if (System.IO.File.Exists(old))
                    System.IO.File.Delete(old);

                string _new = path + person.Id + ".jpg";
                imgFile.SaveAs(_new);

                person.HasPhoto = true;
            }

            manager.Update(person);
            if (person.HasPhoto)
            {
                ViewBag.Photo = "/Uploads/Members/" + uid + ".jpg";
            }
            return View(info);
        }

        public ActionResult ChangePass()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ChangePass(string oldP, string newP)
        {
            UserStore<Person> store = new UserStore<Person>(_uw.db);
            UserManager<Person> manager = new UserManager<Person>(store);

            string uid = User.Identity.GetUserId();
            Person person = manager.FindById(uid);

            bool isCorrect = manager.CheckPassword(person, oldP);
            if (isCorrect)
            {
                IdentityResult r = manager.ChangePassword(uid, oldP, newP);
                if (r.Succeeded)
                    ViewBag.Success = true;
                else
                    ViewBag.Errors = r.Errors;
            }
            else
                ViewBag.WrongPassWord = false;
            return View();
        }
    }
}