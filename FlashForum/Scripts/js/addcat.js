$(document).ready(function (e) {
    $('[data-toggle="tooltip"]').tooltip();
    $("#data-filter").css("visibility", "hidden");

    $("form").submit(function (e) {
        e.preventDefault();

        $.ajax({
            url: '/Home/AddCategory',
            type: 'post',
            cache: false,
            data: {
                'name': $("#name").val(),
                'content': $("#content").val()
            },
            success: function (data) {
                var success = data.success;

                if (!success)
                    $(".text-danger").css("visibility", "visible");
                else {
                    location.href = '/Home/Index';
                    $("#data-filter").css("visibility", "visible");
                }
            },
            async: true
        });
    });
});