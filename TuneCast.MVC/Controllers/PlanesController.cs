﻿using System.Numerics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TuneCastAPIConsumer;
using TuneCastModelo;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TuneCast.MVC.Controllers
{
    public class PlanesController : Controller
    {
        private const double IVA_PERCENTAGE = 0.15;
        public PlanesController()
        {
            Crud<Plan>.EndPoint = "https://localhost:7194/api/Planes";
            Crud<Suscripcion>.EndPoint = "https://localhost:7194/api/Suscripciones";
            Crud<Usuario>.EndPoint = "https://localhost:7194/api/Usuarios";
        }

        // GET: PlanesController
        public ActionResult Index()
        {
            var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            try
            {
                // Obtener todas las suscripciones para encontrar la del usuario
                var suscripciones = Crud<Suscripcion>.GetAll();
                var suscripcionActual = suscripciones.FirstOrDefault(s => s.UsuarioId == usuarioId && s.Activa);

                // Obtener todos los planes disponibles
                var planesDisponibles = Crud<Plan>.GetAll();

                ViewBag.SuscripcionActual = suscripcionActual;
                ViewBag.PlanActual = suscripcionActual != null ?
                    planesDisponibles.FirstOrDefault(p => p.Id == suscripcionActual.PlanId) : null;
                ViewBag.UsuarioId = usuarioId;

                return View(planesDisponibles);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error al cargar los datos: {ex.Message}";
                return View(new List<Plan>());
            }
        }

        // GET: PlanesController/Details/5
        public ActionResult Details(int id)
        {
            try
            {
                var plan = Crud<Plan>.GetById(id);
                if (plan == null)
                {
                    TempData["ErrorMessage"] = "El plan solicitado no existe.";
                    return RedirectToAction(nameof(Index));
                }
                return View(plan);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al cargar el plan: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: PlanesController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: PlanesController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Plan plan)
        {
            try
            {
                // Validar datos de entrada
                if (!ModelState.IsValid)
                {
                    return View(plan);
                }

                // Validaciones de negocio
                if (string.IsNullOrWhiteSpace(plan.Nombre))
                {
                    ModelState.AddModelError("Nombre", "El nombre del plan es requerido.");
                    return View(plan);
                }

                if (plan.Precio <= 0)
                {
                    ModelState.AddModelError("Precio", "El precio debe ser mayor a cero.");
                    return View(plan);
                }
                if (plan.Precio > 1000)
                {
                    ModelState.AddModelError("Precio", "El precio no puede ser mayor a $1000.");
                    return View(plan);
                }

                if (string.IsNullOrWhiteSpace(plan.Descripcion))
                {
                    ModelState.AddModelError("Descripcion", "La descripción es requerida.");
                    return View(plan);
                }

                // Verificar si ya existe un plan con el mismo nombre
                var planesExistentes = Crud<Plan>.GetAll();
                if (planesExistentes.Any(p => p.Nombre.Equals(plan.Nombre, StringComparison.OrdinalIgnoreCase)))
                {
                    ModelState.AddModelError("Nombre", "Ya existe un plan con este nombre.");
                    return View(plan);
                }
                // Calcular precio con IVA (el precio ingresado es el precio base)
                var precioBase = plan.Precio;
                plan.Precio = Math.Round(precioBase * (1 + IVA_PERCENTAGE), 2, MidpointRounding.AwayFromZero);

                // Crear el plan
                var planCreado = await Crud<Plan>.Create(plan);

                if (planCreado?.Id > 0)
                {
                    TempData["SuccessMessage"] = $"Plan '{plan.Nombre}' creado exitosamente. Precio base: ${precioBase:F2}, Precio final: ${plan.Precio:F2}";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "No se pudo crear el plan. Intente nuevamente.");
                    plan.Precio = precioBase; // Restaurar precio base para la vista
                    return View(plan);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al crear el plan: {ex.Message}");
                return View(plan);
            }
        }

        // GET: PlanesController/Edit/5
        public ActionResult Edit(int id)
        {
            var data = Crud<Plan>.GetById(id);
            return View(data);
        }

        // POST: PlanesController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Plan data)
        {
            try
            {
                Crud<Plan>.Update(id, data);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(data);
            }
        }

        // GET: PlanesController/Delete/5
        public ActionResult Delete(int id)
        {
            try
            {
                var plan = Crud<Plan>.GetById(id);
                if (plan == null)
                {
                    TempData["ErrorMessage"] = "El plan solicitado no existe.";
                    return RedirectToAction(nameof(Index));
                }
                return View(plan);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al cargar el plan: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }

        }

        // POST: PlanesController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, Plan plan)
        {
            try
            {
                // Verificar que el plan existe antes de eliminarlo
                var planExistente = Crud<Plan>.GetById(id);
                if (planExistente == null)
                {
                    TempData["ErrorMessage"] = "El plan que intenta eliminar ya no existe.";
                    return RedirectToAction(nameof(Index));
                }

                // TODO: Verificar si el plan tiene suscripciones activas
                // En una implementación completa, aquí verificaríamos si hay usuarios suscritos
                // y manejaríamos esa situación apropiadamente

                bool eliminado = Crud<Plan>.Delete(id);

                if (eliminado)
                {
                    TempData["SuccessMessage"] = $"Plan '{planExistente.Nombre}' eliminado exitosamente.";
                }
                else
                {
                    TempData["ErrorMessage"] = "No se pudo eliminar el plan. Puede que tenga suscripciones activas.";
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al eliminar el plan: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
        [HttpPost]
        public async Task<ActionResult> CambiarPlan(int planId)
        {
            var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            try
            {
                // Obtener suscripción actual
                var suscripciones = Crud<Suscripcion>.GetAll();
                var suscripcionActual = suscripciones.FirstOrDefault(s => s.UsuarioId == usuarioId && s.Activa);

                if (suscripcionActual != null)
                {
                    // Desactivar suscripción actual
                    suscripcionActual.Activa = false;
                    suscripcionActual.FechaFin = DateTime.UtcNow;
                    Crud<Suscripcion>.Update(suscripcionActual.Id, suscripcionActual);
                }

                // Crear nueva suscripción
                var nuevaSuscripcion = new Suscripcion
                {
                    UsuarioId = usuarioId,
                    PlanId = planId,
                    FechaInicio = DateTime.UtcNow,
                    FechaFin = DateTime.UtcNow.AddMonths(1), // Suscripción por 1 mes
                    Activa = true
                };

                await Crud<Suscripcion>.Create(nuevaSuscripcion);

                TempData["SuccessMessage"] = "¡Plan cambiado exitosamente!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al cambiar el plan: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        public ActionResult ConfirmarCambio(int planId)
        {
            try
            {
                var plan = Crud<Plan>.GetById(planId);
                return View(plan);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al cargar el plan: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        public ActionResult Historial()
        {
            var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            try
            {
                var suscripciones = Crud<Suscripcion>.GetAll()
                    .Where(s => s.UsuarioId == usuarioId)
                    .OrderByDescending(s => s.FechaInicio)
                    .ToList();

                var planes = Crud<Plan>.GetAll();
                ViewBag.Planes = planes;

                return View(suscripciones);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error al cargar el historial: {ex.Message}";
                return View(new List<Suscripcion>());
            }
        }

        [Authorize] // Cualquier usuario autenticado puede ver planes para suscribirse
        public ActionResult PlanesDisponibles()
        {
            try
            {
                var planes = Crud<Plan>.GetAll();
                return View(planes);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al cargar los planes: {ex.Message}";
                return View(new List<Plan>());
            }
        }

        [HttpGet]
        public JsonResult CalcularPrecio(double precioBase)
        {
            try
            {
                if (precioBase <= 0)
                {
                    return Json(new { success = false, message = "El precio debe ser mayor a cero." });
                }

                if (precioBase > 1000)
                {
                    return Json(new { success = false, message = "El precio no puede ser mayor a $1000." });
                }

                var iva = Math.Round(precioBase * IVA_PERCENTAGE, 2, MidpointRounding.AwayFromZero);
                var precioFinal = Math.Round(precioBase + iva, 2, MidpointRounding.AwayFromZero);

                return Json(new
                {
                    success = true,
                    precioBase = Math.Round(precioBase, 2, MidpointRounding.AwayFromZero),
                    iva = iva,
                    precioFinal = precioFinal,
                    porcentajeIva = IVA_PERCENTAGE * 100
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al calcular precio: {ex.Message}" });
            }
        }
        private double ObtenerPrecioBase(double precioFinal)
        {
            return Math.Round(precioFinal / (1 + IVA_PERCENTAGE), 2, MidpointRounding.AwayFromZero);
        }

        private double CalcularPrecioFinal(double precioBase)
        {
            return Math.Round(precioBase * (1 + IVA_PERCENTAGE), 2, MidpointRounding.AwayFromZero);
        }


        public JsonResult EstadisticasPlanes()
        {
            try
            {
                var planes = Crud<Plan>.GetAll();

                var estadisticas = new
                {
                    totalPlanes = planes.Count,
                    precioPromedio = planes.Any() ? Math.Round(planes.Average(p => p.Precio), 2) : 0,
                    planMasCaro = planes.Any() ? planes.OrderByDescending(p => p.Precio).First().Nombre : "N/A",
                    planMasBarato = planes.Any() ? planes.OrderBy(p => p.Precio).First().Nombre : "N/A",
                    ingresosPotenciales = Math.Round(planes.Sum(p => p.Precio), 2)
                };

                return Json(new { success = true, data = estadisticas });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}

