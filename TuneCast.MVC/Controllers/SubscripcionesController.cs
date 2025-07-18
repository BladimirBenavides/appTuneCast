using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TuneCastAPIConsumer;
using TuneCastModelo;

namespace TuneCast.MVC.Controllers
{
    public class SubscripcionesController : Controller
    {
        // GET: SubscripcionesController
        public ActionResult Index()
        {
            var data = Crud<Suscripcion>.GetAll();
            return View(data);
        }

        // GET: SubscripcionesController/Details/5
        public ActionResult Details(int id)
        {
            var data = Crud<Suscripcion>.GetById(id);
            return View(data);
        }

        // GET: SubscripcionesController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: SubscripcionesController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Suscripcion data)
        {
            try
            {
                Crud<Suscripcion>.Create(data);
                return RedirectToAction(nameof(Index));
            }
            catch(Exception ex) 
            {
                ModelState.AddModelError("", ex.Message);
                return View(data);
            }
        }

        // GET: SubscripcionesController/Edit/5
        public ActionResult Edit(int id)
        {
            var data = Crud<Suscripcion>.GetById(id);
            return View(data);
        }

        // POST: SubscripcionesController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Suscripcion data)
        {
            try
            {
                Crud<Suscripcion>.Update(id, data);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex) 
            {
                ModelState.AddModelError("", ex.Message);
                return View(data);
            }
        }

        // GET: SubscripcionesController/Delete/5
        public ActionResult Delete(int id)
        {
            var data = Crud<Suscripcion>.GetById(id);
            return View(data);
        }

        // POST: SubscripcionesController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, Suscripcion data)
        {
            try
            {
                Crud<Suscripcion>.Delete(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex) 
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }
        }
    }
}
