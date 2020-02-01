function Event() {
    this.init();
}

Event.prototype = new BaseObject();

Event.prototype.constructor = Event;

Event.prototype.init = function () {
    $("#EventDate").datepicker();
};

