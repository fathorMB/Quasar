import C, { createContext as D, useState as y, useEffect as P, useContext as Y, useRef as ie } from "react";
import Q from "axios";
import * as be from "@microsoft/signalr";
import { useNavigate as ce, useLocation as ke, Link as T, Outlet as se } from "react-router-dom";
var L = { exports: {} }, I = {};
var re;
function ye() {
  if (re) return I;
  re = 1;
  var s = Symbol.for("react.transitional.element"), r = Symbol.for("react.fragment");
  function a(m, d, o) {
    var g = null;
    if (o !== void 0 && (g = "" + o), d.key !== void 0 && (g = "" + d.key), "key" in d) {
      o = {};
      for (var u in d)
        u !== "key" && (o[u] = d[u]);
    } else o = d;
    return d = o.ref, {
      $$typeof: s,
      type: m,
      key: g,
      ref: d !== void 0 ? d : null,
      props: o
    };
  }
  return I.Fragment = r, I.jsx = a, I.jsxs = a, I;
}
var U = {};
var ne;
function Ne() {
  return ne || (ne = 1, process.env.NODE_ENV !== "production" && (function() {
    function s(e) {
      if (e == null) return null;
      if (typeof e == "function")
        return e.$$typeof === we ? null : e.displayName || e.name || null;
      if (typeof e == "string") return e;
      switch (e) {
        case _:
          return "Fragment";
        case M:
          return "Profiler";
        case R:
          return "StrictMode";
        case E:
          return "Suspense";
        case O:
          return "SuspenseList";
        case ge:
          return "Activity";
      }
      if (typeof e == "object")
        switch (typeof e.tag == "number" && console.error(
          "Received an unexpected object in getComponentNameFromType(). This is likely a bug in React. Please file an issue."
        ), e.$$typeof) {
          case j:
            return "Portal";
          case $:
            return e.displayName || "Context";
          case z:
            return (e._context.displayName || "Context") + ".Consumer";
          case N:
            var l = e.render;
            return e = e.displayName, e || (e = l.displayName || l.name || "", e = e !== "" ? "ForwardRef(" + e + ")" : "ForwardRef"), e;
          case pe:
            return l = e.displayName || null, l !== null ? l : s(e.type) || "Memo";
          case q:
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
        var w = l.error, v = typeof Symbol == "function" && Symbol.toStringTag && e[Symbol.toStringTag] || e.constructor.name || "Object";
        return w.call(
          l,
          "The provided key is an unsupported type %s. This value must be coerced to a string before using it here.",
          v
        ), r(e);
      }
    }
    function m(e) {
      if (e === _) return "<>";
      if (typeof e == "object" && e !== null && e.$$typeof === q)
        return "<...>";
      try {
        var l = s(e);
        return l ? "<" + l + ">" : "<...>";
      } catch {
        return "<...>";
      }
    }
    function d() {
      var e = J.A;
      return e === null ? null : e.getOwner();
    }
    function o() {
      return Error("react-stack-top-frame");
    }
    function g(e) {
      if (G.call(e, "key")) {
        var l = Object.getOwnPropertyDescriptor(e, "key").get;
        if (l && l.isReactWarning) return !1;
      }
      return e.key !== void 0;
    }
    function u(e, l) {
      function w() {
        X || (X = !0, console.error(
          "%s: `key` is not a prop. Trying to access it will result in `undefined` being returned. If you need to access the same value within the child component, you should pass it as a different prop. (https://react.dev/link/special-props)",
          l
        ));
      }
      w.isReactWarning = !0, Object.defineProperty(e, "key", {
        get: w,
        configurable: !0
      });
    }
    function b() {
      var e = s(this.type);
      return Z[e] || (Z[e] = !0, console.error(
        "Accessing element.ref was removed in React 19. ref is now a regular prop. It will be removed from the JSX Element type in a future release."
      )), e = this.props.ref, e !== void 0 ? e : null;
    }
    function h(e, l, w, v, F, H) {
      var x = w.ref;
      return e = {
        $$typeof: p,
        type: e,
        key: l,
        props: w,
        _owner: v
      }, (x !== void 0 ? x : null) !== null ? Object.defineProperty(e, "ref", {
        enumerable: !1,
        get: b
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
        value: H
      }), Object.freeze && (Object.freeze(e.props), Object.freeze(e)), e;
    }
    function n(e, l, w, v, F, H) {
      var x = l.children;
      if (x !== void 0)
        if (v)
          if (ve(x)) {
            for (v = 0; v < x.length; v++)
              i(x[v]);
            Object.freeze && Object.freeze(x);
          } else
            console.error(
              "React.jsx: Static children should always be an array. You are likely explicitly calling React.jsxs or React.jsxDEV. Use the Babel transform instead."
            );
        else i(x);
      if (G.call(l, "key")) {
        x = s(e);
        var A = Object.keys(l).filter(function(xe) {
          return xe !== "key";
        });
        v = 0 < A.length ? "{key: someKey, " + A.join(": ..., ") + ": ...}" : "{key: someKey}", te[x + v] || (A = 0 < A.length ? "{" + A.join(": ..., ") + ": ...}" : "{}", console.error(
          `A props object containing a "key" prop is being spread into JSX:
  let props = %s;
  <%s {...props} />
React keys must be passed directly to JSX without using spread:
  let props = %s;
  <%s key={someKey} {...props} />`,
          v,
          x,
          A,
          x
        ), te[x + v] = !0);
      }
      if (x = null, w !== void 0 && (a(w), x = "" + w), g(l) && (a(l.key), x = "" + l.key), "key" in l) {
        w = {};
        for (var B in l)
          B !== "key" && (w[B] = l[B]);
      } else w = l;
      return x && u(
        w,
        typeof e == "function" ? e.displayName || e.name || "Unknown" : e
      ), h(
        e,
        x,
        w,
        d(),
        F,
        H
      );
    }
    function i(e) {
      c(e) ? e._store && (e._store.validated = 1) : typeof e == "object" && e !== null && e.$$typeof === q && (e._payload.status === "fulfilled" ? c(e._payload.value) && e._payload.value._store && (e._payload.value._store.validated = 1) : e._store && (e._store.validated = 1));
    }
    function c(e) {
      return typeof e == "object" && e !== null && e.$$typeof === p;
    }
    var f = C, p = Symbol.for("react.transitional.element"), j = Symbol.for("react.portal"), _ = Symbol.for("react.fragment"), R = Symbol.for("react.strict_mode"), M = Symbol.for("react.profiler"), z = Symbol.for("react.consumer"), $ = Symbol.for("react.context"), N = Symbol.for("react.forward_ref"), E = Symbol.for("react.suspense"), O = Symbol.for("react.suspense_list"), pe = Symbol.for("react.memo"), q = Symbol.for("react.lazy"), ge = Symbol.for("react.activity"), we = Symbol.for("react.client.reference"), J = f.__CLIENT_INTERNALS_DO_NOT_USE_OR_WARN_USERS_THEY_CANNOT_UPGRADE, G = Object.prototype.hasOwnProperty, ve = Array.isArray, W = console.createTask ? console.createTask : function() {
      return null;
    };
    f = {
      react_stack_bottom_frame: function(e) {
        return e();
      }
    };
    var X, Z = {}, K = f.react_stack_bottom_frame.bind(
      f,
      o
    )(), ee = W(m(o)), te = {};
    U.Fragment = _, U.jsx = function(e, l, w) {
      var v = 1e4 > J.recentlyCreatedOwnerStacks++;
      return n(
        e,
        l,
        w,
        !1,
        v ? Error("react-stack-top-frame") : K,
        v ? W(m(e)) : ee
      );
    }, U.jsxs = function(e, l, w) {
      var v = 1e4 > J.recentlyCreatedOwnerStacks++;
      return n(
        e,
        l,
        w,
        !0,
        v ? Error("react-stack-top-frame") : K,
        v ? W(m(e)) : ee
      );
    };
  })()), U;
}
var ae;
function je() {
  return ae || (ae = 1, process.env.NODE_ENV === "production" ? L.exports = ye() : L.exports = Ne()), L.exports;
}
var t = je();
const k = Q.create({
  baseURL: "/auth",
  headers: {
    "Content-Type": "application/json"
  },
  withCredentials: !0
  // Important for cookie-based auth if needed
});
k.interceptors.request.use(
  (s) => {
    const r = localStorage.getItem("accessToken");
    return r && (s.headers.Authorization = `Bearer ${r}`), s;
  },
  (s) => Promise.reject(s)
);
k.interceptors.response.use(
  (s) => s,
  async (s) => {
    const r = s.config;
    if (s.response?.status === 401 && r) {
      const o = localStorage.getItem("refreshToken");
      if (o && !r._retry) {
        r._retry = !0;
        try {
          const g = await Q.post("/auth/token/refresh", {
            refreshToken: o
          }), { accessToken: u, refreshToken: b } = g.data;
          return localStorage.setItem("accessToken", u), localStorage.setItem("refreshToken", b), r.headers.Authorization = `Bearer ${u}`, k(r);
        } catch (g) {
          return localStorage.removeItem("accessToken"), localStorage.removeItem("refreshToken"), localStorage.getItem("quasar_auth_mocked") === "true" || (window.location.href = "/login"), Promise.reject(g);
        }
      }
    }
    const a = s.config?.url?.endsWith("/login"), m = s.response?.data;
    if (s.response?.status === 401 && m?.code === "SESSION_REVOKED") {
      localStorage.removeItem("accessToken"), localStorage.removeItem("refreshToken"), window.dispatchEvent(new CustomEvent("session-revoked", {
        detail: { message: m.message }
      }));
      const o = {
        message: m.message,
        statusCode: 401
      };
      return Promise.reject(o);
    }
    const d = {
      message: s.response?.status === 401 && a ? "Invalid username or password" : m?.message || s.message || "An error occurred",
      statusCode: s.response?.status || 500
    };
    return Promise.reject(d);
  }
);
const oe = {
  /**
   * Login with username and password
   */
  login: async (s) => {
    const r = await k.post("/login", s);
    return localStorage.setItem("accessToken", r.data.accessToken), localStorage.setItem("refreshToken", r.data.refreshToken), r.data;
  },
  /**
   * Register a new user
   */
  register: async (s) => (await k.post("/register", s)).data,
  /**
   * Refresh access token
   */
  refreshToken: async (s) => {
    const r = await k.post("/token/refresh", s);
    return localStorage.setItem("accessToken", r.data.accessToken), localStorage.setItem("refreshToken", r.data.refreshToken), r.data;
  },
  /**
   * Logout and revoke refresh token
   */
  logout: async () => {
    const s = localStorage.getItem("refreshToken");
    if (s)
      try {
        await k.post("/logout", { refreshToken: s });
      } catch (r) {
        console.error("Logout error:", r);
      }
    localStorage.removeItem("accessToken"), localStorage.removeItem("refreshToken");
  },
  /**
   * Check if user is authenticated
   */
  isAuthenticated: () => !!localStorage.getItem("accessToken")
}, le = {
  /**
   * Get all users
   */
  list: async () => (await k.get("/users")).data,
  /**
   * Get roles for a specific user
   */
  getRoles: async (s) => (await k.get(`/users/${s}/roles`)).data,
  /**
   * Get permissions for a specific user
   */
  getPermissions: async (s) => (await k.get(`/users/${s}/permissions`)).data,
  /**
   * Assign a role to a user
   */
  assignRole: async (s, r) => {
    await k.post(`/users/${s}/roles`, r);
  },
  /**
   * Revoke a role from a user
   */
  revokeRole: async (s, r) => {
    await k.delete(`/users/${s}/roles/${r}`);
  },
  /**
   * Reset a user's password (admin only)
   * Returns the new generated password
   */
  resetPassword: async (s) => (await k.post(`/users/${s}/reset-password`)).data.password,
  /**
   * Reset own password (any authenticated user)
   * Returns the new generated password
   */
  resetOwnPassword: async () => (await k.post("/users/me/reset-password")).data.password,
  delete: async (s) => {
    await k.delete(`/users/${s}`);
  }
}, _e = {
  async list() {
    return (await k.get("/api/features", { baseURL: "/" })).data;
  },
  async listDirect() {
    return (await Q.get("/api/features", {
      headers: Re(),
      withCredentials: !0
    })).data;
  }
};
function Re() {
  const s = localStorage.getItem("accessToken");
  return s ? { Authorization: `Bearer ${s}` } : {};
}
const de = D(void 0), V = () => {
  const s = Y(de);
  if (!s)
    throw new Error("useAuth must be used within an AuthProvider");
  return s;
}, Fe = ({ children: s }) => {
  const [r, a] = y(null), [m, d] = y(!0);
  P(() => {
    let n = !0;
    return (async () => {
      try {
        const c = await fetch("/api/config/ui");
        if (c.ok) {
          const f = await c.json();
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
      } catch (c) {
        console.error("Failed to fetch UI settings for auth check:", c);
      }
      n && await o();
    })(), () => {
      n = !1;
    };
  }, []);
  const o = async () => {
    try {
      const n = localStorage.getItem("accessToken");
      if (n) {
        const i = g(n);
        console.log("Decoded payload:", i);
        const c = i.sub, f = i.unique_name || i.name || "User";
        let p = [];
        try {
          console.log("Fetching roles for userId:", c);
          const j = await le.getRoles(c);
          console.log("Fetched userRoles:", j), p = j.map((_) => _.name), console.log("Mapped roles:", p);
        } catch (j) {
          console.error("Failed to fetch roles", j);
        }
        a({
          id: c,
          username: f,
          email: i.email || "",
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
  function g(n) {
    var i = n.split(".")[1], c = i.replace(/-/g, "+").replace(/_/g, "/"), f = decodeURIComponent(window.atob(c).split("").map(function(p) {
      return "%" + ("00" + p.charCodeAt(0).toString(16)).slice(-2);
    }).join(""));
    return JSON.parse(f);
  }
  const h = {
    user: r,
    isAuthenticated: !!r,
    isLoading: m,
    login: async (n) => {
      d(!0);
      try {
        await oe.login(n), await o();
      } catch (i) {
        throw a(null), i;
      } finally {
        d(!1);
      }
    },
    logout: async () => {
      d(!0);
      try {
        await oe.logout();
      } catch (n) {
        console.error("Logout error:", n);
      } finally {
        a(null), d(!1);
      }
    }
  };
  return /* @__PURE__ */ t.jsx(de.Provider, { value: h, children: s });
};
class Te {
  connection = null;
  listeners = [];
  recentIds = /* @__PURE__ */ new Set();
  async start() {
    if (!this.connection) {
      this.connection = new be.HubConnectionBuilder().withUrl("/hubs/notifications", {
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
const S = new Te(), Se = async () => {
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
}, Ee = async (s) => {
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
}, Ae = async () => {
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
}, ue = D(void 0), Le = ({ children: s }) => {
  const [r, a] = y([]), { isAuthenticated: m } = V(), d = r.filter((h) => !h.read).length, o = C.useCallback((h) => {
    const n = {
      ...h,
      id: h.id || Math.random().toString(36).substring(2, 9),
      timestamp: h.createdAt ? new Date(h.createdAt) : /* @__PURE__ */ new Date(),
      read: !1
    };
    console.log("[Notification] addNotification called, id:", n.id, "title:", n.title), a((i) => {
      const c = i.some((f) => f.id === n.id);
      return console.log("[Notification] setNotifications: exists=", c, "prevCount=", i.length, "prevIds=", i.map((f) => f.id)), c ? i : [n, ...i].slice(0, 50);
    });
  }, []);
  C.useEffect(() => {
    let h = !0;
    (async () => {
      if (!m) {
        a([]), S.stop().catch(console.error);
        return;
      }
      await S.stop(), await S.start();
      const c = await Se();
      if (console.log("[Notification] fetchUnread returned", c.length, "items, ids:", c.map((f) => f.id)), h) {
        const f = c.map((p) => ({
          id: p.id,
          title: p.title,
          message: p.message,
          type: p.type,
          timestamp: new Date(p.createdAt),
          read: p.isRead
        }));
        a((p) => {
          const j = new Set(p.map((R) => R.id)), _ = f.filter((R) => !j.has(R.id));
          return console.log("[Notification] fetchUnread merge: existingIds=", [...j], "newItems=", _.length), [..._, ...p].sort((R, M) => M.timestamp.getTime() - R.timestamp.getTime());
        });
      }
    })();
    const i = S.subscribe((c) => {
      o({
        id: c.id,
        title: c.title,
        message: c.message,
        type: c.type,
        createdAt: c.createdAt
      });
    });
    return () => {
      h = !1, i(), S.stop().catch(console.error);
    };
  }, [m, o]);
  const g = C.useCallback((h) => {
    Ee(h), a((n) => n.map((i) => i.id === h ? { ...i, read: !0 } : i));
  }, []), u = C.useCallback(() => {
    Ae(), a((h) => h.map((n) => ({ ...n, read: !0 })));
  }, []), b = C.useCallback(() => {
    a([]);
  }, []);
  return /* @__PURE__ */ t.jsx(ue.Provider, { value: {
    notifications: r,
    unreadCount: d,
    addNotification: o,
    markAsRead: g,
    markAllAsRead: u,
    clearNotifications: b
  }, children: s });
}, Ce = () => {
  const s = Y(ue);
  if (!s)
    throw new Error("useNotifications must be used within a NotificationProvider");
  return s;
}, me = D(void 0), De = ({ children: s }) => {
  const [r, a] = y(null), [m, d] = y(!0), [o, g] = y([]), [u, b] = y([]), [h, n] = y(null);
  return P(() => {
    (async () => {
      try {
        const c = await fetch("/api/config/ui");
        if (c.ok) {
          const f = await c.json();
          a(f);
          const p = `theme-${f.theme || "dark"}`;
          document.documentElement.className = p;
        }
      } catch (c) {
        console.error("Failed to fetch UI settings:", c), document.documentElement.className = "theme-dark";
      } finally {
        d(!1);
      }
    })(), Array.isArray(window.__QUASAR_CUSTOM_MENU__) && g(window.__QUASAR_CUSTOM_MENU__), Array.isArray(window.__QUASAR_CUSTOM_ROUTES__) && b(window.__QUASAR_CUSTOM_ROUTES__), window.__QUASAR_CUSTOM_HEADER__ && n(() => window.__QUASAR_CUSTOM_HEADER__);
  }, []), /* @__PURE__ */ t.jsx(me.Provider, { value: { settings: r, isLoading: m, customMenu: o, customRoutes: u, customHeaderComponent: h }, children: s });
}, fe = () => {
  const s = Y(me);
  if (!s)
    throw new Error("useUi must be used within a UiProvider");
  return s;
}, he = D(void 0), Ye = ({ children: s }) => {
  const [r, a] = y([]), [m, d] = y(!0);
  P(() => {
    (async () => {
      try {
        const u = await _e.list();
        a(u);
      } catch (u) {
        console.error("Failed to fetch features:", u);
      } finally {
        d(!1);
      }
    })();
  }, []);
  const o = (g) => r.some((u) => u.id === g);
  return /* @__PURE__ */ t.jsx(he.Provider, { value: { features: r, isLoading: m, hasFeature: o }, children: s });
}, Pe = () => {
  const s = Y(he);
  if (!s)
    throw new Error("useFeatures must be used within a FeatureProvider");
  return s;
}, Oe = () => {
  const { notifications: s, unreadCount: r, addNotification: a, markAsRead: m, markAllAsRead: d } = Ce(), [o, g] = y(!1), u = ie(null);
  P(() => {
    const n = S.subscribe((i) => {
      a({
        title: i.title,
        message: i.message,
        type: i.type
      });
    });
    return S.start(), () => {
      n();
    };
  }, [a]), P(() => {
    const n = (i) => {
      u.current && !u.current.contains(i.target) && g(!1);
    };
    return o && document.addEventListener("mousedown", n), () => {
      document.removeEventListener("mousedown", n);
    };
  }, [o]);
  const b = (n) => {
    const c = (/* @__PURE__ */ new Date()).getTime() - n.getTime(), f = Math.floor(c / 6e4);
    if (f < 1) return "Just now";
    if (f < 60) return `${f}m ago`;
    const p = Math.floor(f / 60);
    return p < 24 ? `${p}h ago` : n.toLocaleDateString();
  }, h = (n) => {
    m(n);
  };
  return /* @__PURE__ */ t.jsxs("div", { className: "notifications-container", ref: u, children: [
    /* @__PURE__ */ t.jsxs(
      "button",
      {
        className: "bell-button",
        onClick: () => g(!o),
        "aria-label": "Notifications",
        children: [
          /* @__PURE__ */ t.jsx("span", { children: "🔔" }),
          r > 0 && /* @__PURE__ */ t.jsx("span", { className: "notification-badge", children: r > 9 ? "9+" : r })
        ]
      }
    ),
    o && /* @__PURE__ */ t.jsxs("div", { className: "notifications-dropdown", children: [
      /* @__PURE__ */ t.jsxs("div", { className: "notifications-header", children: [
        /* @__PURE__ */ t.jsx("h3", { children: "Notifications" }),
        r > 0 && /* @__PURE__ */ t.jsx("button", { className: "mark-all-btn", onClick: d, children: "Mark all as read" })
      ] }),
      /* @__PURE__ */ t.jsx("div", { className: "notifications-list", children: s.length === 0 ? /* @__PURE__ */ t.jsx("div", { className: "empty-notifications", children: "No notifications yet" }) : s.map((n) => /* @__PURE__ */ t.jsxs(
        "div",
        {
          className: `notification-item ${n.read ? "read" : "unread"}`,
          onClick: () => h(n.id),
          children: [
            /* @__PURE__ */ t.jsxs("div", { className: "notification-item-header", children: [
              /* @__PURE__ */ t.jsx("span", { className: `notification-title type-${n.type}`, children: n.title }),
              /* @__PURE__ */ t.jsx("span", { className: "notification-time", children: b(n.timestamp) })
            ] }),
            /* @__PURE__ */ t.jsx("div", { className: "notification-message", children: n.message })
          ]
        },
        n.id
      )) })
    ] })
  ] });
}, Ie = () => {
  const { user: s, logout: r } = V(), [a, m] = y(!1), [d, o] = y(null), [g, u] = y(!1), [b, h] = y(!1), n = ce(), i = ie(null), { customMenu: c, customHeaderComponent: f } = fe(), p = async () => {
    await r(), n("/login");
  }, j = async () => {
    u(!0), m(!1);
  }, _ = async () => {
    try {
      const N = await le.resetOwnPassword();
      o(N), u(!1);
    } catch (N) {
      console.error("Failed to change password:", N), alert("Failed to change password"), u(!1);
    }
  };
  P(() => {
    const N = (E) => {
      i.current && !i.current.contains(E.target) && m(!1);
    };
    return document.addEventListener("mousedown", N), () => {
      document.removeEventListener("mousedown", N);
    };
  }, []);
  const { pathname: R } = ke(), z = (() => {
    const E = (c.length > 0 ? c : window.__QUASAR_CUSTOM_MENU__ || []).flatMap((O) => O.items).find((O) => O.path === R);
    if (E) return E.label;
    switch (R) {
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
      /* @__PURE__ */ t.jsx("div", { className: "header-left", children: $ ? /* @__PURE__ */ t.jsx($, {}) : /* @__PURE__ */ t.jsx("h2", { className: "page-title", children: z }) }),
      /* @__PURE__ */ t.jsxs("div", { className: "header-right", children: [
        /* @__PURE__ */ t.jsx(Oe, {}),
        /* @__PURE__ */ t.jsxs("div", { className: "user-profile", children: [
          /* @__PURE__ */ t.jsxs("div", { className: "user-info", children: [
            /* @__PURE__ */ t.jsx("span", { className: "user-name", children: s?.username || "Guest" }),
            /* @__PURE__ */ t.jsx("span", { className: "user-role", children: s?.roles?.[0] || "User" })
          ] }),
          /* @__PURE__ */ t.jsx("div", { className: "user-avatar", children: s?.username?.charAt(0).toUpperCase() || "U" })
        ] }),
        /* @__PURE__ */ t.jsxs("div", { className: "menu-container", ref: i, children: [
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
            /* @__PURE__ */ t.jsx("button", { onClick: j, className: "dropdown-item", children: "Change Password" }),
            /* @__PURE__ */ t.jsx("button", { onClick: p, className: "dropdown-item danger", children: "Sign Out" })
          ] })
        ] })
      ] })
    ] }),
    g && /* @__PURE__ */ t.jsx("div", { className: "modal-overlay", onClick: () => u(!1), children: /* @__PURE__ */ t.jsxs("div", { className: "modal", onClick: (N) => N.stopPropagation(), children: [
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
              onClick: _,
              style: { flex: 1 },
              children: "Change Password"
            }
          )
        ] })
      ] })
    ] }) }),
    d && /* @__PURE__ */ t.jsx("div", { className: "modal-overlay", onClick: () => {
      o(null), p();
    }, children: /* @__PURE__ */ t.jsxs("div", { className: "modal", onClick: (N) => N.stopPropagation(), children: [
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
                navigator.clipboard.writeText(d), h(!0), setTimeout(() => h(!1), 2e3);
              },
              children: b ? "✓ Copied!" : "📋 Copy"
            }
          )
        ] }),
        /* @__PURE__ */ t.jsx("p", { className: "warning", style: { marginTop: "var(--spacing-md)", padding: "var(--spacing-md)", background: "var(--color-warning-bg)", border: "1px solid var(--color-warning)", borderRadius: "var(--radius-md)" }, children: "⚠️ Save this password now. You will be logged out." }),
        /* @__PURE__ */ t.jsx(
          "button",
          {
            className: "btn btn-primary",
            onClick: () => {
              o(null), p();
            },
            style: { marginTop: "var(--spacing-lg)", width: "100%" },
            children: "Logout Now"
          }
        )
      ] })
    ] }) })
  ] });
}, ze = ({ children: s }) => {
  const { user: r, logout: a } = V(), { settings: m, customMenu: d } = fe(), { hasFeature: o } = Pe(), g = ce(), u = async () => {
    await a(), g("/login");
  };
  return /* @__PURE__ */ t.jsxs("div", { className: "main-layout", children: [
    /* @__PURE__ */ t.jsxs("aside", { className: "sidebar", children: [
      /* @__PURE__ */ t.jsx("div", { className: "sidebar-header", children: /* @__PURE__ */ t.jsxs("div", { className: "logo", children: [
        /* @__PURE__ */ t.jsx("div", { className: "logo-icon", children: m?.logoSymbol || "Q" }),
        /* @__PURE__ */ t.jsx("span", { className: "logo-text", children: m?.applicationName || "Quasar" })
      ] }) }),
      /* @__PURE__ */ t.jsxs("nav", { className: "sidebar-nav", children: [
        d.map((b, h) => /* @__PURE__ */ t.jsxs("div", { className: "nav-section", children: [
          b.title && /* @__PURE__ */ t.jsx("h3", { className: "nav-section-title", children: b.title }),
          b.items.map((n, i) => n.roles && !n.roles.some((c) => r?.roles?.includes(c)) || n.feature && !o(n.feature) ? null : /* @__PURE__ */ t.jsx(T, { to: n.path, className: "nav-link", children: /* @__PURE__ */ t.jsx("span", { children: n.label }) }, `custom-item-${i}-${n.path}`))
        ] }, `custom-${h}`)),
        r?.roles?.includes("administrator") && m?.showAdminMenu !== !1 && /* @__PURE__ */ t.jsxs("div", { className: "nav-section", children: [
          /* @__PURE__ */ t.jsx("h3", { className: "nav-section-title", children: "Administration" }),
          /* @__PURE__ */ t.jsx(T, { to: "/users", className: "nav-link", children: /* @__PURE__ */ t.jsx("span", { children: "Users" }) }),
          /* @__PURE__ */ t.jsx(T, { to: "/roles", className: "nav-link", children: /* @__PURE__ */ t.jsx("span", { children: "Roles" }) }),
          /* @__PURE__ */ t.jsx(T, { to: "/features", className: "nav-link", children: /* @__PURE__ */ t.jsx("span", { children: "Features" }) }),
          /* @__PURE__ */ t.jsx(T, { to: "/logs", className: "nav-link", children: /* @__PURE__ */ t.jsx("span", { children: "Logs" }) }),
          /* @__PURE__ */ t.jsx(T, { to: "/sessions", className: "nav-link", children: /* @__PURE__ */ t.jsx("span", { children: "Sessions" }) }),
          o("scheduler") && /* @__PURE__ */ t.jsx(T, { to: "/jobs", className: "nav-link", children: /* @__PURE__ */ t.jsx("span", { children: "Jobs" }) }),
          o("telemetry") && /* @__PURE__ */ t.jsx(T, { to: "/metrics", className: "nav-link", children: /* @__PURE__ */ t.jsx("span", { children: "Metrics" }) })
        ] })
      ] }),
      /* @__PURE__ */ t.jsx("div", { className: "sidebar-footer", children: /* @__PURE__ */ t.jsx("button", { onClick: u, className: "btn btn-secondary w-full btn-sm", children: "Sign Out" }) })
    ] }),
    /* @__PURE__ */ t.jsxs("div", { className: "content-wrapper", children: [
      /* @__PURE__ */ t.jsx(Ie, {}),
      /* @__PURE__ */ t.jsx("main", { className: `main-content ${se?.type?.layoutOptions?.noPadding ? "no-padding" : ""}`, children: s || /* @__PURE__ */ t.jsx(se, {}) })
    ] })
  ] });
};
export {
  Fe as AuthProvider,
  Ye as FeatureProvider,
  Ie as Header,
  ze as MainLayout,
  Oe as NotificationBell,
  Le as NotificationProvider,
  De as UiProvider,
  V as useAuth,
  Pe as useFeatures,
  Ce as useNotifications,
  fe as useUi
};
