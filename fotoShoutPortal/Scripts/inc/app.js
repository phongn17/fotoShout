if (!window.App) {
    window.App = {
        init: function (frm, selected) {
            window.onbeforeunload = this.onBeforeUnloadWindow;
            this.frm = frm;
            this.selected = selected;
            this.initialFormData = $(frm).serialize();
            this.submitting = false;
        },
        onBeforeUnloadWindow: function () {
            return window.App.hasPendingChanges();
        },
        selected: 0,
        frm: "form",
        submitting: false,
        initialFormData: "",
        hasPendingChanges: function () {
            if (!window.App.submitting && window.App.initialFormData !== $(window.App.frm).serialize()) {
                $("#tabs-mainmenu").tabs({ active: window.App.selected });
                return "You have pending changes. \n\rIf you leave the current page, those changes will be lost.";
            }
            return undefined;
        }
    };
}