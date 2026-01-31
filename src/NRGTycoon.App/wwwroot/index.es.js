import xe, { createContext as U, useState as j, useContext as M, useEffect as S, useRef as re } from "react";
import W from "axios";
import * as be from "@microsoft/signalr";
import { useNavigate as ne, useLocation as je, Link as R, Outlet as X } from "react-router-dom";
var I = { exports: {} }, A = {};
var Z;
function Ne() {
  if (Z) return A;
  Z = 1;
  var t = Symbol.for("react.transitional.element"), r = Symbol.for("react.fragment");
  function i(u, c, o) {
    var m = null;
    if (o !== void 0 && (m = "" + o), c.key !== void 0 && (m = "" + c.key), "key" in c) {
      o = {};
      for (var l in c)
        l !== "key" && (o[l] = c[l]);
    } else o = c;
    return c = o.ref, {
      $$typeof: t,
      type: u,
      key: m,
      ref: c !== void 0 ? c : null,
      props: o
    };
  }
  return A.Fragment = r, A.jsx = i, A.jsxs = i, A;
}
var P = {};
var K;
function ke() {
  return K || (K = 1, process.env.NODE_ENV !== "production" && (function() {
    function t(e) {
      if (e == null) return null;
      if (typeof e == "function")
        return e.$$typeof === ge ? null : e.displayName || e.name || null;
      if (typeof e == "string") return e;
      switch (e) {
        case _:
          return "Fragment";
        case $:
          return "Profiler";
        case q:
          return "StrictMode";
        case me:
          return "Suspense";
        case fe:
          return "SuspenseList";
        case pe:
          return "Activity";
      }
      if (typeof e == "object")
        switch (typeof e.tag == "number" && console.error(
          "Received an unexpected object in getComponentNameFromType(). This is likely a bug in React. Please file an issue."
        ), e.$$typeof) {
          case y:
            return "Portal";
          case T:
            return e.displayName || "Context";
          case k:
            return (e._context.displayName || "Context") + ".Consumer";
          case C:
            var a = e.render;
            return e = e.displayName, e || (e = a.displayName || a.name || "", e = e !== "" ? "ForwardRef(" + e + ")" : "ForwardRef"), e;
          case he:
            return a = e.displayName || null, a !== null ? a : t(e.type) || "Memo";
          case L:
            a = e._payload, e = e._init;
            try {
              return t(e(a));
            } catch {
            }
        }
      return null;
    }
    function r(e) {
      return "" + e;
    }
    function i(e) {
      try {
        r(e);
        var a = !1;
      } catch {
        a = !0;
      }
      if (a) {
        a = console;
        var h = a.error, p = typeof Symbol == "function" && Symbol.toStringTag && e[Symbol.toStringTag] || e.constructor.name || "Object";
        return h.call(
          a,
          "The provided key is an unsupported type %s. This value must be coerced to a string before using it here.",
          p
        ), r(e);
      }
    }
    function u(e) {
      if (e === _) return "<>";
      if (typeof e == "object" && e !== null && e.$$typeof === L)
        return "<...>";
      try {
        var a = t(e);
        return a ? "<" + a + ">" : "<...>";
      } catch {
        return "<...>";
      }
    }
    function c() {
      var e = F.A;
      return e === null ? null : e.getOwner();
    }
    function o() {
      return Error("react-stack-top-frame");
    }
    function m(e) {
      if (z.call(e, "key")) {
        var a = Object.getOwnPropertyDescriptor(e, "key").get;
        if (a && a.isReactWarning) return !1;
      }
      return e.key !== void 0;
    }
    function l(e, a) {
      function h() {
        V || (V = !0, console.error(
          "%s: `key` is not a prop. Trying to access it will result in `undefined` being returned. If you need to access the same value within the child component, you should pass it as a different prop. (https://react.dev/link/special-props)",
          a
        ));
      }
      h.isReactWarning = !0, Object.defineProperty(e, "key", {
        get: h,
        configurable: !0
      });
    }
    function f() {
      var e = t(this.type);
      return G[e] || (G[e] = !0, console.error(
        "Accessing element.ref was removed in React 19. ref is now a regular prop. It will be removed from the JSX Element type in a future release."
      )), e = this.props.ref, e !== void 0 ? e : null;
    }
    function w(e, a, h, p, O, D) {
      var g = h.ref;
      return e = {
        $$typeof: N,
        type: e,
        key: a,
        props: h,
        _owner: p
      }, (g !== void 0 ? g : null) !== null ? Object.defineProperty(e, "ref", {
        enumerable: !1,
        get: f
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
        value: O
      }), Object.defineProperty(e, "_debugTask", {
        configurable: !1,
        enumerable: !1,
        writable: !0,
        value: D
      }), Object.freeze && (Object.freeze(e.props), Object.freeze(e)), e;
    }
    function n(e, a, h, p, O, D) {
      var g = a.children;
      if (g !== void 0)
        if (p)
          if (ve(g)) {
            for (p = 0; p < g.length; p++)
              d(g[p]);
            Object.freeze && Object.freeze(g);
          } else
            console.error(
              "React.jsx: Static children should always be an array. You are likely explicitly calling React.jsxs or React.jsxDEV. Use the Babel transform instead."
            );
        else d(g);
      if (z.call(a, "key")) {
        g = t(e);
        var E = Object.keys(a).filter(function(we) {
          return we !== "key";
        });
        p = 0 < E.length ? "{key: someKey, " + E.join(": ..., ") + ": ...}" : "{key: someKey}", H[g + p] || (E = 0 < E.length ? "{" + E.join(": ..., ") + ": ...}" : "{}", console.error(
          `A props object containing a "key" prop is being spread into JSX:
  let props = %s;
  <%s {...props} />
React keys must be passed directly to JSX without using spread:
  let props = %s;
  <%s key={someKey} {...props} />`,
          p,
          g,
          E,
          g
        ), H[g + p] = !0);
      }
      if (g = null, h !== void 0 && (i(h), g = "" + h), m(a) && (i(a.key), g = "" + a.key), "key" in a) {
        h = {};
        for (var J in a)
          J !== "key" && (h[J] = a[J]);
      } else h = a;
      return g && l(
        h,
        typeof e == "function" ? e.displayName || e.name || "Unknown" : e
      ), w(
        e,
        g,
        h,
        c(),
        O,
        D
      );
    }
    function d(e) {
      v(e) ? e._store && (e._store.validated = 1) : typeof e == "object" && e !== null && e.$$typeof === L && (e._payload.status === "fulfilled" ? v(e._payload.value) && e._payload.value._store && (e._payload.value._store.validated = 1) : e._store && (e._store.validated = 1));
    }
    function v(e) {
      return typeof e == "object" && e !== null && e.$$typeof === N;
    }
    var b = xe, N = Symbol.for("react.transitional.element"), y = Symbol.for("react.portal"), _ = Symbol.for("react.fragment"), q = Symbol.for("react.strict_mode"), $ = Symbol.for("react.profiler"), k = Symbol.for("react.consumer"), T = Symbol.for("react.context"), C = Symbol.for("react.forward_ref"), me = Symbol.for("react.suspense"), fe = Symbol.for("react.suspense_list"), he = Symbol.for("react.memo"), L = Symbol.for("react.lazy"), pe = Symbol.for("react.activity"), ge = Symbol.for("react.client.reference"), F = b.__CLIENT_INTERNALS_DO_NOT_USE_OR_WARN_USERS_THEY_CANNOT_UPGRADE, z = Object.prototype.hasOwnProperty, ve = Array.isArray, Y = console.createTask ? console.createTask : function() {
      return null;
    };
    b = {
      react_stack_bottom_frame: function(e) {
        return e();
      }
    };
    var V, G = {}, Q = b.react_stack_bottom_frame.bind(
      b,
      o
    )(), B = Y(u(o)), H = {};
    P.Fragment = _, P.jsx = function(e, a, h) {
      var p = 1e4 > F.recentlyCreatedOwnerStacks++;
      return n(
        e,
        a,
        h,
        !1,
        p ? Error("react-stack-top-frame") : Q,
        p ? Y(u(e)) : B
      );
    }, P.jsxs = function(e, a, h) {
      var p = 1e4 > F.recentlyCreatedOwnerStacks++;
      return n(
        e,
        a,
        h,
        !0,
        p ? Error("react-stack-top-frame") : Q,
        p ? Y(u(e)) : B
      );
    };
  })()), P;
}
var ee;
function ye() {
  return ee || (ee = 1, process.env.NODE_ENV === "production" ? I.exports = Ne() : I.exports = ke()), I.exports;
}
var s = ye();
const ae = U(void 0), Ue = ({ children: t }) => {
  const [r, i] = j([]), u = r.filter((f) => !f.read).length, c = (f) => {
    const w = {
      ...f,
      id: Math.random().toString(36).substring(2, 9),
      timestamp: /* @__PURE__ */ new Date(),
      read: !1
    };
    i((n) => [w, ...n].slice(0, 50));
  }, o = (f) => {
    i((w) => w.map((n) => n.id === f ? { ...n, read: !0 } : n));
  }, m = () => {
    i((f) => f.map((w) => ({ ...w, read: !0 })));
  }, l = () => {
    i([]);
  };
  return /* @__PURE__ */ s.jsx(ae.Provider, { value: {
    notifications: r,
    unreadCount: u,
    addNotification: c,
    markAsRead: o,
    markAllAsRead: m,
    clearNotifications: l
  }, children: t });
}, _e = () => {
  const t = M(ae);
  if (!t)
    throw new Error("useNotifications must be used within a NotificationProvider");
  return t;
}, x = W.create({
  baseURL: "/auth",
  headers: {
    "Content-Type": "application/json"
  },
  withCredentials: !0
  // Important for cookie-based auth if needed
});
x.interceptors.request.use(
  (t) => {
    const r = localStorage.getItem("accessToken");
    return r && (t.headers.Authorization = `Bearer ${r}`), t;
  },
  (t) => Promise.reject(t)
);
x.interceptors.response.use(
  (t) => t,
  async (t) => {
    const r = t.config;
    if (t.response?.status === 401 && r) {
      const o = localStorage.getItem("refreshToken");
      if (o && !r._retry) {
        r._retry = !0;
        try {
          const m = await W.post("/auth/token/refresh", {
            refreshToken: o
          }), { accessToken: l, refreshToken: f } = m.data;
          return localStorage.setItem("accessToken", l), localStorage.setItem("refreshToken", f), r.headers.Authorization = `Bearer ${l}`, x(r);
        } catch (m) {
          return localStorage.removeItem("accessToken"), localStorage.removeItem("refreshToken"), window.location.href = "/login", Promise.reject(m);
        }
      }
    }
    const i = t.config?.url?.endsWith("/login"), u = t.response?.data;
    if (t.response?.status === 401 && u?.code === "SESSION_REVOKED") {
      localStorage.removeItem("accessToken"), localStorage.removeItem("refreshToken"), window.dispatchEvent(new CustomEvent("session-revoked", {
        detail: { message: u.message }
      }));
      const o = {
        message: u.message,
        statusCode: 401
      };
      return Promise.reject(o);
    }
    const c = {
      message: t.response?.status === 401 && i ? "Invalid username or password" : u?.message || t.message || "An error occurred",
      statusCode: t.response?.status || 500
    };
    return Promise.reject(c);
  }
);
const se = {
  /**
   * Login with username and password
   */
  login: async (t) => {
    const r = await x.post("/login", t);
    return localStorage.setItem("accessToken", r.data.accessToken), localStorage.setItem("refreshToken", r.data.refreshToken), r.data;
  },
  /**
   * Register a new user
   */
  register: async (t) => (await x.post("/register", t)).data,
  /**
   * Refresh access token
   */
  refreshToken: async (t) => {
    const r = await x.post("/token/refresh", t);
    return localStorage.setItem("accessToken", r.data.accessToken), localStorage.setItem("refreshToken", r.data.refreshToken), r.data;
  },
  /**
   * Logout and revoke refresh token
   */
  logout: async () => {
    const t = localStorage.getItem("refreshToken");
    if (t)
      try {
        await x.post("/logout", { refreshToken: t });
      } catch (r) {
        console.error("Logout error:", r);
      }
    localStorage.removeItem("accessToken"), localStorage.removeItem("refreshToken");
  },
  /**
   * Check if user is authenticated
   */
  isAuthenticated: () => !!localStorage.getItem("accessToken")
}, oe = {
  /**
   * Get all users
   */
  list: async () => (await x.get("/users")).data,
  /**
   * Get roles for a specific user
   */
  getRoles: async (t) => (await x.get(`/users/${t}/roles`)).data,
  /**
   * Get permissions for a specific user
   */
  getPermissions: async (t) => (await x.get(`/users/${t}/permissions`)).data,
  /**
   * Assign a role to a user
   */
  assignRole: async (t, r) => {
    await x.post(`/users/${t}/roles`, r);
  },
  /**
   * Revoke a role from a user
   */
  revokeRole: async (t, r) => {
    await x.delete(`/users/${t}/roles/${r}`);
  },
  /**
   * Reset a user's password (admin only)
   * Returns the new generated password
   */
  resetPassword: async (t) => (await x.post(`/users/${t}/reset-password`)).data.password,
  /**
   * Reset own password (any authenticated user)
   * Returns the new generated password
   */
  resetOwnPassword: async () => (await x.post("/users/me/reset-password")).data.password,
  delete: async (t) => {
    await x.delete(`/users/${t}`);
  }
}, Re = {
  async list() {
    return (await x.get("/api/features", { baseURL: "/" })).data;
  },
  async listDirect() {
    return (await W.get("/api/features", {
      headers: Te(),
      withCredentials: !0
    })).data;
  }
};
function Te() {
  const t = localStorage.getItem("accessToken");
  return t ? { Authorization: `Bearer ${t}` } : {};
}
const ie = U(void 0), ce = () => {
  const t = M(ie);
  if (!t)
    throw new Error("useAuth must be used within an AuthProvider");
  return t;
}, Me = ({ children: t }) => {
  const [r, i] = j(null), [u, c] = j(!0);
  S(() => {
    o();
  }, []);
  const o = async () => {
    try {
      const n = localStorage.getItem("accessToken");
      if (n) {
        const d = m(n);
        console.log("Decoded payload:", d);
        const v = d.sub, b = d.unique_name || d.name || "User";
        let N = [];
        try {
          console.log("Fetching roles for userId:", v);
          const y = await oe.getRoles(v);
          console.log("Fetched userRoles:", y), N = y.map((_) => _.name), console.log("Mapped roles:", N);
        } catch (y) {
          console.error("Failed to fetch roles", y);
        }
        i({
          id: v,
          username: b,
          email: d.email || "",
          roles: N
        });
      } else
        i(null);
    } catch (n) {
      console.error("Auth check failed:", n), i(null);
    } finally {
      c(!1);
    }
  };
  function m(n) {
    var d = n.split(".")[1], v = d.replace(/-/g, "+").replace(/_/g, "/"), b = decodeURIComponent(window.atob(v).split("").map(function(N) {
      return "%" + ("00" + N.charCodeAt(0).toString(16)).slice(-2);
    }).join(""));
    return JSON.parse(b);
  }
  const w = {
    user: r,
    isAuthenticated: !!r,
    isLoading: u,
    login: async (n) => {
      c(!0);
      try {
        await se.login(n), await o();
      } catch (d) {
        throw i(null), d;
      } finally {
        c(!1);
      }
    },
    logout: async () => {
      c(!0);
      try {
        await se.logout();
      } catch (n) {
        console.error("Logout error:", n);
      } finally {
        i(null), c(!1);
      }
    }
  };
  return /* @__PURE__ */ s.jsx(ie.Provider, { value: w, children: t });
}, le = U(void 0), $e = ({ children: t }) => {
  const [r, i] = j(null), [u, c] = j(!0), [o, m] = j([]), [l, f] = j([]);
  return S(() => {
    (async () => {
      try {
        const n = await fetch("/api/config/ui");
        if (n.ok) {
          const d = await n.json();
          i(d);
          const v = `theme-${d.theme || "dark"}`;
          document.documentElement.className = v;
        }
      } catch (n) {
        console.error("Failed to fetch UI settings:", n), document.documentElement.className = "theme-dark";
      } finally {
        c(!1);
      }
    })(), Array.isArray(window.__QUASAR_CUSTOM_MENU__) && m(window.__QUASAR_CUSTOM_MENU__), Array.isArray(window.__QUASAR_CUSTOM_ROUTES__) && f(window.__QUASAR_CUSTOM_ROUTES__);
  }, []), /* @__PURE__ */ s.jsx(le.Provider, { value: { settings: r, isLoading: u, customMenu: o, customRoutes: l }, children: t });
}, ue = () => {
  const t = M(le);
  if (!t)
    throw new Error("useUi must be used within a UiProvider");
  return t;
}, de = U(void 0), Le = ({ children: t }) => {
  const [r, i] = j([]), [u, c] = j(!0);
  S(() => {
    (async () => {
      try {
        const l = await Re.list();
        i(l);
      } catch (l) {
        console.error("Failed to fetch features:", l);
      } finally {
        c(!1);
      }
    })();
  }, []);
  const o = (m) => r.some((l) => l.id === m);
  return /* @__PURE__ */ s.jsx(de.Provider, { value: { features: r, isLoading: u, hasFeature: o }, children: t });
}, Ee = () => {
  const t = M(de);
  if (!t)
    throw new Error("useFeatures must be used within a FeatureProvider");
  return t;
};
class Se {
  connection = null;
  listeners = [];
  async start() {
    if (!this.connection) {
      this.connection = new be.HubConnectionBuilder().withUrl("/hubs/notifications").withAutomaticReconnect().build(), this.connection.on("ReceiveNotification", (r) => {
        this.listeners.forEach((i) => i(r));
      });
      try {
        await this.connection.start(), console.log("Notification SignalR Connected");
      } catch (r) {
        console.error("Notification SignalR Connection Error: ", r), this.connection = null;
      }
    }
  }
  stop() {
    this.connection && (this.connection.stop(), this.connection = null);
  }
  subscribe(r) {
    return this.listeners.push(r), () => {
      this.listeners = this.listeners.filter((i) => i !== r);
    };
  }
}
const te = new Se(), Ce = () => {
  const { notifications: t, unreadCount: r, addNotification: i, markAsRead: u, markAllAsRead: c } = _e(), [o, m] = j(!1), l = re(null);
  S(() => {
    const n = te.subscribe((d) => {
      i({
        title: d.title,
        message: d.message,
        type: d.type
      });
    });
    return te.start(), () => {
      n();
    };
  }, [i]), S(() => {
    const n = (d) => {
      l.current && !l.current.contains(d.target) && m(!1);
    };
    return o && document.addEventListener("mousedown", n), () => {
      document.removeEventListener("mousedown", n);
    };
  }, [o]);
  const f = (n) => {
    const v = (/* @__PURE__ */ new Date()).getTime() - n.getTime(), b = Math.floor(v / 6e4);
    if (b < 1) return "Just now";
    if (b < 60) return `${b}m ago`;
    const N = Math.floor(b / 60);
    return N < 24 ? `${N}h ago` : n.toLocaleDateString();
  }, w = (n) => {
    u(n);
  };
  return /* @__PURE__ */ s.jsxs("div", { className: "notifications-container", ref: l, children: [
    /* @__PURE__ */ s.jsxs(
      "button",
      {
        className: "bell-button",
        onClick: () => m(!o),
        "aria-label": "Notifications",
        children: [
          /* @__PURE__ */ s.jsx("span", { children: "🔔" }),
          r > 0 && /* @__PURE__ */ s.jsx("span", { className: "notification-badge", children: r > 9 ? "9+" : r })
        ]
      }
    ),
    o && /* @__PURE__ */ s.jsxs("div", { className: "notifications-dropdown", children: [
      /* @__PURE__ */ s.jsxs("div", { className: "notifications-header", children: [
        /* @__PURE__ */ s.jsx("h3", { children: "Notifications" }),
        r > 0 && /* @__PURE__ */ s.jsx("button", { className: "mark-all-btn", onClick: c, children: "Mark all as read" })
      ] }),
      /* @__PURE__ */ s.jsx("div", { className: "notifications-list", children: t.length === 0 ? /* @__PURE__ */ s.jsx("div", { className: "empty-notifications", children: "No notifications yet" }) : t.map((n) => /* @__PURE__ */ s.jsxs(
        "div",
        {
          className: `notification-item ${n.read ? "read" : "unread"}`,
          onClick: () => w(n.id),
          children: [
            /* @__PURE__ */ s.jsxs("div", { className: "notification-item-header", children: [
              /* @__PURE__ */ s.jsx("span", { className: `notification-title type-${n.type}`, children: n.title }),
              /* @__PURE__ */ s.jsx("span", { className: "notification-time", children: f(n.timestamp) })
            ] }),
            /* @__PURE__ */ s.jsx("div", { className: "notification-message", children: n.message })
          ]
        },
        n.id
      )) })
    ] })
  ] });
}, Ae = () => {
  const { user: t, logout: r } = ce(), [i, u] = j(!1), [c, o] = j(null), [m, l] = j(!1), [f, w] = j(!1), n = ne(), d = re(null), v = async () => {
    await r(), n("/login");
  }, b = async () => {
    l(!0), u(!1);
  }, N = async () => {
    try {
      const k = await oe.resetOwnPassword();
      o(k), l(!1);
    } catch (k) {
      console.error("Failed to change password:", k), alert("Failed to change password"), l(!1);
    }
  };
  S(() => {
    const k = (T) => {
      d.current && !d.current.contains(T.target) && u(!1);
    };
    return document.addEventListener("mousedown", k), () => {
      document.removeEventListener("mousedown", k);
    };
  }, []);
  const { pathname: y } = je(), { customMenu: _ } = ue(), $ = (() => {
    const T = (_.length > 0 ? _ : window.__QUASAR_CUSTOM_MENU__ || []).flatMap((C) => C.items).find((C) => C.path === y);
    if (T) return T.label;
    switch (y) {
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
  return /* @__PURE__ */ s.jsxs(s.Fragment, { children: [
    /* @__PURE__ */ s.jsxs("header", { className: "app-header", children: [
      /* @__PURE__ */ s.jsx("div", { className: "header-left", children: /* @__PURE__ */ s.jsx("h2", { className: "page-title", children: $ }) }),
      /* @__PURE__ */ s.jsxs("div", { className: "header-right", children: [
        /* @__PURE__ */ s.jsx(Ce, {}),
        /* @__PURE__ */ s.jsxs("div", { className: "user-profile", children: [
          /* @__PURE__ */ s.jsxs("div", { className: "user-info", children: [
            /* @__PURE__ */ s.jsx("span", { className: "user-name", children: t?.username || "Guest" }),
            /* @__PURE__ */ s.jsx("span", { className: "user-role", children: t?.roles?.[0] || "User" })
          ] }),
          /* @__PURE__ */ s.jsx("div", { className: "user-avatar", children: t?.username?.charAt(0).toUpperCase() || "U" })
        ] }),
        /* @__PURE__ */ s.jsxs("div", { className: "menu-container", ref: d, children: [
          /* @__PURE__ */ s.jsx(
            "button",
            {
              className: "menu-button",
              "aria-label": "Menu",
              onClick: () => u(!i),
              children: /* @__PURE__ */ s.jsx("span", { className: "menu-icon", children: "⋮" })
            }
          ),
          i && /* @__PURE__ */ s.jsxs("div", { className: "dropdown-menu", children: [
            /* @__PURE__ */ s.jsx("button", { onClick: b, className: "dropdown-item", children: "Change Password" }),
            /* @__PURE__ */ s.jsx("button", { onClick: v, className: "dropdown-item danger", children: "Sign Out" })
          ] })
        ] })
      ] })
    ] }),
    m && /* @__PURE__ */ s.jsx("div", { className: "modal-overlay", onClick: () => l(!1), children: /* @__PURE__ */ s.jsxs("div", { className: "modal", onClick: (k) => k.stopPropagation(), children: [
      /* @__PURE__ */ s.jsxs("div", { className: "modal-header", children: [
        /* @__PURE__ */ s.jsx("h2", { className: "modal-title", children: "Confirm Password Change" }),
        /* @__PURE__ */ s.jsx("button", { className: "modal-close", onClick: () => l(!1), children: "×" })
      ] }),
      /* @__PURE__ */ s.jsxs("div", { className: "modal-body", children: [
        /* @__PURE__ */ s.jsx("p", { children: "Generate a new password?" }),
        /* @__PURE__ */ s.jsx("p", { className: "text-muted", style: { marginTop: "var(--spacing-md)" }, children: "You will be logged out and need to login with the new password." }),
        /* @__PURE__ */ s.jsxs("div", { style: { display: "flex", gap: "var(--spacing-md)", marginTop: "var(--spacing-lg)" }, children: [
          /* @__PURE__ */ s.jsx(
            "button",
            {
              className: "btn btn-secondary",
              onClick: () => l(!1),
              style: { flex: 1 },
              children: "Cancel"
            }
          ),
          /* @__PURE__ */ s.jsx(
            "button",
            {
              className: "btn btn-primary",
              onClick: N,
              style: { flex: 1 },
              children: "Change Password"
            }
          )
        ] })
      ] })
    ] }) }),
    c && /* @__PURE__ */ s.jsx("div", { className: "modal-overlay", onClick: () => {
      o(null), v();
    }, children: /* @__PURE__ */ s.jsxs("div", { className: "modal", onClick: (k) => k.stopPropagation(), children: [
      /* @__PURE__ */ s.jsx("div", { className: "modal-header", children: /* @__PURE__ */ s.jsx("h2", { className: "modal-title", children: "Password Changed Successfully" }) }),
      /* @__PURE__ */ s.jsxs("div", { className: "modal-body", children: [
        /* @__PURE__ */ s.jsx("p", { children: "Your new password is:" }),
        /* @__PURE__ */ s.jsxs("div", { className: "password-display", style: { display: "flex", alignItems: "center", gap: "var(--spacing-md)", padding: "var(--spacing-md)", background: "var(--color-bg-secondary)", borderRadius: "var(--radius-md)" }, children: [
          /* @__PURE__ */ s.jsx("code", { style: { flex: 1, fontSize: "var(--font-size-base)", fontWeight: "bold" }, children: c }),
          /* @__PURE__ */ s.jsx(
            "button",
            {
              className: "btn btn-sm btn-secondary",
              onClick: () => {
                navigator.clipboard.writeText(c), w(!0), setTimeout(() => w(!1), 2e3);
              },
              children: f ? "✓ Copied!" : "📋 Copy"
            }
          )
        ] }),
        /* @__PURE__ */ s.jsx("p", { className: "warning", style: { marginTop: "var(--spacing-md)", padding: "var(--spacing-md)", background: "var(--color-warning-bg)", border: "1px solid var(--color-warning)", borderRadius: "var(--radius-md)" }, children: "⚠️ Save this password now. You will be logged out." }),
        /* @__PURE__ */ s.jsx(
          "button",
          {
            className: "btn btn-primary",
            onClick: () => {
              o(null), v();
            },
            style: { marginTop: "var(--spacing-lg)", width: "100%" },
            children: "Logout Now"
          }
        )
      ] })
    ] }) })
  ] });
}, Fe = ({ children: t }) => {
  const { user: r, logout: i } = ce(), { settings: u, customMenu: c } = ue(), { hasFeature: o } = Ee(), m = ne(), l = async () => {
    await i(), m("/login");
  };
  return /* @__PURE__ */ s.jsxs("div", { className: "main-layout", children: [
    /* @__PURE__ */ s.jsxs("aside", { className: "sidebar", children: [
      /* @__PURE__ */ s.jsx("div", { className: "sidebar-header", children: /* @__PURE__ */ s.jsxs("div", { className: "logo", children: [
        /* @__PURE__ */ s.jsx("div", { className: "logo-icon", children: u?.logoSymbol || "Q" }),
        /* @__PURE__ */ s.jsx("span", { className: "logo-text", children: u?.applicationName || "Quasar" })
      ] }) }),
      /* @__PURE__ */ s.jsxs("nav", { className: "sidebar-nav", children: [
        c.map((f, w) => /* @__PURE__ */ s.jsxs("div", { className: "nav-section", children: [
          f.title && /* @__PURE__ */ s.jsx("h3", { className: "nav-section-title", children: f.title }),
          f.items.map((n, d) => n.roles && !n.roles.some((v) => r?.roles?.includes(v)) || n.feature && !o(n.feature) ? null : /* @__PURE__ */ s.jsx(R, { to: n.path, className: "nav-link", children: /* @__PURE__ */ s.jsx("span", { children: n.label }) }, `custom-item-${d}-${n.path}`))
        ] }, `custom-${w}`)),
        r?.roles?.includes("administrator") && u?.showAdminMenu !== !1 && /* @__PURE__ */ s.jsxs("div", { className: "nav-section", children: [
          /* @__PURE__ */ s.jsx("h3", { className: "nav-section-title", children: "Administration" }),
          /* @__PURE__ */ s.jsx(R, { to: "/users", className: "nav-link", children: /* @__PURE__ */ s.jsx("span", { children: "Users" }) }),
          /* @__PURE__ */ s.jsx(R, { to: "/roles", className: "nav-link", children: /* @__PURE__ */ s.jsx("span", { children: "Roles" }) }),
          /* @__PURE__ */ s.jsx(R, { to: "/features", className: "nav-link", children: /* @__PURE__ */ s.jsx("span", { children: "Features" }) }),
          /* @__PURE__ */ s.jsx(R, { to: "/logs", className: "nav-link", children: /* @__PURE__ */ s.jsx("span", { children: "Logs" }) }),
          /* @__PURE__ */ s.jsx(R, { to: "/sessions", className: "nav-link", children: /* @__PURE__ */ s.jsx("span", { children: "Sessions" }) }),
          o("scheduler") && /* @__PURE__ */ s.jsx(R, { to: "/jobs", className: "nav-link", children: /* @__PURE__ */ s.jsx("span", { children: "Jobs" }) }),
          o("telemetry") && /* @__PURE__ */ s.jsx(R, { to: "/metrics", className: "nav-link", children: /* @__PURE__ */ s.jsx("span", { children: "Metrics" }) })
        ] })
      ] }),
      /* @__PURE__ */ s.jsx("div", { className: "sidebar-footer", children: /* @__PURE__ */ s.jsx("button", { onClick: l, className: "btn btn-secondary w-full btn-sm", children: "Sign Out" }) })
    ] }),
    /* @__PURE__ */ s.jsxs("div", { className: "content-wrapper", children: [
      /* @__PURE__ */ s.jsx(Ae, {}),
      /* @__PURE__ */ s.jsx("main", { className: `main-content ${X?.type?.layoutOptions?.noPadding ? "no-padding" : ""}`, children: t || /* @__PURE__ */ s.jsx(X, {}) })
    ] })
  ] });
};
export {
  Me as AuthProvider,
  Le as FeatureProvider,
  Ae as Header,
  Fe as MainLayout,
  Ce as NotificationBell,
  Ue as NotificationProvider,
  $e as UiProvider,
  ce as useAuth,
  Ee as useFeatures,
  _e as useNotifications,
  ue as useUi
};
