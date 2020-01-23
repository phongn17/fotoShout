function EventOptions() {
    this.init();
}

EventOptions.prototype = new BaseObject();

EventOptions.prototype.constructor = EventOptions;

EventOptions.prototype.init = function () {
    this.confirm('.linkDelete', '#deleteForm');
    this.hoverList("tr[class='row-item']");
}


