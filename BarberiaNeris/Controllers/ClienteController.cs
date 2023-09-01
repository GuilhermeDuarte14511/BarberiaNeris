using Business;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BarberiaNeris.Controllers
{
    [Authorize] // Proteja todo o controlador
    public class ClienteController : Controller
    {
        private readonly HistoricoAgendamentosBLL _historicoAgendamentosBLL;

        public ClienteController(HistoricoAgendamentosBLL historicoAgendamentosBLL)
        {
            _historicoAgendamentosBLL = historicoAgendamentosBLL;
        }

        public IActionResult HistoricoAgendamentos()
        {
            return View();
        }

        public IActionResult GetHistoricoAgendamento()
        {
            int clienteId = int.Parse(HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));

            var agendamentos = _historicoAgendamentosBLL.ObterInformacoesAgendamentosPorCliente(clienteId);

            return Json(agendamentos);
        }
    }
}
