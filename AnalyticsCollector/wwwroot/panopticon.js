var Panopticon = function (config) {
    this.init(config);
};

Panopticon.prototype = {
    init: function (config) {
        var panopticon = this;

        panopticon.initializeSession(config);
        panopticon.bindEvents();

        // queue page load event
        panopticon.queueEvent({ type: "pagehit", target: { id: null } }, panopticon.session);

        // send page load event
        panopticon.sendEvents(panopticon.session);

        return panopticon;
    },

    initializeSession: function (config) {
        var panopticon = this;

        panopticon.session = {
            updateTimer: typeof (config.updateTimer) == "number" ? config.updateTimer : 2000,
            endpoint: config.collector + "/pushEvents",
            sessionIdUrl: config.collector + "/startSession",
            siteId: config.siteId,
            ready: false,
            cookie: "psess",
            cookieDuration: config.cookieDuration || 1,
            eventTypes: config.eventTypes || new ["click", "blur", "wheel"],
            customProperties: config.customProperties,
            supportsSendBeacon: !!navigator.sendBeacon
        };

        panopticon.getSessionId(panopticon.session);
    },

    bindEvents: function () {
        var panopticon = this;
        panopticon.session.events = [];
        panopticon.session.timer = setInterval(function (e) { panopticon.sendEvents(panopticon.session); }, panopticon.session.updateTimer);

        for (var i = 0; i < panopticon.session.eventTypes.length; i++) {
            document.addEventListener(panopticon.session.eventTypes[i], function (e) { panopticon.queueEvent(e, panopticon.session); });
        }

        // send events before the browser window is closed
        window.addEventListener("beforeunload", function (e) {
            panopticon.sendEvents(panopticon.session, true);
        });
    },

    getSessionId: function (session) {
        var sessionId = this.getCookie(session.cookie)

        if (sessionId) {
            session.sessionId = sessionId;
            session.ready = true;
            return;
        }

        var payload = {
            siteId: session.siteId,
            customProperties: session.customProperties
        };

        var xhr = new XMLHttpRequest();
        xhr.open('POST', session.sessionIdUrl);
        xhr.setRequestHeader("Content-Type", "application/json");
        xhr.onreadystatechange = function (event) {
            var xhr = event.target;

            if (xhr.readyState === 4 && xhr.status === 200) {
                var d = new Date();
                d.setTime(d.getTime() + (session.cookieDuration * 24 * 60 * 60 * 1000));
                document.cookie = "psess=" + xhr.responseText + ";expires=" + d.toUTCString();
                session.sessionId = xhr.responseText;
                session.ready = true;
            }
        };

        xhr.send(JSON.stringify(payload));
    },

    getCookie: function (name) {
        var value = "; " + document.cookie;
        var parts = value.split("; " + name + "=");
        if (parts.length == 2) return parts.pop().split(";").shift();
    },

    sendEvents: function (session, onUnload) {
        if (!session.ready || !session.events || session.events.length === 0) {
            return;
        }

        var payload = {
            sessionId: session.sessionId,
            events: session.events.slice()
        };

        session.events = [];

        if (!!onUnload && session.supportsSendBeacon) {
            navigator.sendBeacon(session.endpoint, JSON.stringify(payload));
        } else {
            var xhr = new XMLHttpRequest();
            xhr.open('PUT', session.endpoint, !onUnload);
            xhr.setRequestHeader("Content-Type", "application/json");
            xhr.send(JSON.stringify(payload));
        }
    },

    queueEvent: function (event, session) {
        var text = "";

        if (event.target.tagName == 'A'
            || event.target.tagName == 'DIV'
            || event.target.tagName == 'SPAN') {
            text = event.target.textContent;
        }

        var classList = null;
        if (event.target.className) {
            classList = event.target.className.split(/\s+/);
        }

        var event = {
            path: window.location.pathname,
            query: window.location.search,
            fragment: window.location.hash.substr(1),
            eventTime: new Date().toISOString(),
            elementId: event.target.id,
            elementType: event.target.tagName,
            elementText: text,
            elementHref: event.target.href,
            elementClasses: classList,
            eventType: event.type,
            x: event.pageX !== null ? Math.round(event.pageX) : undefined,
            y: event.pageY !== null ? Math.round(event.pageY) : undefined,
        };

        session.events.push(event);
    }
};