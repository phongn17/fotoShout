function Events(url) {
    this.init(url);
    updateCss();
}

Events.prototype = new BaseObject();

Events.prototype.constructor = Events;

Events.prototype.init = function (url) {
    this.checkConstrain();
    this.confirm('.linkDelete', '#deleteForm');
    this.confirm('.linkComplete', '#completeForm');
    this.hoverFooter("div[class='thumb']");
    $("#EventType").change(function (e) {
        e.preventDefault();
        var evType = $(this).val();
        $("#main-panel")
            .html("")
            .load(url + "?eventType=" + evType, function (response, status, jqXHR) {
                if (jqXHR.status === 200) {
                    updateCss();
                }
            });
    });
};

function updateCss() {
    var thumbActionsDivs = $('.thumb-date > .links');
    if (thumbActionsDivs.length > 0) {
        for (var idx = 0; idx < thumbActionsDivs.length; idx++) {
            var thumbActionsDiv = thumbActionsDivs[idx];
            var linkEdit = $('.linkEdit', thumbActionsDiv);
            if (linkEdit.length === 0) {
                $(thumbActionsDiv).css('padding-left', '100px');
            }
            else {
                $(thumbActionsDiv).css('padding-left', '68px');
            }
        }
    }
}