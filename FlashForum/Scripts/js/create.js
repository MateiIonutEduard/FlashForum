$(document).ready(function () {
    $("#filter").css("visibility", "hidden");

    var query = location.href;
    var url = new URL(query);

    var id = url.searchParams.get("id");
    if (id === null) location.href = '/Home/Index';
});

$(document).on("submit", "form", function (e) {
    e.preventDefault();
    var query = location.href;
    var url = new URL(query);

    var id = url.searchParams.get("id");
    if (id === null) location.href = '/Home/Index';
    $('[name=choose]').val(parseInt(id));

    var table = new FormData();
    table.append('id', parseInt(id));
    table.append('subject', $('#name').val());
    table.append('question', $('#body').val());
    table.append('view', $("#view").get(0).files[0]);

    $.ajax({
        url: '/Home/AddTopic',
        type: 'post',
        data: table,
        cache: false,
        contentType: false,
        processData: false,
        success: function (data) {
            if (data.success) history.back();
            else $('.text-danger').css('visibility', 'visible');
        },
        async: true
    });

    setTimeout(function () {
        location.href = `/Home/AllTopics/?id=${parseInt(id)}`;
    }, 1000);
});

function OpenFile() {
    $("#view").click();
}