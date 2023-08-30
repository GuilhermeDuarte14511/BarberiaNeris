document.addEventListener('DOMContentLoaded', function () {
    var calendarEl = document.getElementById('calendar');
    calendar = new FullCalendar.Calendar(calendarEl, {
        titleFormat: { // will produce something like "Tuesday, September 18, 2018"
            month: 'short',
            year: '2-digit',
            day: '2-digit',
            weekday: 'short',
        },
        initialView: 'timeGridWeek',
        selectable: true,
        locale: 'pt-br',
        timeZone: 'local',
        headerToolbar: {
            center: 'dayGridMonth,timeGridWeek,timeGridDay',
            container: '#calendarToolbar',
        },

        buttonText: {
            today: 'Hoje',
            month: 'Mês',
            week: 'Semana',
            day: 'Dia',
            list: 'Lista',
            allDay: 'Dia inteiro'

        },

        slotDuration: '01:00:00',
        slotLabelInterval: '01:00:00',
        selectConstraint: {
            start: '00:00',
            end: '24:00',
            dow: [0, 1, 2, 3, 4, 5, 6]
        },
        eventClick: function (info) {
            if (info.event.classNames.includes('disponivel')) {
                let selectedDateTime = info.event.start;
                $('#appointment').val(selectedDateTime.toLocaleString());
                console.log('Data e hora selecionadas: ' + selectedDateTime.toLocaleString());

                // Remova a classe 'selected' de todos os eventos
                calendar.getEvents().forEach(function (event) {
                    event.setProp('classNames', event.classNames.filter(c => c !== 'selected'));
                });

                // Adicione a classe 'selected' ao evento clicado
                info.event.setProp('classNames', [...info.event.classNames, 'selected']);
            }
        }
    });
    calendar.render();
});

$('#barber').change(function () {
    var barbeiroId = $(this).val();

    $.ajax({
        url: '/Agendamento/GetHorariosDisponiveis',
        type: 'GET',
        data: { barbeiroId: barbeiroId },
        success: function (data) {
            // Remova todos os eventos existentes
            calendar.removeAllEvents();

            // Adicione horários disponíveis
            data.disponiveis.forEach(function (horario) {
                calendar.addEvent({
                    title: 'Disponível',
                    start: horario,
                    classNames: ['disponivel']
                });
            });

            // Adicione horários ocupados
            data.ocupados.forEach(function (horario) {
                calendar.addEvent({
                    title: 'Ocupado',
                    start: horario,
                    classNames: ['ocupado'],
                    editable: false // para evitar que o usuário edite/mova o evento
                });
            });
        }
    });
});

$(document).ready(function () {
    if (agendamentoSucesso) {
        $('#confirmationModal').modal('show');
    }
});

$('#carouselBarbeiros').on('slide.bs.carousel', function (event) {
    var activeIndex = $(event.relatedTarget).index();
    $('#carouselBarbeirosText').carousel(activeIndex);
});

function atualizarTotal() {
    let total = 0;
    let checkboxes = document.getElementById('services').querySelectorAll("input[type='checkbox']:checked");
    checkboxes.forEach(checkbox => {
        switch (checkbox.value) {
            case 'cabelo': total += 50; break;
            case 'sobrancelha': total += 20; break;
            case 'barba': total += 30; break;
            case 'bigode': total += 15; break;
        }
    });
    document.getElementById('totalValue').value = "R$" + total;
}

// Agora, chame essa função diretamente quando você passa o valor via parâmetro.
atualizarTotal();

    document.getElementById('services').addEventListener('change', atualizarTotal);
