using Business;
using Entities.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Security.Claims;
using System.Text;

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

    [HttpGet]
    public IActionResult Agendamento(string servico = "")
    {
        var barbeiros = _context.Barbeiros.ToList();
        ViewBag.Barbeiros = barbeiros;

        if (User.Identity.IsAuthenticated)
        {
            var clienteId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(clienteId))
            {
                var cliente = _context.Clientes.Find(int.Parse(clienteId));
                if (cliente != null)
                {
                    ViewBag.ClienteName = cliente.Nome;
                    ViewBag.ClienteEmail = cliente.Email;
                    ViewBag.ClientePhone = cliente.Telefone;
                }
            }
        }

        ViewBag.ServicoSelecionado = servico;

        return View();
    }

    [HttpGet]
    public IActionResult GetHorariosDisponiveis(int barbeiroId)
    {
        var horariosOcupados = GetHorariosOcupados(barbeiroId);

        var horariosDisponiveis = new List<DateTime>();
        var dataInicio = DateTime.Now.Date;

        for (int dia = 0; dia < 14; dia++) // Mostrar horários para os próximos 14 dias
        {
            var data = dataInicio.AddDays(dia);

            for (int hora = 9; hora < 18; hora++) // Horário das 9 às 18
            {
                var horario = data.AddHours(hora);
                if (!horariosOcupados.Contains(horario))
                {
                    horariosDisponiveis.Add(horario);
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
    public IActionResult Agendamento(string name, string email, string phone, List<string> services, int barber, DateTime appointment)
    {       
        _agendamentoBLL.Agendar(name, email, phone, services, barber, appointment);

        // Defina uma variável ViewBag para indicar que o agendamento foi bem-sucedido
        ViewBag.AgendamentoSucesso = true;

        // Enviar e-mail de confirmação
        var teste = SendConfirmationEmail(email, name, barber, services, appointment);
        var barberEmail = _context.Barbeiros.FirstOrDefault(b => b.BarbeiroID == barber)?.Email;
        var barberName = _context.Barbeiros.FirstOrDefault(b => b.BarbeiroID == barber)?.Nome;
        if (!string.IsNullOrEmpty(barberEmail))
        {
            SendEmailToBarber(barberEmail, barberName, name, email, phone, services, appointment);
        }
        var barbeiros = _context.Barbeiros.ToList();
        ViewBag.Barbeiros = barbeiros;

        return View();
    }



    private async Task SendConfirmationEmail(string toEmail, string name, int barberId, List<string> services, DateTime appointment)
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

        var corpoEmail = CorpoEmail(toEmail, name, barberName, services, appointment);

        var plainTextContent = $"Agendamento marcado com sucesso! Nome: {name} Cabeleireiro: {barberName} Serviço: {string.Join(", ", services)} Horário: {appointment}";

        var icsContent = GenerateICalendarEvent("Agendamento na Barbearia", appointment, appointment.AddHours(1));

        var attachment = new Attachment
        {
            Content = Convert.ToBase64String(Encoding.UTF8.GetBytes(icsContent)),
            Filename = "event.ics",
            Type = "text/calendar"
        };

        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, corpoEmail);
        msg.AddAttachment(attachment);

        var response = await client.SendEmailAsync(msg);
    }

    private string GenerateICalendarEvent(string eventName, DateTime startTime, DateTime endTime)
    {
        var icsEvent = new StringBuilder();
        icsEvent.AppendLine("BEGIN:VCALENDAR");
        icsEvent.AppendLine("VERSION:2.0");
        icsEvent.AppendLine("PRODID:-//Barbearia Neris//Agendamento//PT");
        icsEvent.AppendLine("BEGIN:VEVENT");
        icsEvent.AppendLine($"DTSTART:{startTime:yyyyMMddTHHmmss}");
        icsEvent.AppendLine($"DTEND:{endTime:yyyyMMddTHHmmss}");
        icsEvent.AppendLine($"SUMMARY:{eventName}");
        icsEvent.AppendLine("END:VEVENT");
        icsEvent.AppendLine("END:VCALENDAR");
        return icsEvent.ToString();
    }

    private async Task SendConfirmationEmailBarbeiro(string toEmail, string name, int barberId, List<string> services, DateTime appointment)
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


        var corpoEmail = CorpoEmail(toEmail, name, barberName, services, appointment);

        var plainTextContent = $"Agendamento marcado com sucesso! Nome: {name} Cabeleireiro: {barberName} Serviço: {services} Horário: {appointment}";
        var htmlContent = corpoEmail;

        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
        var response = await client.SendEmailAsync(msg);
    }

    private async Task SendEmailToBarber(string barberEmail, string barberName, string clienteName, string clientEmail, string phone, List<string> services, DateTime appointment)
    {
        var apiKey = _configuration["EmailSettings:SendGridApiKey"];
        var client = new SendGridClient(apiKey);

        var from = new EmailAddress("gduarte@sprintmind.com.br", "Barbearia Neris");
        var subject = "Novo Agendamento";
        var to = new EmailAddress(barberEmail);

        var htmlContent = CorpoEmailBarbeiro(barberName, clienteName, clientEmail, phone, services, appointment);
        var plainTextContent = $"O cliente {clienteName} marcou um agendamento para {appointment.ToString("dd/MM/yyyy HH:mm")}. Serviços: {string.Join(", ", services)}";

        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
        var response = await client.SendEmailAsync(msg);
    }

    public string CorpoEmailBarbeiro(string barberName, string clientName, string clientEmail, string clientPhone, List<string> services, DateTime appointment)
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
        html += "<p><b>Olá</b> " + barberName + "," + "</p>";
        html += "<p>Um cliente marcou um agendamento através do nosso sistema.</p>";
        html += "<p>Aqui estão os detalhes:</p>";
        html += "<p>Nome do Cliente: " + clientName + "</p>";
        html += "<p>E-mail do Cliente: " + clientEmail + "</p>";
        html += "<p>Telefone do Cliente: " + clientPhone + "</p>";
        html += "<p>Serviços Agendados: " + string.Join(", ", services) + "</p>";
        html += "<p>Data e Hora Marcada: " + appointment + "</p>";
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


    public string CorpoEmail(string toEmail, string name, string barberId, List<string> services, DateTime appointment)
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
        html += "<p> Serviço: " + string.Join(", ", services) + "</p><br>";
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

        html += $"Olá {name}," +
                 "Seu agendamento foi confirmado para:" +
                 $"Data e Hora: {appointment}" +
                 "Clique no link abaixo para adicionar o evento ao seu calendário:" +
                 "[Adicionar ao Calendário](link_para_download_do_evento.ics)" +
                 "Atenciosamente," +
                 "Barbearia Neris";


        return html;
    }
    public IActionResult Confirmacao()
    {
        return View();
    }
}
