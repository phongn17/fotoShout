function EventSponsors() {
    this.init();
}

EventSponsors.prototype = new BaseObject();

EventSponsors.prototype.constructor = EventSponsors;

EventSponsors.prototype.init = function () {
    this.confirm('.linkDelete', '#deleteForm');
    this.hoverList("tr[class='row-item']");
};

