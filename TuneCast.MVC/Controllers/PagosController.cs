using Microsoft.AspNetCore.Mvc;
using TuneCastAPIConsumer;
using TuneCastModelo;
using System;
using System.Threading.Tasks;

namespace TuneCast.MVC.Controllers
{
    public class PagosController : Controller
    {
        // GET: PagosController
        public ActionResult Index()
        {
            var listaPagos = Crud<Pago>.GetAll();
            return View(listaPagos);
        }

        // GET: PagosController/Details/5
        public ActionResult Details(int id)
        {
            var data = Crud<Pago>.GetById(id);
            return View(data);
        }

        // GET: PagosController/Create
        public ActionResult Create()
        {
            // Inicializar con valores por defecto
            var nuevoPago = new Pago
            {
                FechaPago = DateTime.Now
            };

            // Inicializar ViewData para campos de tarjeta
            ViewData["NumeroTarjeta"] = "";
            ViewData["FechaExpiracion"] = "";
            ViewData["CVV"] = "";
            ViewData["MetodoPago"] = "";

            return View(nuevoPago);
        }

        // POST: PagosController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Pago pago, string numeroTarjeta, string fechaExpiracion, string cvv, string metodoPago)
        {
            // Preservar datos del formulario para repoblación
            ViewData["NumeroTarjeta"] = numeroTarjeta;
            ViewData["FechaExpiracion"] = fechaExpiracion;
            ViewData["CVV"] = cvv;
            ViewData["MetodoPago"] = metodoPago;

            // Validación básica del modelo
            if (!ModelState.IsValid)
            {
                return View(pago); // Si el modelo no es válido, vuelve a la vista con los errores
            }

            // Validación de datos de tarjeta
            if (string.IsNullOrEmpty(numeroTarjeta) || numeroTarjeta.Length < 16)
            {
                ModelState.AddModelError("", "El número de tarjeta debe tener al menos 16 dígitos");
                return View(pago);
            }

            if (string.IsNullOrEmpty(fechaExpiracion) || fechaExpiracion.Length != 5 || !fechaExpiracion.Contains("/"))
            {
                ModelState.AddModelError("", "La fecha de expiración debe tener formato MM/AA");
                return View(pago);
            }

            if (string.IsNullOrEmpty(cvv) || cvv.Length != 3)
            {
                ModelState.AddModelError("", "El CVV debe tener exactamente 3 dígitos");
                return View(pago);
            }

            try
            {
                // Simulación de procesamiento de pago
                var random = new Random();
                var pagoAprobado = random.Next(1, 11) <= 8; // 80% de probabilidad

                if (!pagoAprobado)
                {
                    ModelState.AddModelError("", "Pago rechazado: Fondos insuficientes");
                    return View(pago);
                }

                // Completar información del pago
                pago.FechaPago = DateTime.UtcNow;
                pago.MetodoPago = metodoPago;  // Asignar el método de pago

                // Guardar en base de datos
                var pagoGuardado = await Crud<Pago>.Create(pago);

                if (pagoGuardado == null || pagoGuardado.Id == 0)
                {
                    ModelState.AddModelError("", "No se pudo completar el registro del pago");
                    return View(pago);
                }

                // Redirección a confirmación
                return RedirectToAction("PagoExitoso", new
                {
                    monto = pago.Monto,
                    ultimosDigitos = numeroTarjeta.Substring(numeroTarjeta.Length - 4)
                });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error inesperado: {ex.Message}");
                return View(pago);
            }
        }


        // GET: Confirmación de pago exitoso
        public ActionResult PagoExitoso(decimal monto, string ultimosDigitos)
        {
            ViewBag.Monto = monto;
            ViewBag.UltimosDigitos = ultimosDigitos;
            return View();
        }

        // GET: PagosController/Edit/5
        public ActionResult Edit(int id)
        {
            var data = Crud<Pago>.GetById(id);
            if (data == null)
            {
                return NotFound();
            }
            return View(data);
        }

        // POST: PagosController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Pago data)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(data);
                }

                Crud<Pago>.Update(id, data);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al actualizar: {ex.Message}");
                return View(data);
            }
        }

        // GET: PagosController/Delete/5
        public ActionResult Delete(int id)
        {
            var data = Crud<Pago>.GetById(id);
            if (data == null)
            {
                return NotFound();
            }
            return View(data);
        }

        // POST: PagosController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, Pago data)
        {
            try
            {
                Crud<Pago>.Delete(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al eliminar: {ex.Message}");
                return View(data);
            }
        }
    }
}