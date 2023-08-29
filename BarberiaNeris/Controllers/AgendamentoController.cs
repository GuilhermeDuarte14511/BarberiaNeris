using Business;
using Entities.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SendGrid;
using SendGrid.Helpers.Mail;

public class AgendamentoController : Controller
{
    private readonly AgendamentoBLL _agendamentoBLL;
    private readonly BarbeariaContext _context;
    private readonly IConfiguration _configuration;

    public AgendamentoController(AgendamentoBLL agendamentoBusiness, BarbeariaContext context, IConfiguration configuration)
    {
        _agendamentoBLL = agendamentoBusiness;
        _context = context;
        _configuration = configuration;
    }

    public IActionResult Agendamento()
    {
        var barbeiros = _context.Barbeiros.ToList();
        ViewBag.Barbeiros = barbeiros;
        return View();
    }

    [HttpGet]
    public IActionResult GetHorariosDisponiveis(int barbeiroId)
    {
        var horariosOcupados = GetHorariosOcupados(barbeiroId);

        var horariosDisponiveis = new List<DateTime>();
        for (int i = 0; i < 7; i++)
        {
            var data = DateTime.Now.Date.AddDays(i);
            if (data.DayOfWeek != DayOfWeek.Sunday && data.DayOfWeek != DayOfWeek.Saturday)
            {
                for (int hora = 8; hora <= 18; hora++)
                {
                    var horario = data.AddHours(hora);
                    if (!horariosOcupados.Contains(horario))
                    {
                        horariosDisponiveis.Add(horario);
                    }
                }
            }
        }

        return Json(new
        {
            disponiveis = horariosDisponiveis,
            ocupados = horariosOcupados
        });
    }


    public List<DateTime> GetHorariosOcupados(int barbeiroId)
    {
        return _context.Agendamentos
                       .Where(a => a.BarbeiroID == barbeiroId)
                       .Select(a => a.DataHora)
                       .ToList();
    }



    [HttpPost]
    public IActionResult Agendamento(string name, string email, string phone, string service, int barber, DateTime appointment)
    {
        _agendamentoBLL.Agendar(name, email, phone, service, barber, appointment);

        // Defina uma variável ViewBag para indicar que o agendamento foi bem-sucedido
        ViewBag.AgendamentoSucesso = true;

        // Enviar e-mail de confirmação
        var teste = SendConfirmationEmail(email, name, barber, service, appointment);

        var barbeiros = _context.Barbeiros.ToList();
        ViewBag.Barbeiros = barbeiros;

        return View();
    }


    private async Task SendConfirmationEmail(string toEmail, string name, int barberId, string service, DateTime appointment)
    {
        var apiKey = _configuration["EmailSettings:SendGridApiKey"];
        var client = new SendGridClient(apiKey);

        // Recuperar o nome do barbeiro com base no barberId
        var barber = _context.Barbeiros.FirstOrDefault(b => b.BarbeiroID == barberId);

        if (barber == null)
        {
            
            return;
        }

        var from = new EmailAddress("gduarte@sprintmind.com.br", "Barbearia Neris");
        var subject = "Agendamento Confirmado";
        var to = new EmailAddress(toEmail);

        // Usar o nome do barbeiro recuperado
        var barberName = barber.Nome;


        var corpoEmail = CorpoEmail(toEmail, name, barberName, service, appointment);

        var plainTextContent = $"Agendamento marcado com sucesso! Nome: {name} Cabeleireiro: {barberName} Serviço: {service} Horário: {appointment}";
        var htmlContent = corpoEmail;

        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
        var response = await client.SendEmailAsync(msg);
    }


    public string CorpoEmail(string toEmail, string name, string barberId, string service, DateTime appointment)
    {

        #region[CorpoHTML]
        string html = "<body style='width:100%; height: auto; background-color: #f6f6f6; margin: 0;'>";
        html += "<div style='width:100%; height:5px; background-color:#618D63;'></div>";
        html += "<table width='650' align='center' cellpadding='0' cellspacing='' style=' padding:10px 20px;'>";
        html += "<tr>";
        html += "<td style='text-align:center'>";
        html += "<img src='https://i.imgur.com/bTX9RuM.jpg' /> ";
        html += "</td>";
        html += "</tr>";
        html += "</table>";
        html += "<div style='display:block; border-radius: 8px; width: 650px; margin: 0 auto; box-shadow:0 4px 10px 0 rgba(0,0,0,0.2),0 4px 20px 0 rgba(0,0,0,0.19)'>";
        html += "<table cellpadding='0' cellspacing='0' style='width:100%;background-color:#fff;font-family: Helvetica,Arial,sans-serif; font-size: 14px; font-weight: 300; color: #818181; padding:10px 20px; border-radius: 0 0 8px 8px;'>";
        html += "<tr>";
        html += "<td class='content-wrap'>";
        html += "<table cellpadding='0' cellspacing='0' style='width: 100%;'>";
        html += "<td style='text-align:left'>";
        html += "<p><b>Olá</b> " + name + "," + "</p>";
        html += "<p>Agradecemos a preferência para cuidar do seu estilo e da sua beleza!.</p>";
        html += "<p>Segue informações sobre seu agendamento conosco.</p>";
        html += "<p> Barbeiro: " +  barberId  + "</p><br>";
        html += "<p> Serviço: " + service + "</p><br>";
        html += "<p> Data e hora marcada: " + appointment + "</p><br>";
        html += "</td>";
        html += "</tr>";
        html += "<tr>";
        html += "<td style='text-align:center'>";
        html += "&nbsp;";
        html += "</td>";
        html += "<td style='text-align:center'>";
        html += "&nbsp;>";
        html += "</td>";
        html += "</tr>";
        html += "<tr>";
        html += "<td style='text-align:center'>";
        html += "Atenciosamente, Barbearia Neris.";
        html += "</td>";
        html += "</tr>";
        html += "<td style='text-align:center'>";
        html += "&nbsp;";
        html += "</td>";
        html += "<td style='text-align:center'>";
        html += "&nbsp;";
        html += "</td>";
        html += "</tr>";
        html += "<tr>";
        html += "<td style='text-align:center; font-size: 14px;'>";
        html += "(E-mail enviado automaticamente)";
        html += "</td>";
        html += "</tr>";
        html += "<tr>";
        html += "<td style='text-align:center'>";
        html += "&nbsp;";
        html += "</td>";
        html += "<td style='text-align:center'>";
        html += "&nbsp;";
        html += "</td>";
        html += "</tr>";
        html += "</table>";
        html += "</td>";
        html += "</tr>";
        html += "</table>";
        html += "</div>";
        html += "<table width='650' align='center' cellpadding='0' cellspacing='' style='font-family: Helvetica,Arial,sans-serif; font-weight: 300; color: #818181; padding:10px 20px;'>";
        html += "<tr>";
        html += "<td style='text-align:center'>";
        html += "© <b>Barbearia Neres</b> - Todos os direitos reservados.";
        html += "</td>";
        html += "</tr>";
        html += "<tr>";
        html += "<td style='text-align:center'>";
        html += "&nbsp;";
        html += "</td>";
        html += "</tr>";
        html += "</table>";
        html += "</table>";
        html += "</body>";
        #endregion

        return html;
    }



    public IActionResult Confirmacao()
    {
        return View();
    }
}
