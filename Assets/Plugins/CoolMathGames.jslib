mergeInto(LibraryManager.library, {
    RedirectTo: function () {
        if (parent !== undefined && parent !== null) 
            parent.document.location = "https://www.coolmathgames.com";
        else document.location = "https://www.coolmathgames.com";
    },
    StartGameEvent: function () {
        if (parent !== undefined && parent !== null)
            parent.cmgGameEvent('start');
    },
    StartLevelEvent: function (level) {
        if (parent !== undefined && parent !== null)
            parent.cmgGameEvent('start', level);
    },
    ReplayEvent: function (level) {
        if (parent !== undefined && parent !== null)
            parent.cmgGameEvent('replay', level);
    }
});