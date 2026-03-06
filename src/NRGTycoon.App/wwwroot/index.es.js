import P, { createContext as Y, useState as b, useEffect as C, useContext as Q, useRef as ce } from "react";
import V from "axios";
import * as Ae from "@microsoft/signalr";
import { useNavigate as le, useLocation as ye, Link as R, Outlet as re } from "react-router-dom";
var D = { exports: {} }, I = {};
var ne;
function be() {
  if (ne) return I;
  ne = 1;
  var s = Symbol.for("react.transitional.element"), r = Symbol.for("react.fragment");
  function a(m, d, c) {
    var h = null;
    if (c !== void 0 && (h = "" + c), d.key !== void 0 && (h = "" + d.key), "key" in d) {
      c = {};
      for (var u in d)
        u !== "key" && (c[u] = d[u]);
    } else c = d;
    return d = c.ref, {
      $$typeof: s,
      type: m,
      key: h,
      ref: d !== void 0 ? d : null,
      props: c
    };
  }
  return I.Fragment = r, I.jsx = a, I.jsxs = a, I;
}
var M = {};
var ae;
function xe() {
  return ae || (ae = 1, process.env.NODE_ENV !== "production" && (function() {
    function s(e) {
      if (e == null) return null;
      if (typeof e == "function")
        return e.$$typeof === _e ? null : e.displayName || e.name || null;
      if (typeof e == "string") return e;
      switch (e) {
        case y:
          return "Fragment";
        case j:
          return "Profiler";
        case k:
          return "StrictMode";
        case $:
          return "Suspense";
        case S:
          return "SuspenseList";
        case ge:
          return "Activity";
      }
      if (typeof e == "object")
        switch (typeof e.tag == "number" && console.error(
          "Received an unexpected object in getComponentNameFromType(). This is likely a bug in React. Please file an issue."
        ), e.$$typeof) {
          case A:
            return "Portal";
          case G:
            return e.displayName || "Context";
          case L:
            return (e._context.displayName || "Context") + ".Consumer";
          case q:
            var l = e.render;
            return e = e.displayName, e || (e = l.displayName || l.name || "", e = e !== "" ? "ForwardRef(" + e + ")" : "ForwardRef"), e;
          case O:
            return l = e.displayName || null, l !== null ? l : s(e.type) || "Memo";
          case T:
            l = e._payload, e = e._init;
            try {
              return s(e(l));
            } catch {
            }
        }
      return null;
    }
    function r(e) {
      return "" + e;
    }
    function a(e) {
      try {
        r(e);
        var l = !1;
      } catch {
        l = !0;
      }
      if (l) {
        l = console;
        var _ = l.error, w = typeof Symbol == "function" && Symbol.toStringTag && e[Symbol.toStringTag] || e.constructor.name || "Object";
        return _.call(
          l,
          "The provided key is an unsupported type %s. This value must be coerced to a string before using it here.",
          w
        ), r(e);
      }
    }
    function m(e) {
      if (e === y) return "<>";
      if (typeof e == "object" && e !== null && e.$$typeof === T)
        return "<...>";
      try {
        var l = s(e);
        return l ? "<" + l + ">" : "<...>";
      } catch {
        return "<...>";
      }
    }
    function d() {
      var e = z.A;
      return e === null ? null : e.getOwner();
    }
    function c() {
      return Error("react-stack-top-frame");
    }
    function h(e) {
      if (X.call(e, "key")) {
        var l = Object.getOwnPropertyDescriptor(e, "key").get;
        if (l && l.isReactWarning) return !1;
      }
      return e.key !== void 0;
    }
    function u(e, l) {
      function _() {
        Z || (Z = !0, console.error(
          "%s: `key` is not a prop. Trying to access it will result in `undefined` being returned. If you need to access the same value within the child component, you should pass it as a different prop. (https://react.dev/link/special-props)",
          l
        ));
      }
      _.isReactWarning = !0, Object.defineProperty(e, "key", {
        get: _,
        configurable: !0
      });
    }
    function N() {
      var e = s(this.type);
      return K[e] || (K[e] = !0, console.error(
        "Accessing element.ref was removed in React 19. ref is now a regular prop. It will be removed from the JSX Element type in a future release."
      )), e = this.props.ref, e !== void 0 ? e : null;
    }
    function g(e, l, _, w, F, W) {
      var v = _.ref;
      return e = {
        $$typeof: p,
        type: e,
        key: l,
        props: _,
        _owner: w
      }, (v !== void 0 ? v : null) !== null ? Object.defineProperty(e, "ref", {
        enumerable: !1,
        get: N
      }) : Object.defineProperty(e, "ref", { enumerable: !1, value: null }), e._store = {}, Object.defineProperty(e._store, "validated", {
        configurable: !1,
        enumerable: !1,
        writable: !0,
        value: 0
      }), Object.defineProperty(e, "_debugInfo", {
        configurable: !1,
        enumerable: !1,
        writable: !0,
        value: null
      }), Object.defineProperty(e, "_debugStack", {
        configurable: !1,
        enumerable: !1,
        writable: !0,
        value: F
      }), Object.defineProperty(e, "_debugTask", {
        configurable: !1,
        enumerable: !1,
        writable: !0,
        value: W
      }), Object.freeze && (Object.freeze(e.props), Object.freeze(e)), e;
    }
    function n(e, l, _, w, F, W) {
      var v = l.children;
      if (v !== void 0)
        if (w)
          if (we(v)) {
            for (w = 0; w < v.length; w++)
              o(v[w]);
            Object.freeze && Object.freeze(v);
          } else
            console.error(
              "React.jsx: Static children should always be an array. You are likely explicitly calling React.jsxs or React.jsxDEV. Use the Babel transform instead."
            );
        else o(v);
      if (X.call(l, "key")) {
        v = s(e);
        var U = Object.keys(l).filter(function(ve) {
          return ve !== "key";
        });
        w = 0 < U.length ? "{key: someKey, " + U.join(": ..., ") + ": ...}" : "{key: someKey}", se[v + w] || (U = 0 < U.length ? "{" + U.join(": ..., ") + ": ...}" : "{}", console.error(
          `A props object containing a "key" prop is being spread into JSX:
  let props = %s;
  <%s {...props} />
React keys must be passed directly to JSX without using spread:
  let props = %s;
  <%s key={someKey} {...props} />`,
          w,
          v,
          U,
          v
        ), se[v + w] = !0);
      }
      if (v = null, _ !== void 0 && (a(_), v = "" + _), h(l) && (a(l.key), v = "" + l.key), "key" in l) {
        _ = {};
        for (var H in l)
          H !== "key" && (_[H] = l[H]);
      } else _ = l;
      return v && u(
        _,
        typeof e == "function" ? e.displayName || e.name || "Unknown" : e
      ), g(
        e,
        v,
        _,
        d(),
        F,
        W
      );
    }
    function o(e) {
      i(e) ? e._store && (e._store.validated = 1) : typeof e == "object" && e !== null && e.$$typeof === T && (e._payload.status === "fulfilled" ? i(e._payload.value) && e._payload.value._store && (e._payload.value._store.validated = 1) : e._store && (e._store.validated = 1));
    }
    function i(e) {
      return typeof e == "object" && e !== null && e.$$typeof === p;
    }
    var f = P, p = Symbol.for("react.transitional.element"), A = Symbol.for("react.portal"), y = Symbol.for("react.fragment"), k = Symbol.for("react.strict_mode"), j = Symbol.for("react.profiler"), L = Symbol.for("react.consumer"), G = Symbol.for("react.context"), q = Symbol.for("react.forward_ref"), $ = Symbol.for("react.suspense"), S = Symbol.for("react.suspense_list"), O = Symbol.for("react.memo"), T = Symbol.for("react.lazy"), ge = Symbol.for("react.activity"), _e = Symbol.for("react.client.reference"), z = f.__CLIENT_INTERNALS_DO_NOT_USE_OR_WARN_USERS_THEY_CANNOT_UPGRADE, X = Object.prototype.hasOwnProperty, we = Array.isArray, J = console.createTask ? console.createTask : function() {
      return null;
    };
    f = {
      react_stack_bottom_frame: function(e) {
        return e();
      }
    };
    var Z, K = {}, ee = f.react_stack_bottom_frame.bind(
      f,
      c
    )(), te = J(m(c)), se = {};
    M.Fragment = y, M.jsx = function(e, l, _) {
      var w = 1e4 > z.recentlyCreatedOwnerStacks++;
      return n(
        e,
        l,
        _,
        !1,
        w ? Error("react-stack-top-frame") : ee,
        w ? J(m(e)) : te
      );
    }, M.jsxs = function(e, l, _) {
      var w = 1e4 > z.recentlyCreatedOwnerStacks++;
      return n(
        e,
        l,
        _,
        !0,
        w ? Error("react-stack-top-frame") : ee,
        w ? J(m(e)) : te
      );
    };
  })()), M;
}
var oe;
function Ne() {
  return oe || (oe = 1, process.env.NODE_ENV === "production" ? D.exports = be() : D.exports = xe()), D.exports;
}
var t = Ne();
const x = V.create({
  baseURL: "/auth",
  headers: {
    "Content-Type": "application/json"
  },
  withCredentials: !0
  // Important for cookie-based auth if needed
});
x.interceptors.request.use(
  (s) => {
    const r = localStorage.getItem("accessToken");
    return r && (s.headers.Authorization = `Bearer ${r}`), s;
  },
  (s) => Promise.reject(s)
);
x.interceptors.response.use(
  (s) => s,
  async (s) => {
    const r = s.config;
    if (s.response?.status === 401 && r) {
      const c = localStorage.getItem("refreshToken");
      if (c && !r._retry) {
        r._retry = !0;
        try {
          const h = await V.post("/auth/token/refresh", {
            refreshToken: c
          }), { accessToken: u, refreshToken: N } = h.data;
          return localStorage.setItem("accessToken", u), localStorage.setItem("refreshToken", N), r.headers.Authorization = `Bearer ${u}`, x(r);
        } catch (h) {
          return localStorage.removeItem("accessToken"), localStorage.removeItem("refreshToken"), localStorage.getItem("quasar_auth_mocked") === "true" || (window.location.href = "/login"), Promise.reject(h);
        }
      }
    }
    const a = s.config?.url?.endsWith("/login"), m = s.response?.data;
    if (s.response?.status === 401 && m?.code === "SESSION_REVOKED") {
      localStorage.removeItem("accessToken"), localStorage.removeItem("refreshToken"), window.dispatchEvent(new CustomEvent("session-revoked", {
        detail: { message: m.message }
      }));
      const c = {
        message: m.message,
        statusCode: 401
      };
      return Promise.reject(c);
    }
    const d = {
      message: s.response?.status === 401 && a ? "Invalid username or password" : m?.message || s.message || "An error occurred",
      statusCode: s.response?.status || 500
    };
    return Promise.reject(d);
  }
);
const ie = {
  /**
   * Login with username and password
   */
  login: async (s) => {
    const r = await x.post("/login", s);
    return localStorage.setItem("accessToken", r.data.accessToken), localStorage.setItem("refreshToken", r.data.refreshToken), r.data;
  },
  /**
   * Register a new user
   */
  register: async (s) => (await x.post("/register", s)).data,
  /**
   * Refresh access token
   */
  refreshToken: async (s) => {
    const r = await x.post("/token/refresh", s);
    return localStorage.setItem("accessToken", r.data.accessToken), localStorage.setItem("refreshToken", r.data.refreshToken), r.data;
  },
  /**
   * Logout and revoke refresh token
   */
  logout: async () => {
    const s = localStorage.getItem("refreshToken");
    if (s)
      try {
        await x.post("/logout", { refreshToken: s });
      } catch (r) {
        console.error("Logout error:", r);
      }
    localStorage.removeItem("accessToken"), localStorage.removeItem("refreshToken");
  },
  /**
   * Check if user is authenticated
   */
  isAuthenticated: () => !!localStorage.getItem("accessToken")
}, de = {
  /**
   * Get all users
   */
  list: async () => (await x.get("/users")).data,
  /**
   * Get roles for a specific user
   */
  getRoles: async (s) => (await x.get(`/users/${s}/roles`)).data,
  /**
   * Get permissions for a specific user
   */
  getPermissions: async (s) => (await x.get(`/users/${s}/permissions`)).data,
  /**
   * Assign a role to a user
   */
  assignRole: async (s, r) => {
    await x.post(`/users/${s}/roles`, r);
  },
  /**
   * Revoke a role from a user
   */
  revokeRole: async (s, r) => {
    await x.delete(`/users/${s}/roles/${r}`);
  },
  /**
   * Reset a user's password (admin only)
   * Returns the new generated password
   */
  resetPassword: async (s) => (await x.post(`/users/${s}/reset-password`)).data.password,
  /**
   * Reset own password (any authenticated user)
   * Returns the new generated password
   */
  resetOwnPassword: async () => (await x.post("/users/me/reset-password")).data.password,
  delete: async (s) => {
    await x.delete(`/users/${s}`);
  }
}, Se = {
  async list() {
    return (await x.get("/api/features", { baseURL: "/" })).data;
  },
  async listDirect() {
    return (await V.get("/api/features", {
      headers: ke(),
      withCredentials: !0
    })).data;
  }
};
function ke() {
  const s = localStorage.getItem("accessToken");
  return s ? { Authorization: `Bearer ${s}` } : {};
}
const ue = Y(void 0), B = () => {
  const s = Q(ue);
  if (!s)
    throw new Error("useAuth must be used within an AuthProvider");
  return s;
}, $e = ({ children: s }) => {
  const [r, a] = b(null), [m, d] = b(!0);
  C(() => {
    let n = !0;
    return (async () => {
      try {
        const i = await fetch("/api/config/ui");
        if (i.ok) {
          const f = await i.json();
          if (f.requireAuthentication === !1) {
            n && (localStorage.setItem("quasar_auth_mocked", "true"), a({
              id: "00000000-0000-0000-0000-000000000000",
              username: f.applicationName || "Local User",
              email: "local@localhost",
              roles: ["administrator"]
              // Give admin access so they can see everything
            }), d(!1));
            return;
          }
        }
      } catch (i) {
        console.error("Failed to fetch UI settings for auth check:", i);
      }
      n && await c();
    })(), () => {
      n = !1;
    };
  }, []);
  const c = async () => {
    try {
      const n = localStorage.getItem("accessToken");
      if (n) {
        const o = h(n);
        console.log("Decoded payload:", o);
        const i = o.sub, f = o.unique_name || o.name || "User";
        let p = [];
        try {
          console.log("Fetching roles for userId:", i);
          const A = await de.getRoles(i);
          console.log("Fetched userRoles:", A), p = A.map((y) => y.name), console.log("Mapped roles:", p);
        } catch (A) {
          console.error("Failed to fetch roles", A);
        }
        a({
          id: i,
          username: f,
          email: o.email || "",
          roles: p
        });
      } else
        a(null);
    } catch (n) {
      console.error("Auth check failed:", n), a(null);
    } finally {
      d(!1);
    }
  };
  function h(n) {
    var o = n.split(".")[1], i = o.replace(/-/g, "+").replace(/_/g, "/"), f = decodeURIComponent(window.atob(i).split("").map(function(p) {
      return "%" + ("00" + p.charCodeAt(0).toString(16)).slice(-2);
    }).join(""));
    return JSON.parse(f);
  }
  const g = {
    user: r,
    isAuthenticated: !!r,
    isLoading: m,
    login: async (n) => {
      d(!0);
      try {
        await ie.login(n), await c();
      } catch (o) {
        throw a(null), o;
      } finally {
        d(!1);
      }
    },
    logout: async () => {
      d(!0);
      try {
        await ie.logout();
      } catch (n) {
        console.error("Logout error:", n);
      } finally {
        a(null), d(!1);
      }
    }
  };
  return /* @__PURE__ */ t.jsx(ue.Provider, { value: g, children: s });
};
class Re {
  connection = null;
  listeners = [];
  recentIds = /* @__PURE__ */ new Set();
  async start() {
    if (!this.connection) {
      this.connection = new Ae.HubConnectionBuilder().withUrl("/hubs/notifications", {
        accessTokenFactory: () => localStorage.getItem("accessToken") || ""
      }).withAutomaticReconnect().build(), this.connection.on("ReceiveNotification", (r) => {
        console.log("[SignalR] RAW ReceiveNotification:", JSON.stringify(r));
        const a = {
          id: r.id || r.Id || "",
          title: r.title || r.Title || "",
          message: r.message || r.Message || "",
          type: (r.type || r.Type || "info").toLowerCase(),
          createdAt: r.createdAt || r.CreatedAt || (/* @__PURE__ */ new Date()).toISOString()
        };
        if (console.log("[SignalR] Normalized notification id:", a.id, "recentIds:", [...this.recentIds], "listeners:", this.listeners.length), a.id && this.recentIds.has(a.id)) {
          console.log("[SignalR] DEDUP: Skipping already-seen notification", a.id);
          return;
        }
        a.id && (this.recentIds.add(a.id), setTimeout(() => this.recentIds.delete(a.id), 1e4)), console.log("[SignalR] Forwarding to", this.listeners.length, "listeners"), this.listeners.forEach((m) => m(a));
      });
      try {
        await this.connection.start(), console.log("Notification SignalR Connected");
      } catch (r) {
        console.error("Notification SignalR Connection Error: ", r), this.connection = null;
      }
    }
  }
  async stop() {
    if (this.connection)
      try {
        this.connection.off("ReceiveNotification"), await this.connection.stop();
      } catch (r) {
        console.error("Error stopping SignalR connection:", r);
      } finally {
        this.connection = null;
      }
  }
  subscribe(r) {
    return this.listeners = [r], () => {
      this.listeners = this.listeners.filter((a) => a !== r);
    };
  }
}
const E = new Re(), je = async () => {
  const s = localStorage.getItem("accessToken");
  if (!s) return [];
  try {
    const r = await fetch("/api/player/notifications", {
      headers: {
        Authorization: `Bearer ${s}`
      }
    });
    if (r.status === 401) return [];
    if (!r.ok) throw new Error("Failed to fetch notifications");
    return await r.json();
  } catch (r) {
    return console.error("Error fetching notifications:", r), [];
  }
}, Te = async (s) => {
  const r = localStorage.getItem("accessToken");
  if (r)
    try {
      await fetch(`/api/player/notifications/${s}/read`, {
        method: "POST",
        headers: {
          Authorization: `Bearer ${r}`
        }
      });
    } catch (a) {
      console.error("Error marking notification as read:", a);
    }
}, Ee = async () => {
  const s = localStorage.getItem("accessToken");
  if (s)
    try {
      await fetch("/api/player/notifications/read-all", {
        method: "POST",
        headers: {
          Authorization: `Bearer ${s}`
        }
      });
    } catch (r) {
      console.error("Error marking all notifications as read:", r);
    }
}, me = Y(void 0), Fe = ({ children: s }) => {
  const [r, a] = b([]), { isAuthenticated: m } = B(), d = r.filter((g) => !g.read).length, c = P.useCallback((g) => {
    const n = {
      ...g,
      id: g.id || Math.random().toString(36).substring(2, 9),
      timestamp: g.createdAt ? new Date(g.createdAt) : /* @__PURE__ */ new Date(),
      read: !1
    };
    console.log("[Notification] addNotification called, id:", n.id, "title:", n.title), a((o) => {
      const i = o.some((f) => f.id === n.id);
      return console.log("[Notification] setNotifications: exists=", i, "prevCount=", o.length, "prevIds=", o.map((f) => f.id)), i ? o : [n, ...o].slice(0, 50);
    });
  }, []);
  P.useEffect(() => {
    let g = !0;
    (async () => {
      if (!m) {
        a([]), E.stop().catch(console.error);
        return;
      }
      await E.stop(), await E.start();
      const i = await je();
      if (console.log("[Notification] fetchUnread returned", i.length, "items, ids:", i.map((f) => f.id)), g) {
        const f = i.map((p) => ({
          id: p.id,
          title: p.title,
          message: p.message,
          type: p.type,
          timestamp: new Date(p.createdAt),
          read: p.isRead
        }));
        a((p) => {
          const A = new Set(p.map((k) => k.id)), y = f.filter((k) => !A.has(k.id));
          return console.log("[Notification] fetchUnread merge: existingIds=", [...A], "newItems=", y.length), [...y, ...p].sort((k, j) => j.timestamp.getTime() - k.timestamp.getTime());
        });
      }
    })();
    const o = E.subscribe((i) => {
      c({
        id: i.id,
        title: i.title,
        message: i.message,
        type: i.type,
        createdAt: i.createdAt
      });
    });
    return () => {
      g = !1, o(), E.stop().catch(console.error);
    };
  }, [m, c]);
  const h = P.useCallback((g) => {
    Te(g), a((n) => n.map((o) => o.id === g ? { ...o, read: !0 } : o));
  }, []), u = P.useCallback(() => {
    Ee(), a((g) => g.map((n) => ({ ...n, read: !0 })));
  }, []), N = P.useCallback(() => {
    a([]);
  }, []);
  return /* @__PURE__ */ t.jsx(me.Provider, { value: {
    notifications: r,
    unreadCount: d,
    addNotification: c,
    markAsRead: h,
    markAllAsRead: u,
    clearNotifications: N
  }, children: s });
}, Ce = () => {
  const s = Q(me);
  if (!s)
    throw new Error("useNotifications must be used within a NotificationProvider");
  return s;
}, fe = Y(void 0), De = ({ children: s }) => {
  const [r, a] = b(null), [m, d] = b(!0), [c, h] = b([]), [u, N] = b([]), [g, n] = b(null), [o, i] = b([]), [f, p] = b(null);
  return C(() => {
    (async () => {
      try {
        const y = await fetch("/api/config/ui");
        if (y.ok) {
          const k = await y.json();
          a(k);
          const j = `theme-${k.theme || "dark"}`;
          document.documentElement.className = j;
        }
      } catch (y) {
        console.error("Failed to fetch UI settings:", y), document.documentElement.className = "theme-dark";
      } finally {
        d(!1);
      }
    })(), Array.isArray(window.__QUASAR_CUSTOM_MENU__) && h(window.__QUASAR_CUSTOM_MENU__), Array.isArray(window.__QUASAR_CUSTOM_ROUTES__) && N(window.__QUASAR_CUSTOM_ROUTES__), window.__QUASAR_CUSTOM_HEADER__ && n(() => window.__QUASAR_CUSTOM_HEADER__), Array.isArray(window.__QUASAR_CUSTOM_ACTIONS__) && i(window.__QUASAR_CUSTOM_ACTIONS__), window.__QUASAR_CUSTOM_OVERLAY__ && p(() => window.__QUASAR_CUSTOM_OVERLAY__);
  }, []), C(() => {
    const A = () => {
      Array.isArray(window.__QUASAR_CUSTOM_MENU__) && h(window.__QUASAR_CUSTOM_MENU__), Array.isArray(window.__QUASAR_CUSTOM_ROUTES__) && N(window.__QUASAR_CUSTOM_ROUTES__), window.__QUASAR_CUSTOM_HEADER__ && n(() => window.__QUASAR_CUSTOM_HEADER__), Array.isArray(window.__QUASAR_CUSTOM_ACTIONS__) && i(window.__QUASAR_CUSTOM_ACTIONS__), window.__QUASAR_CUSTOM_OVERLAY__ && p(() => window.__QUASAR_CUSTOM_OVERLAY__);
    };
    return window.__QUASAR_BUNDLE_LOADED__ && A(), window.addEventListener("quasar-bundle-loaded", A), () => window.removeEventListener("quasar-bundle-loaded", A);
  }, []), /* @__PURE__ */ t.jsx(fe.Provider, { value: { settings: r, isLoading: m, customMenu: c, customRoutes: u, customHeaderComponent: g, customActions: o, customOverlayComponent: f }, children: s });
}, he = () => {
  const s = Q(fe);
  if (!s)
    throw new Error("useUi must be used within a UiProvider");
  return s;
}, pe = Y(void 0), Ye = ({ children: s }) => {
  const [r, a] = b([]), [m, d] = b(!0);
  C(() => {
    (async () => {
      try {
        const u = await Se.list();
        a(u);
      } catch (u) {
        console.error("Failed to fetch features:", u);
      } finally {
        d(!1);
      }
    })();
  }, []);
  const c = (h) => r.some((u) => u.id === h);
  return /* @__PURE__ */ t.jsx(pe.Provider, { value: { features: r, isLoading: m, hasFeature: c }, children: s });
}, Oe = () => {
  const s = Q(pe);
  if (!s)
    throw new Error("useFeatures must be used within a FeatureProvider");
  return s;
}, Ue = () => {
  const { notifications: s, unreadCount: r, addNotification: a, markAsRead: m, markAllAsRead: d } = Ce(), [c, h] = b(!1), u = ce(null);
  C(() => {
    const n = E.subscribe((o) => {
      a({
        title: o.title,
        message: o.message,
        type: o.type
      });
    });
    return E.start(), () => {
      n();
    };
  }, [a]), C(() => {
    const n = (o) => {
      u.current && !u.current.contains(o.target) && h(!1);
    };
    return c && document.addEventListener("mousedown", n), () => {
      document.removeEventListener("mousedown", n);
    };
  }, [c]);
  const N = (n) => {
    const i = (/* @__PURE__ */ new Date()).getTime() - n.getTime(), f = Math.floor(i / 6e4);
    if (f < 1) return "Just now";
    if (f < 60) return `${f}m ago`;
    const p = Math.floor(f / 60);
    return p < 24 ? `${p}h ago` : n.toLocaleDateString();
  }, g = (n) => {
    m(n);
  };
  return /* @__PURE__ */ t.jsxs("div", { className: "notifications-container", ref: u, children: [
    /* @__PURE__ */ t.jsxs(
      "button",
      {
        className: "bell-button",
        onClick: () => h(!c),
        "aria-label": "Notifications",
        children: [
          /* @__PURE__ */ t.jsx("span", { children: "🔔" }),
          r > 0 && /* @__PURE__ */ t.jsx("span", { className: "notification-badge", children: r > 9 ? "9+" : r })
        ]
      }
    ),
    c && /* @__PURE__ */ t.jsxs("div", { className: "notifications-dropdown", children: [
      /* @__PURE__ */ t.jsxs("div", { className: "notifications-header", children: [
        /* @__PURE__ */ t.jsx("h3", { children: "Notifications" }),
        r > 0 && /* @__PURE__ */ t.jsx("button", { className: "mark-all-btn", onClick: d, children: "Mark all as read" })
      ] }),
      /* @__PURE__ */ t.jsx("div", { className: "notifications-list", children: s.length === 0 ? /* @__PURE__ */ t.jsx("div", { className: "empty-notifications", children: "No notifications yet" }) : s.map((n) => /* @__PURE__ */ t.jsxs(
        "div",
        {
          className: `notification-item ${n.read ? "read" : "unread"}`,
          onClick: () => g(n.id),
          children: [
            /* @__PURE__ */ t.jsxs("div", { className: "notification-item-header", children: [
              /* @__PURE__ */ t.jsx("span", { className: `notification-title type-${n.type}`, children: n.title }),
              /* @__PURE__ */ t.jsx("span", { className: "notification-time", children: N(n.timestamp) })
            ] }),
            /* @__PURE__ */ t.jsx("div", { className: "notification-message", children: n.message })
          ]
        },
        n.id
      )) })
    ] })
  ] });
}, Pe = () => {
  const { user: s, logout: r } = B(), [a, m] = b(!1), [d, c] = b(null), [h, u] = b(!1), [N, g] = b(!1), n = le(), o = ce(null), { customMenu: i, customHeaderComponent: f, settings: p } = he(), A = p?.requireAuthentication !== !1, y = async () => {
    await r(), n("/login");
  }, k = async () => {
    u(!0), m(!1);
  }, j = async () => {
    try {
      const S = await de.resetOwnPassword();
      c(S), u(!1);
    } catch (S) {
      console.error("Failed to change password:", S), alert("Failed to change password"), u(!1);
    }
  };
  C(() => {
    const S = (O) => {
      o.current && !o.current.contains(O.target) && m(!1);
    };
    return document.addEventListener("mousedown", S), () => {
      document.removeEventListener("mousedown", S);
    };
  }, []);
  const { pathname: L } = ye(), q = (() => {
    const O = (i.length > 0 ? i : window.__QUASAR_CUSTOM_MENU__ || []).flatMap((T) => T.items).find((T) => T.path === L);
    if (O) return O.label;
    switch (L) {
      case "/":
        return "Dashboard";
      // Fallback if not overridden
      case "/users":
        return "Users";
      case "/roles":
        return "Roles";
      case "/features":
        return "Features";
      case "/jobs":
        return "Jobs";
      case "/logs":
        return "Logs";
      case "/metrics":
        return "Metrics";
      case "/sessions":
        return "Sessions";
      default:
        return "Dashboard";
    }
  })(), $ = f;
  return /* @__PURE__ */ t.jsxs(t.Fragment, { children: [
    /* @__PURE__ */ t.jsxs("header", { className: "app-header", children: [
      /* @__PURE__ */ t.jsx("div", { className: "header-left", children: $ ? /* @__PURE__ */ t.jsx($, {}) : /* @__PURE__ */ t.jsx("h2", { className: "page-title", children: q }) }),
      /* @__PURE__ */ t.jsxs("div", { className: "header-right", children: [
        /* @__PURE__ */ t.jsx(Ue, {}),
        A && /* @__PURE__ */ t.jsxs("div", { className: "user-profile", children: [
          /* @__PURE__ */ t.jsxs("div", { className: "user-info", children: [
            /* @__PURE__ */ t.jsx("span", { className: "user-name", children: s?.username || "Guest" }),
            /* @__PURE__ */ t.jsx("span", { className: "user-role", children: s?.roles?.[0] || "User" })
          ] }),
          /* @__PURE__ */ t.jsx("div", { className: "user-avatar", children: s?.username?.charAt(0).toUpperCase() || "U" })
        ] }),
        A && /* @__PURE__ */ t.jsxs("div", { className: "menu-container", ref: o, children: [
          /* @__PURE__ */ t.jsx(
            "button",
            {
              className: "menu-button",
              "aria-label": "Menu",
              onClick: () => m(!a),
              children: /* @__PURE__ */ t.jsx("span", { className: "menu-icon", children: "⋮" })
            }
          ),
          a && /* @__PURE__ */ t.jsxs("div", { className: "dropdown-menu", children: [
            /* @__PURE__ */ t.jsx("button", { onClick: k, className: "dropdown-item", children: "Change Password" }),
            /* @__PURE__ */ t.jsx("button", { onClick: y, className: "dropdown-item danger", children: "Sign Out" })
          ] })
        ] })
      ] })
    ] }),
    h && /* @__PURE__ */ t.jsx("div", { className: "modal-overlay", onClick: () => u(!1), children: /* @__PURE__ */ t.jsxs("div", { className: "modal", onClick: (S) => S.stopPropagation(), children: [
      /* @__PURE__ */ t.jsxs("div", { className: "modal-header", children: [
        /* @__PURE__ */ t.jsx("h2", { className: "modal-title", children: "Confirm Password Change" }),
        /* @__PURE__ */ t.jsx("button", { className: "modal-close", onClick: () => u(!1), children: "×" })
      ] }),
      /* @__PURE__ */ t.jsxs("div", { className: "modal-body", children: [
        /* @__PURE__ */ t.jsx("p", { children: "Generate a new password?" }),
        /* @__PURE__ */ t.jsx("p", { className: "text-muted", style: { marginTop: "var(--spacing-md)" }, children: "You will be logged out and need to login with the new password." }),
        /* @__PURE__ */ t.jsxs("div", { style: { display: "flex", gap: "var(--spacing-md)", marginTop: "var(--spacing-lg)" }, children: [
          /* @__PURE__ */ t.jsx(
            "button",
            {
              className: "btn btn-secondary",
              onClick: () => u(!1),
              style: { flex: 1 },
              children: "Cancel"
            }
          ),
          /* @__PURE__ */ t.jsx(
            "button",
            {
              className: "btn btn-primary",
              onClick: j,
              style: { flex: 1 },
              children: "Change Password"
            }
          )
        ] })
      ] })
    ] }) }),
    d && /* @__PURE__ */ t.jsx("div", { className: "modal-overlay", onClick: () => {
      c(null), y();
    }, children: /* @__PURE__ */ t.jsxs("div", { className: "modal", onClick: (S) => S.stopPropagation(), children: [
      /* @__PURE__ */ t.jsx("div", { className: "modal-header", children: /* @__PURE__ */ t.jsx("h2", { className: "modal-title", children: "Password Changed Successfully" }) }),
      /* @__PURE__ */ t.jsxs("div", { className: "modal-body", children: [
        /* @__PURE__ */ t.jsx("p", { children: "Your new password is:" }),
        /* @__PURE__ */ t.jsxs("div", { className: "password-display", style: { display: "flex", alignItems: "center", gap: "var(--spacing-md)", padding: "var(--spacing-md)", background: "var(--color-bg-secondary)", borderRadius: "var(--radius-md)" }, children: [
          /* @__PURE__ */ t.jsx("code", { style: { flex: 1, fontSize: "var(--font-size-base)", fontWeight: "bold" }, children: d }),
          /* @__PURE__ */ t.jsx(
            "button",
            {
              className: "btn btn-sm btn-secondary",
              onClick: () => {
                navigator.clipboard.writeText(d), g(!0), setTimeout(() => g(!1), 2e3);
              },
              children: N ? "✓ Copied!" : "📋 Copy"
            }
          )
        ] }),
        /* @__PURE__ */ t.jsx("p", { className: "warning", style: { marginTop: "var(--spacing-md)", padding: "var(--spacing-md)", background: "var(--color-warning-bg)", border: "1px solid var(--color-warning)", borderRadius: "var(--radius-md)" }, children: "⚠️ Save this password now. You will be logged out." }),
        /* @__PURE__ */ t.jsx(
          "button",
          {
            className: "btn btn-primary",
            onClick: () => {
              c(null), y();
            },
            style: { marginTop: "var(--spacing-lg)", width: "100%" },
            children: "Logout Now"
          }
        )
      ] })
    ] }) })
  ] });
}, Qe = ({ children: s }) => {
  const { user: r, logout: a } = B(), { settings: m, customMenu: d, customActions: c } = he(), { hasFeature: h } = Oe(), u = le(), N = m?.requireAuthentication !== !1, g = async () => {
    await a(), u("/login");
  };
  return /* @__PURE__ */ t.jsxs("div", { className: "main-layout", children: [
    /* @__PURE__ */ t.jsxs("aside", { className: "sidebar", children: [
      /* @__PURE__ */ t.jsx("div", { className: "sidebar-header", children: /* @__PURE__ */ t.jsxs("div", { className: "logo", children: [
        /* @__PURE__ */ t.jsx("div", { className: "logo-icon", children: m?.logoSymbol || "Q" }),
        /* @__PURE__ */ t.jsx("span", { className: "logo-text", children: m?.applicationName || "Quasar" })
      ] }) }),
      /* @__PURE__ */ t.jsxs("nav", { className: "sidebar-nav", children: [
        d.map((n, o) => /* @__PURE__ */ t.jsxs("div", { className: "nav-section", children: [
          n.title && /* @__PURE__ */ t.jsx("h3", { className: "nav-section-title", children: n.title }),
          n.items.map((i, f) => i.roles && !i.roles.some((p) => r?.roles?.includes(p)) || i.feature && !h(i.feature) ? null : /* @__PURE__ */ t.jsx(R, { to: i.path, className: "nav-link", children: /* @__PURE__ */ t.jsx("span", { children: i.label }) }, `custom-item-${f}-${i.path}`))
        ] }, `custom-${o}`)),
        r?.roles?.includes("administrator") && m?.showAdminMenu !== !1 && /* @__PURE__ */ t.jsxs("div", { className: "nav-section", children: [
          /* @__PURE__ */ t.jsx("h3", { className: "nav-section-title", children: "Administration" }),
          /* @__PURE__ */ t.jsx(R, { to: "/users", className: "nav-link", children: /* @__PURE__ */ t.jsx("span", { children: "Users" }) }),
          /* @__PURE__ */ t.jsx(R, { to: "/roles", className: "nav-link", children: /* @__PURE__ */ t.jsx("span", { children: "Roles" }) }),
          /* @__PURE__ */ t.jsx(R, { to: "/features", className: "nav-link", children: /* @__PURE__ */ t.jsx("span", { children: "Features" }) }),
          /* @__PURE__ */ t.jsx(R, { to: "/logs", className: "nav-link", children: /* @__PURE__ */ t.jsx("span", { children: "Logs" }) }),
          /* @__PURE__ */ t.jsx(R, { to: "/sessions", className: "nav-link", children: /* @__PURE__ */ t.jsx("span", { children: "Sessions" }) }),
          h("scheduler") && /* @__PURE__ */ t.jsx(R, { to: "/jobs", className: "nav-link", children: /* @__PURE__ */ t.jsx("span", { children: "Jobs" }) }),
          h("telemetry") && /* @__PURE__ */ t.jsx(R, { to: "/metrics", className: "nav-link", children: /* @__PURE__ */ t.jsx("span", { children: "Metrics" }) })
        ] })
      ] }),
      /* @__PURE__ */ t.jsxs("div", { className: "sidebar-footer", children: [
        c.length > 0 && /* @__PURE__ */ t.jsx("div", { className: "sidebar-actions", children: c.map((n, o) => /* @__PURE__ */ t.jsx(
          "button",
          {
            onClick: n.onClick,
            className: `btn btn-sm w-full ${n.variant === "primary" ? "btn-primary" : n.variant === "danger" ? "btn-danger" : "btn-secondary"}`,
            children: n.label
          },
          o
        )) }),
        N && /* @__PURE__ */ t.jsx("button", { onClick: g, className: "btn btn-secondary w-full btn-sm", children: "Sign Out" })
      ] })
    ] }),
    /* @__PURE__ */ t.jsxs("div", { className: "content-wrapper", children: [
      /* @__PURE__ */ t.jsx(Pe, {}),
      /* @__PURE__ */ t.jsx("main", { className: `main-content ${re?.type?.layoutOptions?.noPadding ? "no-padding" : ""}`, children: s || /* @__PURE__ */ t.jsx(re, {}) })
    ] })
  ] });
};
export {
  $e as AuthProvider,
  Ye as FeatureProvider,
  Pe as Header,
  Qe as MainLayout,
  Ue as NotificationBell,
  Fe as NotificationProvider,
  De as UiProvider,
  B as useAuth,
  Oe as useFeatures,
  Ce as useNotifications,
  he as useUi
};
