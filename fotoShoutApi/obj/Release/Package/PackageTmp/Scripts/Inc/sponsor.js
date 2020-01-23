// Sponsor View Model

function SponsorViewModel(baseUri) {
    var self = this;
    self.user = ko.observable(null);
    self.sponsors = ko.observableArray();
    self.error = ko.observable(null);
    self.msg = ko.observable(null);

    $.ajax({
        type: "GET",
        url: "/api/fs1/Authorization",
        dataType: "json",
        beforeSend: function (jqXHR) {
            addAuthorizationKey(jqXHR);
        }
    })
    .done(function (data) {
        self.user(data);
        $(".username-link").append("<img class='userimage' src='/Images/User.jpg' />").append("<span class='username'>" + data.AccountName + ' > ' + data.FirstName + ' ' + data.LastName + "</span>");
    })
    .fail(function (jqXHR, textStatus) {
        setError(self, jqXHR, textStatus);
    });

    $.ajax({
        type: "GET",
        url: baseUri,
        dataType: "json",
        beforeSend: function (jqXHR) {
            addAuthorizationKey(jqXHR);
        }
    })
    .done(function (data) {
        self.sponsors(data);
    })
    .fail(function (jqXHR, textStatus) {
        setError(self, jqXHR, textStatus);
    });

    self.create = function (formElm) {
        $(formElm).validate();
        if ($(formElm).valid()) {
            var sponsor = ko.toJSON($(formElm).serializeArray());
            $.ajax({
                type: "POST",
                url: baseUri,
                data: $(formElm).serialize(),
                dataType: "json",
                beforeSend: function (jqXHR) {
                    addAuthorizationKey(jqXHR);
                }
            })
            .done(function (sponsor) {
                self.sponsors.push(sponsor);
                self.msg('Added the ' + sponsor.SponsorName + ' sponsor.');
                self.error(null);
                $.ajax({
                    type: "GET",
                    url: baseUri,
                    dataType: "json",
                    beforeSend: function (jqXHR) {
                        addAuthorizationKey(jqXHR);
                    }
                })
                .done(function (data) {
                    self.sponsors(data);
                })
                .fail(function (jqXHR, textStatus) {
                    setError(self, jqXHR, textStatus);
                });
            })
            .fail(function (jqXHR, textStatus) {
                setError(self, jqXHR, textStatus);
            });
        }
    }

    self.update = function (sponsor) {
        $.ajax({
            type: "PUT",
            url: baseUri + '/' + sponsor.SponsorId,
            data: sponsor,
            beforeSend: function (jqXHR) {
                addAuthorizationKey(jqXHR);
            }
        })
        .done(function () {
            setMessage(self, 'Updated the ' + sponsor.SponsorName + ' sponsor.');
        })
        .fail(function (jqXHR, textStatus) {
            setError(self, jqXHR, textStatus);
        });
    }

    self.delete = function (sponsor) {
        $.ajax({
            type: "DELETE",
            url: baseUri + '/' + sponsor.SponsorId,
            beforeSend: function (jqXHR) {
                addAuthorizationKey(jqXHR);
            }
        })
        .done(function (sponsor) {
            self.sponsors.remove(sponsor);
            setMessage(self, 'Deleted the ' + sponsor.SponsorName + ' sponsor.');
            $.ajax({
                type: "GET",
                url: baseUri,
                dataType: "json",
                beforeSend: function (jqXHR) {
                    addAuthorizationKey(jqXHR);
                }
            })
            .done(function (data) {
                self.sponsors(data);
            })
            .fail(function (jqXHR, textStatus) {
                setError(self, jqXHR, textStatus);
            });
        })
        .fail(function (jqXHR, textStatus) {
            setError(self, jqXHR, textStatus);
        });
    }
}