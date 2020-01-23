function TabControl(appPath) {
    if (appPath != undefined) {
        if (!appPath.endsWith("/"))
            appPath += "/";
        this.appPath = appPath;
    }
}

TabControl.prototype.getContentTab = function (tabName) {
    if (this.curAction != undefined) {
        var url = this.appPath + "?tabName=" + tabName;
        confirm(url);
    }
    else {
        this.getContent(tabName);
    }
}

TabControl.prototype.getContent = function (tabName) {
    var url = this.appPath + tabName;
    window.location.href = url;
}

