$(document).on("click", "#admin", function () {
    $("#check").click();
});

$(document).on("click", ".fa-file-image-o", function () {
    $("#upload").click();
});

$(document).on("change", "#upload", function () {
    var fileName = $("#upload").val().split("\\").pop();
    $("#model").text(fileName);
});

$(document).ready(function () {
    $(".form-signup").submit(function (e) {
        e.preventDefault();
        $(".text-danger").css('visibility', 'hidden');
        var pass = $("#password").val();
        var conf = $("#confirm").val();

        if (pass !== conf)
            $(".text-danger").css('visibility', 'visible');
        else {
            var fileUpload = $("#upload").get(0);
            var files = fileUpload.files;
            var data = new FormData();

            var data = new FormData();
            data.append("username", $("#name").val());
            data.append("password", $("#password").val());
            data.append("email", $("#email").val());
            data.append("level", false);
            data.append("file", $("#upload").get(0).files[0]);

            $.ajax({
                url: '/Account/Signup',
                data: data,
                cache: false,
                contentType: false,
                processData: false,
                method: 'POST',
                async: true,
                success: function (data) {
                    if (data.success) location.href = "/Home/Index";
                    else location.href = "/Account/Login";
                }
            });
        }
    });
});