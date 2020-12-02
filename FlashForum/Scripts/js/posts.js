$(document).ready(function () {
    $("#filter").css("visibility", "hidden");
    var query = location.href;
    var url = new URL(query);

    var id = url.searchParams.get("id");
    if (id === null) location.href = '/Home/Index';
});

function OpenFile() {
    $('#apply').click();
}

function SendPost() {
    var buffer = new FormData();
    var query = location.href;
    var url = new URL(query);
    var id = url.searchParams.get("id");

    if (id !== null) {
        buffer.append('topic', parseInt(id));
        buffer.append('message', $('#post').val());
        buffer.append('file', $('#apply').get(0).files[0]);

        if ($('#apply').get(0).files.length) {
            $.ajax({
                url: '/Home/SendPost',
                type: 'post',
                contentType: false,
                cache: false,
                processData: false,
                data: buffer,
                xhr: function () {
                    var xhr = new window.XMLHttpRequest();
                    xhr.upload.addEventListener('progress', function (e) {
                        if (e.lengthComputable) {
                            var percent = parseInt((e.loaded / e.total) * 100);
                            $('.progress-bar').width(`${percent}%`);
                        }
                    }, false);

                    return xhr;
                },
                beforeSend: function () {
                    $('.container-sm').css('display', 'block');
                    $('.progress-bar').width("0%");
                },
                success: function (data) {
                    $('.progress-bar').width('0%');
                    $('.container-sm').css('display', 'none');
                }
            });
        } else {
            $.ajax({
                url: '/Home/SendPost',
                type: 'post',
                contentType: false,
                cache: false,
                processData: false,
                data: buffer
            });
        }

        setTimeout(function () {
            location.reload(true);
        }, 1000);
    }
}

function ScrollDown() {
    $(document).scrollTop($(document).height());
    $('#post').focus();
}

function RemovePost(id) {
    var modal = $("#myModal");
    modal.css("display", "block");
    var close = $(".close");

    $("#del").on("click", function () {
        $.ajax({
            url: '/Home/RemovePost',
            type: 'delete',
            cache: false,
            data: {
                'id': parseInt(id)
            }
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