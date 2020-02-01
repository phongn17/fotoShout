function Initializer(tabCtrl, controller) {
    this.init(tabCtrl, controller);
}

Initializer.prototype.init = function (tabCtrl, controller) {
    var activeTab = (controller === "event") ? 0 : 1;
    $("#tabs-mainmenu").tabs({ active: activeTab });

    var tabLinks = $("a[id^='tabLink-']");
    if (tabLinks !== undefined) {
        tabLinks.click(function () {
            var id = $(this).attr('id');
            var pair = id.split('-');
            if (pair.length === 2) {
                var actionName = pair[1];
                tabCtrl.getContentTab(actionName);
            }
        });
    }
};

String.prototype.endsWith = function (suffix) {
    if (this.length < suffix.length)
        return false;

    return (this.indexOf(suffix, this.length - suffix.length) !== -1);
};