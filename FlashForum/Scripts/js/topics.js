function CloseTopic(id) {
    $.ajax({
        url: '/Home/CloseTopic',
        type: 'put',
        data: {
            'id': parseInt(id)
        },
        cache: false,
        success: function (data) {
            setTimeout(location.reload(true), 1000);
        },
        async: true
    });
}
function RemoveTopic(id) {
    // show popup first...
    var modal = $("#myModal");
    modal.css("display", "block");
    var close = $(".close");

    $("#del").on("click", function () {
        $.ajax({
            url: '/Home/RemoveTopic',
            type: 'delete',
            data: {
                'id': parseInt(id)
            },
            cache: false,
            success: function (data) {
                if (!data.success) alert(data.message);
                else location.reload(500, true);
            },
            async: true
        });
    });

    $("#can").on("click", function () {
        close.click();
    });

    close.on("click", function () {
        modal.css("display", "none");
    });
}

$(document).ready(function () {
    var query = location.href;
    var url = new URL(query);
    var id = url.searchParams.get("id");

    var filter = url.searchParams.get("filter");
    if (id === null) location.href = '/Home/Index';

    $("#filter").append(`<option value="/Home/AllTopics/?id=${id}&filter=1">All Threads</option>`);
    $("#filter").append(`<option value="/Home/AllTopics/?id=${id}&filter=2">Most Posts...</option>`);
    $("#filter").append(`<option value="/Home/AllTopics/?id=${id}&filter=3">Active Topics</option>`);

    if (filter === null || filter === 1)
        $('#filter').val(`/Home/AllTopics/?id=${id}&filter=1`);
    else
        $('#filter').val(`/Home/AllTopics/?id=${id}&filter=${filter}`);
});

$(document).on("change", "#filter", function () {
    var filter = $("#filter").val();
    location.href = filter;
});