function EmailTemplates() {
    this.init();
}

EmailTemplates.prototype = new BaseObject();

EmailTemplates.prototype.constructor = EmailTemplates;

EmailTemplates.prototype.init = function () {
    this.loadTemplate();
    this.confirm('.linkDelete', '#deleteForm');
    this.hoverList("tr[class='row-item']");
};

EmailTemplates.prototype.loadTemplate = function () {
    var emailContent = $('#EmailContent');
    if (emailContent.length === 1 && emailContent[0].innerHTML=== "") {
        var idx = document.URL.lastIndexOf('/');
        var url = document.URL.substr(0, idx + 1) + "LoadTemplate";
        $(emailContent[0])
            .load(url, function (response, status, jqXHR) {
            });
    }
};

