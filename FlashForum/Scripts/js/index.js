$(document).ready(function () {
    $("#filter").append(`<option value="/Home/Index/?filter=1">All Categories</option>`);
    $("#filter").append(`<option value="/Home/Index/?filter=2">Best activity...</option>`);
    $("#filter").append(`<option value="/Home/Index/?filter=3">Last created</option>`);

    var query = location.href;
    var url = new URL(query);
    var id = url.searchParams.get("filter");

    if (id === null || id === 1)
        $('#filter').val('/Home/Index/?filter=1');
    else
        $('#filter').val(`/Home/Index/?filter=${id}`);
});

$(document).on("change", "#filter", function () {
    var filter = $("#filter").val();
    location.href = filter;
});

function RemoveAll(id) {
    var modal = $("#myModal");
    modal.css("display", "block");
    var close = $(".close");

    $("#del").on("click", function () {
        $.ajax({
            url: '/Home/RemoveCategory/',
            type: 'delete',
            cache: false,
            data: {
                'id': parseInt(id)
            },
            async: true
        });

        setTimeout(function () {
            location.reload(true);
        }, 1000);
    });

    $("#can").on("click", function () {
        close.click();
    });

    close.on("click", function () {
        modal.css("display", "none");
    });
}