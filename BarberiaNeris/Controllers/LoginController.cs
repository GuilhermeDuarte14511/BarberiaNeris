using Business;
using Entities.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Collections.Generic;
using System.Security.Claims;

namespace BarberiaNeris.Controllers
{
    public class LoginController : Controller
    {
        private readonly BarbeariaContext _context;

        public LoginController(BarbeariaContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string senha)
        {
            var cliente = _context.Clientes.FirstOrDefault(b => b.Email == email);

            if (cliente != null && BaseBLL.VerifyPassword(senha, cliente.Senha))
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, cliente.Nome),
                    new Claim(ClaimTypes.NameIdentifier, cliente.ClienteID.ToString()),
                    new Claim("Email", cliente.Email.ToString()),
                    new Claim("Telefone", cliente.Telefone.ToString()),
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                // Defina a propriedade IsPersistent para true
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = false
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                // Redirecione para a página desejada após o login.
                return RedirectToAction("HistoricoAgendamentos", "Cliente");
            }
            else
            {
                // Informe o usuário que o email ou a senha estão incorretos.
                ModelState.AddModelError(string.Empty, "Email ou senha inválidos.");
                return View();  // Retorna para a página de login com a mensagem de erro.
            }
        }


        [HttpPost]
        public IActionResult CadastrarUsuario(string nome, string email, string telefone, string senha)
        {
            // Verificar se já existe um usuário com o mesmo e-mail
            var clienteExistente = _context.Clientes.FirstOrDefault(b => b.Email == email);

            
            if (clienteExistente == null)
            {
                var senhaCriptografada = BaseBLL.HashPassword(senha);

                var novoCliente = new Cliente
                {
                    Nome = nome,
                    Email = email,
                    Telefone = telefone,
                    Senha = senhaCriptografada,
                };

                _context.Clientes.Add(novoCliente);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Cadastro feito com sucesso!";
                ViewBag.CadastroConcluido = true;
                return RedirectToAction("Index", "Home"); // Redirecione para a página inicial após o cadastro
            }
            else if(clienteExistente != null && clienteExistente.Senha == null)
            {
                // Atualize os campos do cliente existente
                clienteExistente.Nome = nome;
                clienteExistente.Email = email;
                clienteExistente.Telefone = telefone;
                clienteExistente.Senha = BaseBLL.HashPassword(senha);
                // Persiste a atualização
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Cadastro feito com sucesso!";
                ViewBag.CadastroConcluido = true;
                return RedirectToAction("Index", "Home"); // Redirecione para a página inicial após o cadastro
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Email já está em uso.");
                return View(); // Retorna para a página de registro com a mensagem de erro.
            }
        }

        [HttpPost]
        public IActionResult UpdateCadastro(int clienteId, string nome, string email, string telefone, string senha)
        {
            var clienteExistente = _context.Clientes.FirstOrDefault(c => c.Email == email);

            if (clienteExistente == null)
            {
                // Cliente não encontrado, trate o erro
                ModelState.AddModelError(string.Empty, "Cliente não encontrado.");
                return View(); // Retorna para a página de edição com a mensagem de erro.
            }

            // Atualize os campos do cliente existente
            clienteExistente.Nome = nome;
            clienteExistente.Email = email;
            clienteExistente.Telefone = telefone;
            clienteExistente.Senha = BaseBLL.HashPassword(senha);

            // Persiste a atualização
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Cadastro atualizado com sucesso!";
            return RedirectToAction("Index", "Home");
        }


        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}
