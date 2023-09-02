document.addEventListener('DOMContentLoaded', function () {

    // Calendário
    var calendarEl = document.getElementById('calendar');
    if (calendarEl) {
        var calendar = new FullCalendar.Calendar(calendarEl, {
            titleFormat: {
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
    }

    function zeroHora(data) {
        return new Date(data.setHours(0, 0, 0, 0));
    }

    // Dropdown 'barber'
    if ($('#barber').length) {
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
                        var horarioDate = zeroHora(new Date(horario));
                        var currentDate = zeroHora(new Date());

                        // Se o horário for de um dia anterior à data atual, ignore-o.
                        if (horarioDate < currentDate) {
                            return;
                        }

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
    }

    // Modal de confirmação
    if (typeof agendamentoSucesso !== 'undefined' && agendamentoSucesso) {
        $('#confirmationModal').modal('show');
    }

    // Carrossel
    if ($('#carouselBarbeiros').length) {
        $('#carouselBarbeiros').on('slide.bs.carousel', function (event) {
            var activeIndex = $(event.relatedTarget).index();
            $('#carouselBarbeirosText').carousel(activeIndex);
        });
    }

    // Atualização do valor total
    if (document.getElementById('services')) {
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

        // Chame essa função diretamente quando você passar o valor via parâmetro.
        atualizarTotal();
        document.getElementById('services').addEventListener('change', atualizarTotal);
    }
    // Para o modal de LOGIN
    const togglePasswordLogin = document.querySelector("#togglePassword");
    const passwordFieldLogin = document.querySelector("#senha");

    togglePasswordLogin.addEventListener("click", function () {
        if (passwordFieldLogin.type === "password") {
            passwordFieldLogin.type = "text";
            togglePasswordLogin.innerHTML = '<i class="fa fa-eye-slash"></i>';
        } else {
            passwordFieldLogin.type = "password";
            togglePasswordLogin.innerHTML = '<i class="fa fa-eye"></i>';
        }
    });

    // Para o modal de CADASTRO
    const togglePasswordCadastro = document.querySelector("#toggleCadastroPassword");
    const passwordFieldCadastro = document.querySelector("#cadastroSenha");

    togglePasswordCadastro.addEventListener("click", function () {
        if (passwordFieldCadastro.type === "password") {
            passwordFieldCadastro.type = "text";
            togglePasswordCadastro.innerHTML = '<i class="fa fa-eye-slash"></i>';
        } else {
            passwordFieldCadastro.type = "password";
            togglePasswordCadastro.innerHTML = '<i class="fa fa-eye"></i>';
        }
    });

    $(document).ready(function () {
        var successMessage = $('#successMessage').val();
        if (successMessage) {
            $('#successModal').modal('show');
        }
    });

    $(document).ready(function () {
        var showUpdateModal = $('#showUpdateModal').val();
        if (showUpdateModal) { // TempData armazena valores booleanos como 'True' ou 'False' em strings
            $('#updateCadastroModal').modal('show');
        }
    });

    $.ajax({
        type: "GET",
        url: "/Cliente/GetHistoricoAgendamento",
        success: function (response) {
            if (response != null) {
                var table = $('#agendamentosTable').dataTable({
                    destroy: true,
                    "aaData": response,
                    stripeClasses: [],
                    responsive: true,
                    "language": {
                        "lengthMenu": "_MENU_ Registros por páginas",
                        "search": "Busca:",
                        "info": "Exibindo _START_ a _END_ de _TOTAL_ registros",
                        "infoEmpty": "Não há registros para mostrar",
                        "infoFiltered": "(Filtrados de _MAX_ registros)",
                        "emptyTable": "Nenhum registro encontrado!",
                        "paginate": {
                            "next": "Próxima",
                            "previous": "Anterior",
                            "first": "Primeiro",
                            "last": "Último"
                        }
                    },
                    "lengthMenu": [20, 30, 50], // Opções de quantidade de resultados por página
                    "pageLength": 20, // Quantidade de resultados por página inicial (opcional)
                    "columns": [
                        { "data": "agendamentoID" },
                        {
                            "data": "dataHora",
                            "render": function (data) {
                                return new Date(data).toLocaleString(); // Formata a data e hora
                            }
                        },
                        { "data": "servico" },
                        {
                            "data": "valorTotal",
                            "render": function (data, type, row) {
                                return $.fn.dataTable.render.number(',', '.', 2, 'R$').display(data);
                            }
                        },
                        { "data": "nomeBarbeiro" },
                        { "data": "nomeCliente" }
                    ],
                });
            }
        }
    });

});