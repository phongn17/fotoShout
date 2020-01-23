function EventPhoto(id, url) {
    this.init(id, url);
    adGallery();
}

EventPhoto.prototype = new BaseObject();

EventPhoto.prototype.constructor = EventPhoto;

EventPhoto.prototype.init = function (id, url) {
    $("#ViewType").live("change", function (e) {
        e.preventDefault();
        var viewType = escape($(this).val());
        $("#view")
        .html("")
        .load(url + "?id=" + id + "&viewType=" + viewType, function (response, status, jqXHR) {
            if (jqXHR.status == 200) {
                adGallery();
            }
        });
    });

    $(".linkPGDetails").live("click", function (e) {
        e.preventDefault();
        $("#view")
        .html("")
        .load(this.href, function (response, status, jqXHR) {
            if (jqXHR.status == 200) {
            }
        });
    });

    $(".event-report").live("click", function (e) {
        e.preventDefault();
        var report = $('#event-report');
        if (report != undefined) {
            var divDlg = document.createElement("div");
            $(report).append(divDlg);
            $(divDlg).html("")
            .load(this.href, function (response, status, jqXHR) {
                if (jqXHR.status == 200) {
                    messageDialog(divDlg);
                }
            });
        }
    });

    $("#view").tooltip({ tooltipClass: "error-publishing" });
}

function adGallery() {
    var gallaries = $(".ad-gallery");
    if (gallaries != undefined)
        $(gallaries).adGallery({
            slideshow: {
                enable: false
            }
        });
}
