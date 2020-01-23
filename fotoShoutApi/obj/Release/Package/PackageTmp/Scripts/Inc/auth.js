// Authorization View Model

function AuthorizationViewModel(baseUri) {
    var self = this;
    self.msg = ko.observable(null);
    self.error = ko.observable(null);

    self.login = function (formElm) {
        var credentials = {
            "APIKey": formElm.APIKey.value,
            "Email": formElm.Email.value,
            "Password": formElm.Password.value
        };

        $.ajax({
            type: "POST",
            url: baseUri,
            data: JSON.stringify(credentials),
            contentType: "application/json; charset=utf-8",
            dataType: "json"
        })
        .done(function (data) {
            setMessage(self, "Your authorization key is " + data);
        })
        .fail(function (jqXHR, textStatus) {
            setError(self, jqXHR, textStatus);
        });
    }
}