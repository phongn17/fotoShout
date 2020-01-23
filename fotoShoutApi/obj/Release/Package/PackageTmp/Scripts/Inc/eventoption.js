// Event Options View Model

function EventOptionsViewModel(baseUri) {
    var self = this;
    self.user = ko.observable(null);
    self.eventOptions = ko.observableArray();
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
        self.eventOptions(data);
    })
    .fail(function (jqXHR, textStatus) {
        setError(self, jqXHR, textStatus);
    });

    self.create = function (formElm) {
        $(formElm).validate();
        if ($(formElm).valid()) {
            var evo = $(formElm).serializeArray();
            $.ajax({
                type: "POST",
                url: baseUri, 
                data: convertToJsonString(evo), 
                dataType: "json",
                beforeSend: function (jqXHR) {
                    addAuthorizationKey(jqXHR);
                }
            })
            .done(function (evo) {
                // add the new event option to the view-model
                self.eventOptions.push(evo);
                setMessage(self, 'Added the ' + evo.EventOptionName + ' event.')
            })
            .fail(function (jqXHR, textStatus) {
                setError(self, jqXHR, textStatus);
            });
        }
    }

    self.update = function (evo) {
        $.ajax({
            type: "PUT",
            url: baseUri + '/' + evo.EventOptionId,
            data: evo,
            beforeSend: function (jqXHR) {
                addAuthorizationKey(jqXHR);
            }
        })
        .done(function () {
            setMessage(self, 'Updated the ' + evo.EventOptionName + ' event option.')
        })
        .fail(function (jqXHR, textStatus) {
            setError(self, jqXHR, textStatus);
        });
    }

    self.delete = function (evo) {
        // First remove from the server, then remove from the model-view
        $.ajax({
            type: "DELETE",
            url: baseUri + '/' + evo.EventOptionId,
            beforeSend: function (jqXHR) {
                addAuthorizationKey(jqXHR);
            }
        })
        .done(function (evo) {
            self.eventOptions.remove(evo);
            setMessage(self, 'Deleted the ' + evo.EventOptionName + ' event option.')
            $.ajax({
                type: "GET",
                url: baseUri,
                dataType: "json",
                beforeSend: function (jqXHR) {
                    addAuthorizationKey(jqXHR);
                }
            })
            .done(function (data) {
                self.eventOptions(data);
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

