function BaseObject() {
    $.ajaxSetup({
        cache: false
    });
    this.loadHelp();
    this.confirm('.linkLogoff', '#logoutForm');
    this.confirm('.linkTerm', '#acceptForm', '', 'Terms of service', ["Accept", "Cancel"]);
    var submit = $("input[type='submit']");
    if (submit.length === 1) {
        $(submit[0]).click(function () {
            if (window.App !== null)
                window.App.submitting = true;
        });
    }
}

BaseObject.prototype.hoverList = function (selector) {
    $(selector).hover(
        function () {
            $(this).find("*[class='hidden']").removeClass("hidden").addClass("visible");
        },
        function () {
            $(this).find("*[class='visible']").removeClass("visible").addClass("hidden");
        }
    );
};

BaseObject.prototype.hoverFooter = function (selector) {
    $(selector).live({
        mouseover: function () {
            invertVisibility(this);
        },
        mouseout: function () {
            invertVisibility(this);
        }
    });
};

BaseObject.prototype.checkConstrain = function () {
    $('.linkCheckConstrain').live("click", function (e) {
        e.preventDefault();
        var checkConstrain = $('#check-constrain');
        if (checkConstrain !== undefined) {
            var divDlg = document.createElement("div");
            $(checkConstrain).append(divDlg);
            $(divDlg).html("")
                .load(this.href, function (response, status, jqXHR) {
                    if (jqXHR.status === 200) {
                        var cmd = $('#submitCommand', divDlg);
                        if (cmd.length === 1) {
                            window.location.href = cmd[0].href;
                        }
                        else {
                            messageDialog(divDlg);
                        }
                    }
                });
        }
    });
};

BaseObject.prototype.loadHelp = function () {
    $('.linkHelp').on("click mouseover", function (e) {
        e.preventDefault();
        var helpContent = $('#help-content');
        if (helpContent.length === 1) {
            positionDialog(helpContent, [e.pageX, e.pageY + 20], 470);
            return;
        }
        var help = $('#help-pane');
        if (help.length === 1) {
            helpContent = document.createElement("div");
            $(helpContent).attr("id", "help-content");
            $(help).append(helpContent);
            $(helpContent).html("")
                .load(this.href, function (response, status, jqXHR) {
                    if (jqXHR.status === 200) {
                        messageDialog(helpContent, true, 470, false, [e.pageX, e.pageY + 20]);
                    }
                });
        }
    });
};

BaseObject.prototype.confirm = function (cls, frmId, dialogClass, title, buttonLabels) {
    $(cls).live("click", function (e) {
        e.preventDefault();
        var confirm = $('#confirmation');
        if (confirm.length === 1) {
            var divDlg = document.createElement("div");
            $(confirm).append(divDlg);
            $(divDlg).html("")
                .load(this.href, function (response, status, jqXHR) {
                    if (jqXHR.status === 200) {
                        if (dialogClass === undefined)
                            dialogClass = 'dialog-no-title';
                        if (buttonLabels === undefined) {
                            buttons = {
                                Yes: function () {
                                    $(frmId).submit();
                                    $(divDlg).dialog("close");
                                },
                                No: function () {
                                    $(divDlg).dialog("close");
                                }
                            };
                        }
                        else {
                            buttons = [
                                {
                                    text: buttonLabels[0], click: function () {
                                        $(frmId).submit();
                                        $(divDlg).dialog("close");
                                    }
                                },
                                {
                                    text: buttonLabels[1], click: function () {
                                        $(divDlg).dialog("close");
                                    }
                                }
                            ];
                        }
                        var frm = $(frmId, divDlg);
                        if (frm.length === 1) {
                            $(divDlg).dialog({
                                modal: true,
                                dialogClass: dialogClass,
                                zIndex: 10000,
                                autoOpen: true,
                                width: 'auto',
                                resizable: false,
                                buttons: buttons,
                                close: function (ev, ui) {
                                    $(this).remove();
                                },
                                open: function () { $(this).parent().find('.ui-dialog-buttonpane button:eq(1)').focus(); }
                            });
                            if (title !== undefined) {
                                $(divDlg).dialog("option", "title", title);
                            }
                        }
                        else {
                            messageDialog(divDlg);
                        }
                    }
                });
        }
    });
};

function invertVisibility(elm) {
    var hidden = $(elm).find("*[class='hidden']");
    var visible = $(elm).find("*[class='visible']");
    $(hidden).removeClass("hidden").addClass("visible");
    $(visible).removeClass("visible").addClass("hidden");
}

function messageDialog(elm, isModel, w, resizable, pos) {
    if (w === undefined)
        w = 'auto';
    if (isModel === undefined)
        isModel = true;
    if (resizable === undefined)
        resizable = false;

    $(elm).dialog({
        modal: isModel,
        dialogClass: 'dialog-no-title',
        zIndex: 10000,
        autoOpen: true,
        width: w,
        resizable: resizable,
        buttons: {
            Close: function () {
                $(this).dialog("close");
            }
        },
        close: function (ev, ui) {
            $(this).remove();
        }
    });

    if (pos !== undefined)
        $(elm).dialog("option", { position: pos });
}

function positionDialog(elm, pos, w) {
    if (pos !== undefined) {
        $(elm).dialog("option", { position: pos });
        if (w !== undefined)
        $(elm).dialog("option", "width", w);
    }
}