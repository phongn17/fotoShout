function EventWebsites() {
    this.init();
}

EventWebsites.prototype = new BaseObject();

EventWebsites.prototype.constructor = EventWebsites;

EventWebsites.prototype.init = function () {
    this.confirm('.linkDelete', '#deleteForm');
    this.hoverList("tr[class='row-item']");
};

