// Real-time refresh over SignalR. The server broadcasts { module } after any
// successful data change in the household; we reload the current page only if
// it shows data related to that module, so a member's change becomes visible to
// everyone without a manual reload (Docs/00 - "Sinhronizacija u realnom vremenu").
(function () {
    if (!window.signalR || !window.homeOsRealtime) return;

    var current = window.homeOsRealtime.currentController || '';
    var loadedAt = Date.now();

    // Which changed modules should refresh which page. `null` = refresh on any
    // change (dashboard aggregates everything). Kanban and Tasks are two views
    // over the same data, so each refreshes on the other. Calendar also shows
    // task deadlines, so it refreshes on Task changes.
    var relatedness = {
        Home: null,
        Tasks: ['Tasks', 'Kanban'],
        Kanban: ['Tasks', 'Kanban'],
        Calendar: ['Calendar', 'Tasks'],
        Reminders: ['Reminders'],
        Notes: ['Notes'],
        ShoppingLists: ['ShoppingLists']
    };

    function shouldReload(changedModule) {
        if (!(current in relatedness)) return changedModule === current;
        var rule = relatedness[current];
        if (rule === null) return true; // dashboard: any change
        return rule.indexOf(changedModule) !== -1;
    }

    var connection = new signalR.HubConnectionBuilder()
        .withUrl('/hubs/household')
        .withAutomaticReconnect()
        .build();

    connection.on('dataChanged', function (msg) {
        if (!msg || !msg.module) return;
        // Ignore the echo of our own just-submitted action: a POST redirects and
        // reloads this page anyway, and that broadcast lands right after load.
        if (Date.now() - loadedAt < 1500) return;
        if (shouldReload(msg.module)) {
            window.location.reload();
        }
    });

    connection.start().catch(function () { /* offline / not signed in - ignore */ });
})();
