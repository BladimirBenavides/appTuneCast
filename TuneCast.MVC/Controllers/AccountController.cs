using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.RegularExpressions;
using TuneCastAPIConsumer;
using TuneCastModelo;

namespace TuneCast.MVC.Controllers
{
    public class AccountController : Controller
    {
        public AccountController()
        {
            // Configura el endpoint de la API para usuarios
            Crud<Usuario>.EndPoint = "https://localhost:7194/api/Usuarios"; // Reemplaza con tu URL real
        }
        [HttpGet]
        public IActionResult RecuperarPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult RecuperarPassword(string email, string palabraClave, string newPassword)
        {
            try
            {
                var usuario = Crud<Usuario>.GetAll().FirstOrDefault(u => u.Email == email);

                if (usuario == null)
                {
                    ViewData["ErrorMessage"] = "Correo no encontrado";
                    ViewData["Email"] = email;
                    ViewData["PalabraClave"] = palabraClave;
                    return View();
                }

                if (usuario.PalabraClaveRecuperacion != palabraClave)
                {
                    ViewData["ErrorMessage"] = "Palabra clave de recuperación incorrecta";
                    ViewData["Email"] = email;
                    ViewData["PalabraClave"] = palabraClave;
                    return View();
                }

                var passwordPattern = new Regex(@"^(?=.*[A-Z])(?=.*[\W]).{8,}$");
                if (!passwordPattern.IsMatch(newPassword))
                {
                    ViewData["ErrorMessage"] = "La nueva contraseña debe comenzar con mayúscula, contener al menos un carácter especial y tener al menos 8 caracteres.";
                    ViewData["Email"] = email;
                    ViewData["PalabraClave"] = palabraClave;
                    return View();
                }

                usuario.Contraseña = newPassword;
                bool success = Crud<Usuario>.Update(usuario.Id, usuario);

                if (success)
                {
                    TempData["SuccessMessage"] = "Contraseña actualizada correctamente";
                    return RedirectToAction("Login");
                }

                ViewData["ErrorMessage"] = "Error al actualizar la contraseña";
                ViewData["Email"] = email;
                ViewData["PalabraClave"] = palabraClave;
                return View();
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = $"Error: {ex.Message}";
                ViewData["Email"] = email;
                ViewData["PalabraClave"] = palabraClave;
                return View();
            }
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string contraseña, bool rememberMe = false, string returnUrl = null)
        {
            try
            {
                var usuarios = Crud<Usuario>.GetAll();
                var usuario = usuarios.FirstOrDefault(u => u.Email == email && u.Contraseña == contraseña);

                if (usuario != null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                        new Claim(ClaimTypes.Name, usuario.Nombre),
                        new Claim(ClaimTypes.Email, usuario.Email),
                        new Claim(ClaimTypes.Role, usuario.Rol ?? "Usuario")
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        new AuthenticationProperties
                        {
                            IsPersistent = rememberMe
                        });

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    return RedirectToAction("Index", "Home");
                }

                ViewData["ErrorMessage"] = "Email o contraseña incorrectos.";
                return View();
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = $"Error al iniciar sesión: {ex.Message}";
                return View();
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string nombre, string email, string contraseña, string palabraClaveRecuperacion)
        {
            try
            {
                var usuarios = Crud<Usuario>.GetAll();
                if (usuarios.Any(u => u.Email == email))
                {
                    ViewData["ErrorMessage"] = "El correo electrónico ya está registrado.";
                    return View();
                }

                // Validar contraseña: al menos una mayúscula, un carácter especial y longitud mínima de 8 caracteres
                var passwordPattern = new Regex(@"^(?=.*[A-Z])(?=.*[\W]).{8,}$");
                if (!passwordPattern.IsMatch(contraseña))
                {
                    ViewData["ErrorMessage"] = "La contraseña debe comenzar con una mayúscula, contener al menos un carácter especial y tener al menos 8 caracteres.";
                    return View();
                }

                var nuevoUsuario = new Usuario
                {
                    Nombre = nombre,
                    Email = email,
                    Contraseña = contraseña,
                    Rol = "Usuario", // Rol por defecto
                    PalabraClaveRecuperacion = palabraClaveRecuperacion
                };

                var usuarioCreado = await Crud<Usuario>.Create(nuevoUsuario);

                if (usuarioCreado?.Id > 0)
                {
                    TempData["SuccessMessage"] = "Registro exitoso. Por favor inicie sesión.";
                    return RedirectToAction("Login");
                }

                ViewData["ErrorMessage"] = "No se pudo crear el usuario. Intente nuevamente.";
                return View();
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = $"Error al registrar: {ex.Message}";
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}