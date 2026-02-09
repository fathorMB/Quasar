import C, { createContext as L, useState as k, useEffect as P, useContext as D, useRef as ne } from "react";
import B from "axios";
import * as be from "@microsoft/signalr";
import { useNavigate as oe, useLocation as ye, Link as _, Outlet as ee } from "react-router-dom";
var F = { exports: {} }, U = {};
var te;
function ke() {
  if (te) return U;
  te = 1;
  var s = Symbol.for("react.transitional.element"), r = Symbol.for("react.fragment");
  function o(m, l, n) {
    var h = null;
    if (n !== void 0 && (h = "" + n), l.key !== void 0 && (h = "" + l.key), "key" in l) {
      n = {};
      for (var d in l)
        d !== "key" && (n[d] = l[d]);
    } else n = l;
    return l = n.ref, {
      $$typeof: s,
      type: m,
      key: h,
      ref: l !== void 0 ? l : null,
      props: n
    };
  }
  return U.Fragment = r, U.jsx = o, U.jsxs = o, U;
}
var $ = {};
var se;
function je() {
  return se || (se = 1, process.env.NODE_ENV !== "production" && (function() {
    function s(e) {
      if (e == null) return null;
      if (typeof e == "function")
        return e.$$typeof === we ? null : e.displayName || e.name || null;
      if (typeof e == "string") return e;
      switch (e) {
        case T:
          return "Fragment";
        case O:
          return "Profiler";
        case R:
          return "StrictMode";
        case fe:
          return "Suspense";
        case he:
          return "SuspenseList";
        case ge:
          return "Activity";
      }
      if (typeof e == "object")
        switch (typeof e.tag == "number" && console.error(
          "Received an unexpected object in getComponentNameFromType(). This is likely a bug in React. Please file an issue."
        ), e.$$typeof) {
          case N:
            return "Portal";
          case S:
            return e.displayName || "Context";
          case j:
            return (e._context.displayName || "Context") + ".Consumer";
          case I:
            var c = e.render;
            return e = e.displayName, e || (e = c.displayName || c.name || "", e = e !== "" ? "ForwardRef(" + e + ")" : "ForwardRef"), e;
          case pe:
            return c = e.displayName || null, c !== null ? c : s(e.type) || "Memo";
          case Y:
            c = e._payload, e = e._init;
            try {
              return s(e(c));
            } catch {
            }
        }
      return null;
    }
    function r(e) {
      return "" + e;
    }
    function o(e) {
      try {
        r(e);
        var c = !1;
      } catch {
        c = !0;
      }
      if (c) {
        c = console;
        var g = c.error, v = typeof Symbol == "function" && Symbol.toStringTag && e[Symbol.toStringTag] || e.constructor.name || "Object";
        return g.call(
          c,
          "The provided key is an unsupported type %s. This value must be coerced to a string before using it here.",
          v
        ), r(e);
      }
    }
    function m(e) {
      if (e === T) return "<>";
      if (typeof e == "object" && e !== null && e.$$typeof === Y)
        return "<...>";
      try {
        var c = s(e);
        return c ? "<" + c + ">" : "<...>";
      } catch {
        return "<...>";
      }
    }
    function l() {
      var e = z.A;
      return e === null ? null : e.getOwner();
    }
    function n() {
      return Error("react-stack-top-frame");
    }
    function h(e) {
      if (G.call(e, "key")) {
        var c = Object.getOwnPropertyDescriptor(e, "key").get;
        if (c && c.isReactWarning) return !1;
      }
      return e.key !== void 0;
    }
    function d(e, c) {
      function g() {
        Q || (Q = !0, console.error(
          "%s: `key` is not a prop. Trying to access it will result in `undefined` being returned. If you need to access the same value within the child component, you should pass it as a different prop. (https://react.dev/link/special-props)",
          c
        ));
      }
      g.isReactWarning = !0, Object.defineProperty(e, "key", {
        get: g,
        configurable: !0
      });
    }
    function b() {
      var e = s(this.type);
      return H[e] || (H[e] = !0, console.error(
        "Accessing element.ref was removed in React 19. ref is now a regular prop. It will be removed from the JSX Element type in a future release."
      )), e = this.props.ref, e !== void 0 ? e : null;
    }
    function f(e, c, g, v, M, W) {
      var x = g.ref;
      return e = {
        $$typeof: p,
        type: e,
        key: c,
        props: g,
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
        value: M
      }), Object.defineProperty(e, "_debugTask", {
        configurable: !1,
        enumerable: !1,
        writable: !0,
        value: W
      }), Object.freeze && (Object.freeze(e.props), Object.freeze(e)), e;
    }
    function a(e, c, g, v, M, W) {
      var x = c.children;
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
      if (G.call(c, "key")) {
        x = s(e);
        var A = Object.keys(c).filter(function(xe) {
          return xe !== "key";
        });
        v = 0 < A.length ? "{key: someKey, " + A.join(": ..., ") + ": ...}" : "{key: someKey}", K[x + v] || (A = 0 < A.length ? "{" + A.join(": ..., ") + ": ...}" : "{}", console.error(
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
        ), K[x + v] = !0);
      }
      if (x = null, g !== void 0 && (o(g), x = "" + g), h(c) && (o(c.key), x = "" + c.key), "key" in c) {
        g = {};
        for (var q in c)
          q !== "key" && (g[q] = c[q]);
      } else g = c;
      return x && d(
        g,
        typeof e == "function" ? e.displayName || e.name || "Unknown" : e
      ), f(
        e,
        x,
        g,
        l(),
        M,
        W
      );
    }
    function i(e) {
      u(e) ? e._store && (e._store.validated = 1) : typeof e == "object" && e !== null && e.$$typeof === Y && (e._payload.status === "fulfilled" ? u(e._payload.value) && e._payload.value._store && (e._payload.value._store.validated = 1) : e._store && (e._store.validated = 1));
    }
    function u(e) {
      return typeof e == "object" && e !== null && e.$$typeof === p;
    }
    var w = C, p = Symbol.for("react.transitional.element"), N = Symbol.for("react.portal"), T = Symbol.for("react.fragment"), R = Symbol.for("react.strict_mode"), O = Symbol.for("react.profiler"), j = Symbol.for("react.consumer"), S = Symbol.for("react.context"), I = Symbol.for("react.forward_ref"), fe = Symbol.for("react.suspense"), he = Symbol.for("react.suspense_list"), pe = Symbol.for("react.memo"), Y = Symbol.for("react.lazy"), ge = Symbol.for("react.activity"), we = Symbol.for("react.client.reference"), z = w.__CLIENT_INTERNALS_DO_NOT_USE_OR_WARN_USERS_THEY_CANNOT_UPGRADE, G = Object.prototype.hasOwnProperty, ve = Array.isArray, J = console.createTask ? console.createTask : function() {
      return null;
    };
    w = {
      react_stack_bottom_frame: function(e) {
        return e();
      }
    };
    var Q, H = {}, X = w.react_stack_bottom_frame.bind(
      w,
      n
    )(), Z = J(m(n)), K = {};
    $.Fragment = T, $.jsx = function(e, c, g) {
      var v = 1e4 > z.recentlyCreatedOwnerStacks++;
      return a(
        e,
        c,
        g,
        !1,
        v ? Error("react-stack-top-frame") : X,
        v ? J(m(e)) : Z
      );
    }, $.jsxs = function(e, c, g) {
      var v = 1e4 > z.recentlyCreatedOwnerStacks++;
      return a(
        e,
        c,
        g,
        !0,
        v ? Error("react-stack-top-frame") : X,
        v ? J(m(e)) : Z
      );
    };
  })()), $;
}
var re;
function Ne() {
  return re || (re = 1, process.env.NODE_ENV === "production" ? F.exports = ke() : F.exports = je()), F.exports;
}
var t = Ne();
const y = B.create({
  baseURL: "/auth",
  headers: {
    "Content-Type": "application/json"
  },
  withCredentials: !0
  // Important for cookie-based auth if needed
});
y.interceptors.request.use(
  (s) => {
    const r = localStorage.getItem("accessToken");
    return r && (s.headers.Authorization = `Bearer ${r}`), s;
  },
  (s) => Promise.reject(s)
);
y.interceptors.response.use(
  (s) => s,
  async (s) => {
    const r = s.config;
    if (s.response?.status === 401 && r) {
      const n = localStorage.getItem("refreshToken");
      if (n && !r._retry) {
        r._retry = !0;
        try {
          const h = await B.post("/auth/token/refresh", {
            refreshToken: n
          }), { accessToken: d, refreshToken: b } = h.data;
          return localStorage.setItem("accessToken", d), localStorage.setItem("refreshToken", b), r.headers.Authorization = `Bearer ${d}`, y(r);
        } catch (h) {
          return localStorage.removeItem("accessToken"), localStorage.removeItem("refreshToken"), window.location.href = "/login", Promise.reject(h);
        }
      }
    }
    const o = s.config?.url?.endsWith("/login"), m = s.response?.data;
    if (s.response?.status === 401 && m?.code === "SESSION_REVOKED") {
      localStorage.removeItem("accessToken"), localStorage.removeItem("refreshToken"), window.dispatchEvent(new CustomEvent("session-revoked", {
        detail: { message: m.message }
      }));
      const n = {
        message: m.message,
        statusCode: 401
      };
      return Promise.reject(n);
    }
    const l = {
      message: s.response?.status === 401 && o ? "Invalid username or password" : m?.message || s.message || "An error occurred",
      statusCode: s.response?.status || 500
    };
    return Promise.reject(l);
  }
);
const ae = {
  /**
   * Login with username and password
   */
  login: async (s) => {
    const r = await y.post("/login", s);
    return localStorage.setItem("accessToken", r.data.accessToken), localStorage.setItem("refreshToken", r.data.refreshToken), r.data;
  },
  /**
   * Register a new user
   */
  register: async (s) => (await y.post("/register", s)).data,
  /**
   * Refresh access token
   */
  refreshToken: async (s) => {
    const r = await y.post("/token/refresh", s);
    return localStorage.setItem("accessToken", r.data.accessToken), localStorage.setItem("refreshToken", r.data.refreshToken), r.data;
  },
  /**
   * Logout and revoke refresh token
   */
  logout: async () => {
    const s = localStorage.getItem("refreshToken");
    if (s)
      try {
        await y.post("/logout", { refreshToken: s });
      } catch (r) {
        console.error("Logout error:", r);
      }
    localStorage.removeItem("accessToken"), localStorage.removeItem("refreshToken");
  },
  /**
   * Check if user is authenticated
   */
  isAuthenticated: () => !!localStorage.getItem("accessToken")
}, ie = {
  /**
   * Get all users
   */
  list: async () => (await y.get("/users")).data,
  /**
   * Get roles for a specific user
   */
  getRoles: async (s) => (await y.get(`/users/${s}/roles`)).data,
  /**
   * Get permissions for a specific user
   */
  getPermissions: async (s) => (await y.get(`/users/${s}/permissions`)).data,
  /**
   * Assign a role to a user
   */
  assignRole: async (s, r) => {
    await y.post(`/users/${s}/roles`, r);
  },
  /**
   * Revoke a role from a user
   */
  revokeRole: async (s, r) => {
    await y.delete(`/users/${s}/roles/${r}`);
  },
  /**
   * Reset a user's password (admin only)
   * Returns the new generated password
   */
  resetPassword: async (s) => (await y.post(`/users/${s}/reset-password`)).data.password,
  /**
   * Reset own password (any authenticated user)
   * Returns the new generated password
   */
  resetOwnPassword: async () => (await y.post("/users/me/reset-password")).data.password,
  delete: async (s) => {
    await y.delete(`/users/${s}`);
  }
}, Te = {
  async list() {
    return (await y.get("/api/features", { baseURL: "/" })).data;
  },
  async listDirect() {
    return (await B.get("/api/features", {
      headers: Re(),
      withCredentials: !0
    })).data;
  }
};
function Re() {
  const s = localStorage.getItem("accessToken");
  return s ? { Authorization: `Bearer ${s}` } : {};
}
const ce = L(void 0), V = () => {
  const s = D(ce);
  if (!s)
    throw new Error("useAuth must be used within an AuthProvider");
  return s;
}, Fe = ({ children: s }) => {
  const [r, o] = k(null), [m, l] = k(!0);
  P(() => {
    n();
  }, []);
  const n = async () => {
    try {
      const a = localStorage.getItem("accessToken");
      if (a) {
        const i = h(a);
        console.log("Decoded payload:", i);
        const u = i.sub, w = i.unique_name || i.name || "User";
        let p = [];
        try {
          console.log("Fetching roles for userId:", u);
          const N = await ie.getRoles(u);
          console.log("Fetched userRoles:", N), p = N.map((T) => T.name), console.log("Mapped roles:", p);
        } catch (N) {
          console.error("Failed to fetch roles", N);
        }
        o({
          id: u,
          username: w,
          email: i.email || "",
          roles: p
        });
      } else
        o(null);
    } catch (a) {
      console.error("Auth check failed:", a), o(null);
    } finally {
      l(!1);
    }
  };
  function h(a) {
    var i = a.split(".")[1], u = i.replace(/-/g, "+").replace(/_/g, "/"), w = decodeURIComponent(window.atob(u).split("").map(function(p) {
      return "%" + ("00" + p.charCodeAt(0).toString(16)).slice(-2);
    }).join(""));
    return JSON.parse(w);
  }
  const f = {
    user: r,
    isAuthenticated: !!r,
    isLoading: m,
    login: async (a) => {
      l(!0);
      try {
        await ae.login(a), await n();
      } catch (i) {
        throw o(null), i;
      } finally {
        l(!1);
      }
    },
    logout: async () => {
      l(!0);
      try {
        await ae.logout();
      } catch (a) {
        console.error("Logout error:", a);
      } finally {
        o(null), l(!1);
      }
    }
  };
  return /* @__PURE__ */ t.jsx(ce.Provider, { value: f, children: s });
};
class _e {
  connection = null;
  listeners = [];
  async start() {
    if (!this.connection) {
      this.connection = new be.HubConnectionBuilder().withUrl("/hubs/notifications", {
        accessTokenFactory: () => localStorage.getItem("accessToken") || ""
      }).withAutomaticReconnect().build(), this.connection.on("ReceiveNotification", (r) => {
        this.listeners.forEach((o) => o(r));
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
    return this.listeners.push(r), () => {
      this.listeners = this.listeners.filter((o) => o !== r);
    };
  }
}
const E = new _e(), Ee = async () => {
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
}, Se = async (s) => {
  const r = localStorage.getItem("accessToken");
  if (r)
    try {
      await fetch(`/api/player/notifications/${s}/read`, {
        method: "POST",
        headers: {
          Authorization: `Bearer ${r}`
        }
      });
    } catch (o) {
      console.error("Error marking notification as read:", o);
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
}, le = L(void 0), Le = ({ children: s }) => {
  const [r, o] = k([]), { isAuthenticated: m } = V(), l = r.filter((f) => !f.read).length, n = C.useCallback((f) => {
    const a = {
      ...f,
      id: f.id || Math.random().toString(36).substring(2, 9),
      timestamp: f.createdAt ? new Date(f.createdAt) : /* @__PURE__ */ new Date(),
      read: !1
    };
    o((i) => i.some((w) => w.id === a.id) ? i : [a, ...i].slice(0, 50));
  }, []);
  C.useEffect(() => {
    let f = !0;
    (async () => {
      if (!m) {
        o([]), E.stop().catch(console.error);
        return;
      }
      await E.stop(), await E.start();
      const u = await Ee();
      if (f) {
        const w = u.map((p) => ({
          id: p.id,
          title: p.title,
          message: p.message,
          type: p.type,
          timestamp: new Date(p.createdAt),
          read: p.isRead
        }));
        o((p) => {
          const N = new Set(p.map((R) => R.id));
          return [...w.filter((R) => !N.has(R.id)), ...p].sort((R, O) => O.timestamp.getTime() - R.timestamp.getTime());
        });
      }
    })();
    const i = E.subscribe((u) => {
      console.error("SignalR Notification Received:", u), n({
        id: u.id || u.Id,
        title: u.title || u.Title,
        message: u.message || u.Message,
        type: (u.type || u.Type || "info").toLowerCase(),
        createdAt: u.createdAt || u.CreatedAt
      });
    });
    return () => {
      f = !1, i(), E.stop().catch(console.error);
    };
  }, [m, n]);
  const h = C.useCallback((f) => {
    Se(f), o((a) => a.map((i) => i.id === f ? { ...i, read: !0 } : i));
  }, []), d = C.useCallback(() => {
    Ae(), o((f) => f.map((a) => ({ ...a, read: !0 })));
  }, []), b = C.useCallback(() => {
    o([]);
  }, []);
  return /* @__PURE__ */ t.jsx(le.Provider, { value: {
    notifications: r,
    unreadCount: l,
    addNotification: n,
    markAsRead: h,
    markAllAsRead: d,
    clearNotifications: b
  }, children: s });
}, Ce = () => {
  const s = D(le);
  if (!s)
    throw new Error("useNotifications must be used within a NotificationProvider");
  return s;
}, ue = L(void 0), De = ({ children: s }) => {
  const [r, o] = k(null), [m, l] = k(!0), [n, h] = k([]), [d, b] = k([]);
  return P(() => {
    (async () => {
      try {
        const a = await fetch("/api/config/ui");
        if (a.ok) {
          const i = await a.json();
          o(i);
          const u = `theme-${i.theme || "dark"}`;
          document.documentElement.className = u;
        }
      } catch (a) {
        console.error("Failed to fetch UI settings:", a), document.documentElement.className = "theme-dark";
      } finally {
        l(!1);
      }
    })(), Array.isArray(window.__QUASAR_CUSTOM_MENU__) && h(window.__QUASAR_CUSTOM_MENU__), Array.isArray(window.__QUASAR_CUSTOM_ROUTES__) && b(window.__QUASAR_CUSTOM_ROUTES__);
  }, []), /* @__PURE__ */ t.jsx(ue.Provider, { value: { settings: r, isLoading: m, customMenu: n, customRoutes: d }, children: s });
}, de = () => {
  const s = D(ue);
  if (!s)
    throw new Error("useUi must be used within a UiProvider");
  return s;
}, me = L(void 0), Ye = ({ children: s }) => {
  const [r, o] = k([]), [m, l] = k(!0);
  P(() => {
    (async () => {
      try {
        const d = await Te.list();
        o(d);
      } catch (d) {
        console.error("Failed to fetch features:", d);
      } finally {
        l(!1);
      }
    })();
  }, []);
  const n = (h) => r.some((d) => d.id === h);
  return /* @__PURE__ */ t.jsx(me.Provider, { value: { features: r, isLoading: m, hasFeature: n }, children: s });
}, Pe = () => {
  const s = D(me);
  if (!s)
    throw new Error("useFeatures must be used within a FeatureProvider");
  return s;
}, Oe = () => {
  const { notifications: s, unreadCount: r, addNotification: o, markAsRead: m, markAllAsRead: l } = Ce(), [n, h] = k(!1), d = ne(null);
  P(() => {
    const a = E.subscribe((i) => {
      o({
        title: i.title,
        message: i.message,
        type: i.type
      });
    });
    return E.start(), () => {
      a();
    };
  }, [o]), P(() => {
    const a = (i) => {
      d.current && !d.current.contains(i.target) && h(!1);
    };
    return n && document.addEventListener("mousedown", a), () => {
      document.removeEventListener("mousedown", a);
    };
  }, [n]);
  const b = (a) => {
    const u = (/* @__PURE__ */ new Date()).getTime() - a.getTime(), w = Math.floor(u / 6e4);
    if (w < 1) return "Just now";
    if (w < 60) return `${w}m ago`;
    const p = Math.floor(w / 60);
    return p < 24 ? `${p}h ago` : a.toLocaleDateString();
  }, f = (a) => {
    m(a);
  };
  return /* @__PURE__ */ t.jsxs("div", { className: "notifications-container", ref: d, children: [
    /* @__PURE__ */ t.jsxs(
      "button",
      {
        className: "bell-button",
        onClick: () => h(!n),
        "aria-label": "Notifications",
        children: [
          /* @__PURE__ */ t.jsx("span", { children: "🔔" }),
          r > 0 && /* @__PURE__ */ t.jsx("span", { className: "notification-badge", children: r > 9 ? "9+" : r })
        ]
      }
    ),
    n && /* @__PURE__ */ t.jsxs("div", { className: "notifications-dropdown", children: [
      /* @__PURE__ */ t.jsxs("div", { className: "notifications-header", children: [
        /* @__PURE__ */ t.jsx("h3", { children: "Notifications" }),
        r > 0 && /* @__PURE__ */ t.jsx("button", { className: "mark-all-btn", onClick: l, children: "Mark all as read" })
      ] }),
      /* @__PURE__ */ t.jsx("div", { className: "notifications-list", children: s.length === 0 ? /* @__PURE__ */ t.jsx("div", { className: "empty-notifications", children: "No notifications yet" }) : s.map((a) => /* @__PURE__ */ t.jsxs(
        "div",
        {
          className: `notification-item ${a.read ? "read" : "unread"}`,
          onClick: () => f(a.id),
          children: [
            /* @__PURE__ */ t.jsxs("div", { className: "notification-item-header", children: [
              /* @__PURE__ */ t.jsx("span", { className: `notification-title type-${a.type}`, children: a.title }),
              /* @__PURE__ */ t.jsx("span", { className: "notification-time", children: b(a.timestamp) })
            ] }),
            /* @__PURE__ */ t.jsx("div", { className: "notification-message", children: a.message })
          ]
        },
        a.id
      )) })
    ] })
  ] });
}, Ie = () => {
  const { user: s, logout: r } = V(), [o, m] = k(!1), [l, n] = k(null), [h, d] = k(!1), [b, f] = k(!1), a = oe(), i = ne(null), u = async () => {
    await r(), a("/login");
  }, w = async () => {
    d(!0), m(!1);
  }, p = async () => {
    try {
      const j = await ie.resetOwnPassword();
      n(j), d(!1);
    } catch (j) {
      console.error("Failed to change password:", j), alert("Failed to change password"), d(!1);
    }
  };
  P(() => {
    const j = (S) => {
      i.current && !i.current.contains(S.target) && m(!1);
    };
    return document.addEventListener("mousedown", j), () => {
      document.removeEventListener("mousedown", j);
    };
  }, []);
  const { pathname: N } = ye(), { customMenu: T } = de(), O = (() => {
    const S = (T.length > 0 ? T : window.__QUASAR_CUSTOM_MENU__ || []).flatMap((I) => I.items).find((I) => I.path === N);
    if (S) return S.label;
    switch (N) {
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
  })();
  return /* @__PURE__ */ t.jsxs(t.Fragment, { children: [
    /* @__PURE__ */ t.jsxs("header", { className: "app-header", children: [
      /* @__PURE__ */ t.jsx("div", { className: "header-left", children: /* @__PURE__ */ t.jsx("h2", { className: "page-title", children: O }) }),
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
              onClick: () => m(!o),
              children: /* @__PURE__ */ t.jsx("span", { className: "menu-icon", children: "⋮" })
            }
          ),
          o && /* @__PURE__ */ t.jsxs("div", { className: "dropdown-menu", children: [
            /* @__PURE__ */ t.jsx("button", { onClick: w, className: "dropdown-item", children: "Change Password" }),
            /* @__PURE__ */ t.jsx("button", { onClick: u, className: "dropdown-item danger", children: "Sign Out" })
          ] })
        ] })
      ] })
    ] }),
    h && /* @__PURE__ */ t.jsx("div", { className: "modal-overlay", onClick: () => d(!1), children: /* @__PURE__ */ t.jsxs("div", { className: "modal", onClick: (j) => j.stopPropagation(), children: [
      /* @__PURE__ */ t.jsxs("div", { className: "modal-header", children: [
        /* @__PURE__ */ t.jsx("h2", { className: "modal-title", children: "Confirm Password Change" }),
        /* @__PURE__ */ t.jsx("button", { className: "modal-close", onClick: () => d(!1), children: "×" })
      ] }),
      /* @__PURE__ */ t.jsxs("div", { className: "modal-body", children: [
        /* @__PURE__ */ t.jsx("p", { children: "Generate a new password?" }),
        /* @__PURE__ */ t.jsx("p", { className: "text-muted", style: { marginTop: "var(--spacing-md)" }, children: "You will be logged out and need to login with the new password." }),
        /* @__PURE__ */ t.jsxs("div", { style: { display: "flex", gap: "var(--spacing-md)", marginTop: "var(--spacing-lg)" }, children: [
          /* @__PURE__ */ t.jsx(
            "button",
            {
              className: "btn btn-secondary",
              onClick: () => d(!1),
              style: { flex: 1 },
              children: "Cancel"
            }
          ),
          /* @__PURE__ */ t.jsx(
            "button",
            {
              className: "btn btn-primary",
              onClick: p,
              style: { flex: 1 },
              children: "Change Password"
            }
          )
        ] })
      ] })
    ] }) }),
    l && /* @__PURE__ */ t.jsx("div", { className: "modal-overlay", onClick: () => {
      n(null), u();
    }, children: /* @__PURE__ */ t.jsxs("div", { className: "modal", onClick: (j) => j.stopPropagation(), children: [
      /* @__PURE__ */ t.jsx("div", { className: "modal-header", children: /* @__PURE__ */ t.jsx("h2", { className: "modal-title", children: "Password Changed Successfully" }) }),
      /* @__PURE__ */ t.jsxs("div", { className: "modal-body", children: [
        /* @__PURE__ */ t.jsx("p", { children: "Your new password is:" }),
        /* @__PURE__ */ t.jsxs("div", { className: "password-display", style: { display: "flex", alignItems: "center", gap: "var(--spacing-md)", padding: "var(--spacing-md)", background: "var(--color-bg-secondary)", borderRadius: "var(--radius-md)" }, children: [
          /* @__PURE__ */ t.jsx("code", { style: { flex: 1, fontSize: "var(--font-size-base)", fontWeight: "bold" }, children: l }),
          /* @__PURE__ */ t.jsx(
            "button",
            {
              className: "btn btn-sm btn-secondary",
              onClick: () => {
                navigator.clipboard.writeText(l), f(!0), setTimeout(() => f(!1), 2e3);
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
              n(null), u();
            },
            style: { marginTop: "var(--spacing-lg)", width: "100%" },
            children: "Logout Now"
          }
        )
      ] })
    ] }) })
  ] });
}, ze = ({ children: s }) => {
  const { user: r, logout: o } = V(), { settings: m, customMenu: l } = de(), { hasFeature: n } = Pe(), h = oe(), d = async () => {
    await o(), h("/login");
  };
  return /* @__PURE__ */ t.jsxs("div", { className: "main-layout", children: [
    /* @__PURE__ */ t.jsxs("aside", { className: "sidebar", children: [
      /* @__PURE__ */ t.jsx("div", { className: "sidebar-header", children: /* @__PURE__ */ t.jsxs("div", { className: "logo", children: [
        /* @__PURE__ */ t.jsx("div", { className: "logo-icon", children: m?.logoSymbol || "Q" }),
        /* @__PURE__ */ t.jsx("span", { className: "logo-text", children: m?.applicationName || "Quasar" })
      ] }) }),
      /* @__PURE__ */ t.jsxs("nav", { className: "sidebar-nav", children: [
        l.map((b, f) => /* @__PURE__ */ t.jsxs("div", { className: "nav-section", children: [
          b.title && /* @__PURE__ */ t.jsx("h3", { className: "nav-section-title", children: b.title }),
          b.items.map((a, i) => a.roles && !a.roles.some((u) => r?.roles?.includes(u)) || a.feature && !n(a.feature) ? null : /* @__PURE__ */ t.jsx(_, { to: a.path, className: "nav-link", children: /* @__PURE__ */ t.jsx("span", { children: a.label }) }, `custom-item-${i}-${a.path}`))
        ] }, `custom-${f}`)),
        r?.roles?.includes("administrator") && m?.showAdminMenu !== !1 && /* @__PURE__ */ t.jsxs("div", { className: "nav-section", children: [
          /* @__PURE__ */ t.jsx("h3", { className: "nav-section-title", children: "Administration" }),
          /* @__PURE__ */ t.jsx(_, { to: "/users", className: "nav-link", children: /* @__PURE__ */ t.jsx("span", { children: "Users" }) }),
          /* @__PURE__ */ t.jsx(_, { to: "/roles", className: "nav-link", children: /* @__PURE__ */ t.jsx("span", { children: "Roles" }) }),
          /* @__PURE__ */ t.jsx(_, { to: "/features", className: "nav-link", children: /* @__PURE__ */ t.jsx("span", { children: "Features" }) }),
          /* @__PURE__ */ t.jsx(_, { to: "/logs", className: "nav-link", children: /* @__PURE__ */ t.jsx("span", { children: "Logs" }) }),
          /* @__PURE__ */ t.jsx(_, { to: "/sessions", className: "nav-link", children: /* @__PURE__ */ t.jsx("span", { children: "Sessions" }) }),
          n("scheduler") && /* @__PURE__ */ t.jsx(_, { to: "/jobs", className: "nav-link", children: /* @__PURE__ */ t.jsx("span", { children: "Jobs" }) }),
          n("telemetry") && /* @__PURE__ */ t.jsx(_, { to: "/metrics", className: "nav-link", children: /* @__PURE__ */ t.jsx("span", { children: "Metrics" }) })
        ] })
      ] }),
      /* @__PURE__ */ t.jsx("div", { className: "sidebar-footer", children: /* @__PURE__ */ t.jsx("button", { onClick: d, className: "btn btn-secondary w-full btn-sm", children: "Sign Out" }) })
    ] }),
    /* @__PURE__ */ t.jsxs("div", { className: "content-wrapper", children: [
      /* @__PURE__ */ t.jsx(Ie, {}),
      /* @__PURE__ */ t.jsx("main", { className: `main-content ${ee?.type?.layoutOptions?.noPadding ? "no-padding" : ""}`, children: s || /* @__PURE__ */ t.jsx(ee, {}) })
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
  de as useUi
};
