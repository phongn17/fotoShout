using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FotoShoutPortal.Models;

namespace FotoShoutPortal.Controllers
{
    public class FakeController : Controller
    {
        private FotoShoutPortalContext db = new FotoShoutPortalContext();

        //
        // GET: /Fake/

        public ActionResult Index()
        {
            return View(db.Fakes.ToList());
        }

        //
        // GET: /Fake/Details/5

        public ActionResult Details(int id = 0)
        {
            Fake fake = db.Fakes.Find(id);
            if (fake == null)
            {
                return HttpNotFound();
            }
            return View(fake);
        }

        //
        // GET: /Fake/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Fake/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Fake fake)
        {
            if (ModelState.IsValid)
            {
                db.Fakes.Add(fake);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(fake);
        }

        //
        // GET: /Fake/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Fake fake = db.Fakes.Find(id);
            if (fake == null)
            {
                return HttpNotFound();
            }
            return View(fake);
        }

        //
        // POST: /Fake/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Fake fake)
        {
            if (ModelState.IsValid)
            {
                db.Entry(fake).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(fake);
        }

        //
        // GET: /Fake/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Fake fake = db.Fakes.Find(id);
            if (fake == null)
            {
                return HttpNotFound();
            }
            return View(fake);
        }

        //
        // POST: /Fake/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Fake fake = db.Fakes.Find(id);
            db.Fakes.Remove(fake);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}