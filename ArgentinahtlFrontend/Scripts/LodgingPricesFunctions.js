function verTarifasHotel(lodgingId, lodgingName) {
    debugger;
    $("#lodgingId").val(lodgingId);
    $("#lodging-prices-modalLabel").text("Tarifas para el hotel " + lodgingName)
    var values;
    if ($("#checkinDate").val() !== undefined) {
        var date = $("#checkinDate").val().split("-");
        var newDate = date[2] + "/" + date[1] + "/" + date[0];
        values = {
            "LodgingId": lodgingId,
            "dateString": newDate
        }
    } else {
        values = {
            "LodgingId": lodgingId
        };
    }
    
    $.ajax({
        url: '../Login/SearchLodgingWeeklyPrices',
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        cache: false,
        data: JSON.stringify(values),
        beforeSend: function () {
            debugger;
            $("#PricesTable").empty();
            $("#lodging-prices-modal").modal('show');
            $("#spinnerModal").show();
        },
        success: function (data) {
            debugger;
            $("#spinnerModal").fadeOut('fast', function () {
                buildDatesTable(data);
            });
        },
        error: function (xhr) {
            alert("Error");
        }
    });
}

function buildDatesTable(data) {
    var table = $("#PricesTable");
    var body = "";
    var colspan = 0;
    for (var i in data.Units) {
        var unit = data.Units[0];
        colspan = unit.Quota.length + 1;
    }
    debugger;

    body = body + "<thead><tr style='background-color: #00125A; color: white;'><th colspan=" + colspan + " style='text-align:center; border-radius: 6px 6px 0px 0px;'><select class='form-control' style='color: black;' onchange='showTable(this)'><option selected>Selecciona una Habitacion</option>";
    for (var key in data.Units) {
        var unit2 = data.Units[key];
        body = body + "<option value='" + unit2["IdUnidad"] + "'>" + unit2["NombreUnidad"] + "</option>";
    }
    body = body + "</select></th></tr></thead>";
    for (var key1 in data.Units) {
        var unit3 = data.Units[key1];
        body = body + "<tbody id='" + unit3["IdUnidad"] + "' class='tableUnit'><tr><td></td>";
        for (var key3 in unit.Quota) {
            var quota2 = unit.Quota[key3];
            var src = quota2["Fecha"];
            src = src.replace(/[^0-9 +]/g, '');
            var date = new Date(parseInt(src));
            var day = date.getDate();
            var month = date.getMonth() + 1;
            var year = date.getFullYear();
            debugger;
            if (key3 == 0) {
                $("#firstWeekDay").val(year + "/" + month + "/" + day);
            }
            body = body + "<th>" + day + "/" + month + "/" + year + "</th>";
        }
        body = body + "</tr><tr class='rowTarifa tarifaArgentina'><td>Con Desayuno Tarifa Reembolsable</td>";
        for (var key2 in unit.Quota) {
            var quota3 = unit.Quota[key2];
            body = body + "<td>" + quota3["Monto"] + "</td>";
        }
        body = body + "</tr><tr class='rowTarifa tarifaExtrangera'><td>Con Desayuno Tarifa Reembolsable</td>";
        for (var key4 in unit.Quota) {
            var quota4 = unit.Quota[key4];
            body = body + "<td>" + quota4["MontoExtranjeroCDTR"] + "</td>";
        }
        body = body + "</tr><tr class='rowTarifa tarifaMercosur'><td>Con Desayuno Tarifa Reembolsable</td>";
        for (var key5 in unit.Quota) {
            var quota5 = unit.Quota[key5];
            body = body + "<td>" + quota5["MontoMercosurCDTR"] + "</td>";
        }
        body = body + "</tr><tr class='rowTarifa tarifaArgentina'><td>Sin Desayuno Tarifa Reembolsable</td>";
        for (var key6 in unit.Quota) {
            var quota6 = unit.Quota[key6];
            body = body + "<td>" + quota6["MontoArgentinoSDTR"] + "</td>";
        }
        body = body + "</tr><tr class='rowTarifa tarifaExtrangera'><td>Sin Desayuno Tarifa Reembolsable</td>";
        for (var key7 in unit.Quota) {
            var quota7 = unit.Quota[key7];
            body = body + "<td>" + quota7["MontoExtranjeroSDTR"] + "</td>";
        }
        body = body + "</tr><tr class='rowTarifa tarifaMercosur'><td>Sin Desayuno Tarifa Reembolsable</td>";
        for (var key8 in unit.Quota) {
            var quota8 = unit.Quota[key8];
            body = body + "<td>" + quota8["MontoMercosurSDTR"] + "</td>";
        }
        body = body + "</tr><tr class='rowTarifa tarifaArgentina'><td>Con Desayuno Tarifa No Reembolsable</td>";
        for (var key9 in unit.Quota) {
            var quota9 = unit.Quota[key9];
            body = body + "<td>" + quota9["MontoArgentinoCDTNR"] + "</td>";
        }
        body = body + "</tr><tr class='rowTarifa tarifaExtrangera'><td>Con Desayuno Tarifa No Reembolsable</td>";
        for (var key10 in unit.Quota) {
            var quota10 = unit.Quota[key10];
            body = body + "<td>" + quota10["MontoExtrajeroCDTNR"] + "</td>";
        }
        body = body + "</tr><tr class='rowTarifa tarifaMercosur'><td>Con Desayuno Tarifa No Reembolsable</td>";
        for (var key11 in unit.Quota) {
            var quota11 = unit.Quota[key11];
            body = body + "<td>" + quota11["MontoMercosurCDTNR"] + "</td>";
        }
        body = body + "</tr><tr class='rowTarifa tarifaArgentina'><td>Sin Desayuno Tarifa No Reembolsable</td>";
        for (var key12 in unit.Quota) {
            var quota12 = unit.Quota[key12];
            body = body + "<td>" + quota12["MontoArgentinoSDTNR"] + "</td>";
        }
        body = body + "</tr><tr class='rowTarifa tarifaExtrangera'><td>Sin Desayuno Tarifa No Reembolsable</td>";
        for (var key13 in unit.Quota) {
            var quota13 = unit.Quota[key13];
            body = body + "<td>" + quota13["MontoExtranjeroSDTNR"] + "</td>";
        }
        body = body + "</tr><tr class='rowTarifa tarifaMercosur'><td>Sin Desayuno Tarifa No Reembolsable</td>";
        for (var key14 in unit.Quota) {
            var quota14 = unit.Quota[key14];
            body = body + "<td>" + quota14["MontoMercosurSDTNR"] + "</td>";
        }
        body = body + "</tr>";
    }
    table.append(body);
    $(".tableUnit").hide();
    table.fadeIn('fast');
}

function showTable(selected) {
    $(".rowTarifa").hide();
    if ($("#nationalityFilter").val() == 'arg') {
        $(".tarifaArgentina").show();
    } else if ($("#nationalityFilter").val() == 'ext') {
        $(".tarifaExtrangera").show();
    } else if ($("#nationalityFilter").val() == 'mer') {
        $(".tarifaMercosur").show();
    }
    $(".tableUnit").hide();
    $("#" + selected.value).show();
}

function oneMoreWeek() {

    var table = $("#PricesTable");
    table.empty();
    lodgingId = $("#lodgingId").val();
    oldDate = $("#firstWeekDay").val();
    var parts = oldDate.split('/');
    var date = new Date(parts[0], parts[1] - 1, parts[2]);
    date.setDate(date.getDate() + 7);
    var newDate = date.getFullYear() + "/" + (date.getMonth() + 1) + "/" + date.getDate();
    var values = {
        "lodgingId": lodgingId,
        "dateString": newDate
    };
    $.ajax({
        url: '../Login/SearchLodgingWeeklyPrices',
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        cache: false,
        data: JSON.stringify(values),
        success: function (data) {
                buildDatesTable(data);
        },
        error: function (xhr) {
            alert("Error");
        }
    });
}

function oneLessWeek() {
    debugger;
    var table = $("#PricesTable");
    table.empty();
    lodgingId = $("#lodgingId").val();
    oldDate = $("#firstWeekDay").val();
    var parts = oldDate.split('/');
    var date = new Date(parts[0], parts[1] - 1, parts[2]);
    date.setDate(date.getDate() - 7);
    var day = date.getDate();
    var month = date.getMonth() + 1;
    var year = date.getFullYear();
    var newDate = year + "/" + month + "/" + day;
    var values = {
        "lodgingId": lodgingId,
        "dateString": newDate
    };
    $.ajax({
        url: '../Login/SearchLodgingWeeklyPrices',
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        cache: false,
        data: JSON.stringify(values),
        success: function (data) {
                buildDatesTable(data);
        },
        error: function (xhr) {
            alert("Error");
        }
    });
}

function searchSpecificDate() {
    var table = $("#PricesTable");
    table.fadeOut('fast', function () {
        table.empty();
        $("#spinnerModal").fadeIn('fast');
    });
    lodgingId = $("#lodgingId").val();
    newDate = $("#searchDate").val();
    var array = newDate.split('/');
    var newDate2 = array[2] + '/' + array[1] + '/' + array[0];
    var values = {
        "lodgingId": lodgingId,
        "dateString": newDate2
    };
    $.ajax({
        url: '../Login/SearchLodgingWeeklyPrices',
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        cache: false,
        data: JSON.stringify(values),
        success: function (data) {
            $("#spinnerModal").fadeOut('fast', function () {
                buildDatesTable(data);
            });
        },
        error: function (xhr) {
            alert("Error");
        }
    });
}

$('#lodging-prices-modal').on('hidden.bs.modal', function () {
    debugger;
    var table = $("#PricesTable");
    table.empty();
});