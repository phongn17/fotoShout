// Utility functions

function convertToJsonString(params) {
    var ret = {};
    for (var idx = (params.length - 1); idx >= 0; idx--) {
        var param = params[idx];
        if (param.name.indexOf('.') == -1) {
            ret[param.name] = param.value;
        }
        else {
            var pair = param.name.split('.');
            if (pair.length == 2) {
                var obj = ret[pair[0]];
                if (obj == null) {
                    obj = {};
                    ret[pair[0]] = obj;
                }
                obj[pair[1]] = param.value;
            }
        }
    }

    return ret;
}

function initializeEvent(type) {
    $("#EventDate").datepicker();
    $("#ev-editor > p").append("<input id=\"ev-submit\" type=\"submit\" value=\"" + type + "\" />");
}

function setMessage(self, msg) {
    self.msg(msg);
    self.error(null);
}

function setError(self, jqXHR, textStatus, response) {
    if (response != undefined) {
        $("#error-page").removeClass("message-error");
        self.error(response);
    }
    else {
        $("#error-page").addClass("message-error");
        self.error(textStatus + ' - ' + jqXHR.status + ': ' + jqXHR.responseText);
    }
    self.msg(null);
}

function addAuthorizationKey(jqXHR) {
    //jqXHR.setRequestHeader("FS-Authorization", "6b8ff049-b656-475c-b918-a6f582d0fb9d");
    jqXHR.setRequestHeader("FS-Authorization", "18a4f579-f907-4f07-8ba8-84bbbecd99bd");
}