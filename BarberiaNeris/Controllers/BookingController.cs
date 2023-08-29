using Google.Apis.Calendar.v3;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class BookingController : Controller
{
    private readonly CalendarService _calendarService;

    public BookingController(CalendarService calendarService)
    {
        _calendarService = calendarService;
    }

    [HttpGet("availableSlots/{barberName}")]
    public IActionResult GetAvailableSlots(string barberName)
    {
        string calendarId = barberName + "@sprintmind.com.br"; // Apenas um exemplo

        var request = _calendarService.Events.List(calendarId);
        request.TimeMin = DateTime.Now;
        request.TimeMax = DateTime.Now.AddDays(7);
        var events = request.Execute();

        var availableSlots = new List<DateTime>();

        for (var date = DateTime.Now; date < DateTime.Now.AddDays(7); date = date.AddHours(1))
        {
            // Verifica se é entre 8h e 18h e se é um dia da semana (não sábado ou domingo)
            if (date.Hour >= 8 && date.Hour < 18 && date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
            {
                if (!events.Items.Any(e => e.Start.DateTime.HasValue && DateTime.Parse(e.Start.DateTime.Value.ToString()).TimeOfDay == date.TimeOfDay))
                {
                    availableSlots.Add(date);
                }

            }
        }

        return Ok(availableSlots);
    }


}
