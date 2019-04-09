using BLL;
using Entity;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace MVCSozluk.Controllers
{
    [Authorize(Roles = "Admin")]
    public class WordsController : Controller
    {
        UnitOfWork _uw = new UnitOfWork();
        public ActionResult Index(int? langid, int? sil)
        {
            if (sil.HasValue)
            {
                _uw.Words.Delete(sil.Value);
                _uw.Complete();
                return RedirectToAction("Index", new { @langid = langid });
            }
            List<SelectListItem> langs = _uw.Languages.GetAll().Select(x => new SelectListItem()
            {
                Text = x.Name,
                Value = x.Id.ToString()
            }).ToList();

            langs.Insert(0, new SelectListItem()
            {
                Text = "Seçiniz",
                Value = ""
            });

            ViewData["LangOptions"] = langs;
            if (langid.HasValue)
            {
                ViewBag.langid = langid.Value;
                var list = _uw.Words.Search(x => x.Language_Id == langid.Value);
                return View(list);
            }
            else
                return View();
        }

        public ActionResult Create()
        {
            //ViewBag.LangOption = _uw.Languages.GetAll();
            //ViewData["LangOptions"] = _uw.Languages.GetAll();
            TempData["LangOptions"] = _uw.Languages.GetAll()
                .Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                });// TempData artı yönü controllerdan controllera bir seferlik aktarım yapıyor. Ordan başka bir sayfaya giderse veriyi saklamayacaktır.
            return View();
        }

        [HttpPost]
        public ActionResult Create(string WordTxt, int LanguageId)
        {
            if (string.IsNullOrEmpty(WordTxt))
                ModelState.AddModelError("", "Kelime boş bırakılamaz");

            if (ModelState.IsValid)
            {
                Word w = new Word();
                w.WordTxt = WordTxt;
                w.Language_Id = LanguageId;

                var langs = _uw.Languages.GetAll();

                w.Translations = new List<Word>();

                foreach (var item in langs)
                {
                    string input = Request.Form["ceviri" + item.Id];

                    string[] words = input.Split(',');

                    foreach (string item2 in words)
                    {
                        if (!string.IsNullOrEmpty(item2))
                        {
                            Word ceviri = new Word();
                            ceviri.Language_Id = item.Id;
                            ceviri.WordTxt = item2;
                            w.Translations.Add(ceviri);
                        }
                    }

                }

                _uw.Words.Add(w);
                _uw.Complete();
                return RedirectToAction("Index");
            }
            return View();
        }

        public JsonResult AutoComplete(int langid, string q)
        {
            var list = _uw.Words.Search(x => x.Language_Id == langid && x.WordTxt.ToLower().StartsWith(q.ToLower())).Select(x => x.WordTxt).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }
    }
}