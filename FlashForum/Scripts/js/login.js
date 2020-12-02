$(document).ready(function () {
    $(".form-signin").submit(function (e) {
        e.preventDefault();
        var form = {
            "email": $("#email").val(),
            "password": $("#password").val()
        };

        $.ajax({
            url: '/Account/Login',
            type: 'post',
            data: form,
            dataType: 'json',
            cache: false,
            success: function (data) {
                if (data.success)
                    location.href = "/Home/Index";
                else {
                    $(".text-danger").css("visibility", "visible");
                }
            },
            async: true
        });
    });
});