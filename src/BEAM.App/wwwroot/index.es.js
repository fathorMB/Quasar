import we, { createContext as U, useState as k, useContext as M, useEffect as P, useRef as te } from "react";
import re from "axios";
import * as xe from "@microsoft/signalr";
import { useNavigate as ne, useLocation as be, Link as R, Outlet as X } from "react-router-dom";
var I = { exports: {} }, C = {};
var H;
function je() {
  if (H) return C;
  H = 1;
  var t = Symbol.for("react.transitional.element"), r = Symbol.for("react.fragment");
  function o(d, i, c) {
    var f = null;
    if (c !== void 0 && (f = "" + c), i.key !== void 0 && (f = "" + i.key), "key" in i) {
      c = {};
      for (var l in i)
        l !== "key" && (c[l] = i[l]);
    } else c = i;
    return i = c.ref, {
      $$typeof: t,
      type: d,
      key: f,
      ref: i !== void 0 ? i : null,
      props: c
    };
  }
  return C.Fragment = r, C.jsx = o, C.jsxs = o, C;
}
var A = {};
var Z;
function Ne() {
  return Z || (Z = 1, process.env.NODE_ENV !== "production" && (function() {
    function t(e) {
      if (e == null) return null;
      if (typeof e == "function")
        return e.$$typeof === pe ? null : e.displayName || e.name || null;
      if (typeof e == "string") return e;
      switch (e) {
        case _:
          return "Fragment";
        case $:
          return "Profiler";
        case W:
          return "StrictMode";
        case de:
          return "Suspense";
        case me:
          return "SuspenseList";
        case he:
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
          case N:
            return (e._context.displayName || "Context") + ".Consumer";
          case S:
            var a = e.render;
            return e = e.displayName, e || (e = a.displayName || a.name || "", e = e !== "" ? "ForwardRef(" + e + ")" : "ForwardRef"), e;
          case fe:
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
    function o(e) {
      try {
        r(e);
        var a = !1;
      } catch {
        a = !0;
      }
      if (a) {
        a = console;
        var h = a.error, g = typeof Symbol == "function" && Symbol.toStringTag && e[Symbol.toStringTag] || e.constructor.name || "Object";
        return h.call(
          a,
          "The provided key is an unsupported type %s. This value must be coerced to a string before using it here.",
          g
        ), r(e);
      }
    }
    function d(e) {
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
    function i() {
      var e = F.A;
      return e === null ? null : e.getOwner();
    }
    function c() {
      return Error("react-stack-top-frame");
    }
    function f(e) {
      if (q.call(e, "key")) {
        var a = Object.getOwnPropertyDescriptor(e, "key").get;
        if (a && a.isReactWarning) return !1;
      }
      return e.key !== void 0;
    }
    function l(e, a) {
      function h() {
        z || (z = !0, console.error(
          "%s: `key` is not a prop. Trying to access it will result in `undefined` being returned. If you need to access the same value within the child component, you should pass it as a different prop. (https://react.dev/link/special-props)",
          a
        ));
      }
      h.isReactWarning = !0, Object.defineProperty(e, "key", {
        get: h,
        configurable: !0
      });
    }
    function p() {
      var e = t(this.type);
      return V[e] || (V[e] = !0, console.error(
        "Accessing element.ref was removed in React 19. ref is now a regular prop. It will be removed from the JSX Element type in a future release."
      )), e = this.props.ref, e !== void 0 ? e : null;
    }
    function m(e, a, h, g, O, D) {
      var v = h.ref;
      return e = {
        $$typeof: j,
        type: e,
        key: a,
        props: h,
        _owner: g
      }, (v !== void 0 ? v : null) !== null ? Object.defineProperty(e, "ref", {
        enumerable: !1,
        get: p
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
    function n(e, a, h, g, O, D) {
      var v = a.children;
      if (v !== void 0)
        if (g)
          if (ge(v)) {
            for (g = 0; g < v.length; g++)
              u(v[g]);
            Object.freeze && Object.freeze(v);
          } else
            console.error(
              "React.jsx: Static children should always be an array. You are likely explicitly calling React.jsxs or React.jsxDEV. Use the Babel transform instead."
            );
        else u(v);
      if (q.call(a, "key")) {
        v = t(e);
        var E = Object.keys(a).filter(function(ve) {
          return ve !== "key";
        });
        g = 0 < E.length ? "{key: someKey, " + E.join(": ..., ") + ": ...}" : "{key: someKey}", B[v + g] || (E = 0 < E.length ? "{" + E.join(": ..., ") + ": ...}" : "{}", console.error(
          `A props object containing a "key" prop is being spread into JSX:
  let props = %s;
  <%s {...props} />
React keys must be passed directly to JSX without using spread:
  let props = %s;
  <%s key={someKey} {...props} />`,
          g,
          v,
          E,
          v
        ), B[v + g] = !0);
      }
      if (v = null, h !== void 0 && (o(h), v = "" + h), f(a) && (o(a.key), v = "" + a.key), "key" in a) {
        h = {};
        for (var J in a)
          J !== "key" && (h[J] = a[J]);
      } else h = a;
      return v && l(
        h,
        typeof e == "function" ? e.displayName || e.name || "Unknown" : e
      ), m(
        e,
        v,
        h,
        i(),
        O,
        D
      );
    }
    function u(e) {
      w(e) ? e._store && (e._store.validated = 1) : typeof e == "object" && e !== null && e.$$typeof === L && (e._payload.status === "fulfilled" ? w(e._payload.value) && e._payload.value._store && (e._payload.value._store.validated = 1) : e._store && (e._store.validated = 1));
    }
    function w(e) {
      return typeof e == "object" && e !== null && e.$$typeof === j;
    }
    var x = we, j = Symbol.for("react.transitional.element"), y = Symbol.for("react.portal"), _ = Symbol.for("react.fragment"), W = Symbol.for("react.strict_mode"), $ = Symbol.for("react.profiler"), N = Symbol.for("react.consumer"), T = Symbol.for("react.context"), S = Symbol.for("react.forward_ref"), de = Symbol.for("react.suspense"), me = Symbol.for("react.suspense_list"), fe = Symbol.for("react.memo"), L = Symbol.for("react.lazy"), he = Symbol.for("react.activity"), pe = Symbol.for("react.client.reference"), F = x.__CLIENT_INTERNALS_DO_NOT_USE_OR_WARN_USERS_THEY_CANNOT_UPGRADE, q = Object.prototype.hasOwnProperty, ge = Array.isArray, Y = console.createTask ? console.createTask : function() {
      return null;
    };
    x = {
      react_stack_bottom_frame: function(e) {
        return e();
      }
    };
    var z, V = {}, G = x.react_stack_bottom_frame.bind(
      x,
      c
    )(), Q = Y(d(c)), B = {};
    A.Fragment = _, A.jsx = function(e, a, h) {
      var g = 1e4 > F.recentlyCreatedOwnerStacks++;
      return n(
        e,
        a,
        h,
        !1,
        g ? Error("react-stack-top-frame") : G,
        g ? Y(d(e)) : Q
      );
    }, A.jsxs = function(e, a, h) {
      var g = 1e4 > F.recentlyCreatedOwnerStacks++;
      return n(
        e,
        a,
        h,
        !0,
        g ? Error("react-stack-top-frame") : G,
        g ? Y(d(e)) : Q
      );
    };
  })()), A;
}
var K;
function ke() {
  return K || (K = 1, process.env.NODE_ENV === "production" ? I.exports = je() : I.exports = Ne()), I.exports;
}
var s = ke();
const ae = U(void 0), Oe = ({ children: t }) => {
  const [r, o] = k([]), d = r.filter((p) => !p.read).length, i = (p) => {
    const m = {
      ...p,
      id: Math.random().toString(36).substring(2, 9),
      timestamp: /* @__PURE__ */ new Date(),
      read: !1
    };
    o((n) => [m, ...n].slice(0, 50));
  }, c = (p) => {
    o((m) => m.map((n) => n.id === p ? { ...n, read: !0 } : n));
  }, f = () => {
    o((p) => p.map((m) => ({ ...m, read: !0 })));
  }, l = () => {
    o([]);
  };
  return /* @__PURE__ */ s.jsx(ae.Provider, { value: {
    notifications: r,
    unreadCount: d,
    addNotification: i,
    markAsRead: c,
    markAllAsRead: f,
    clearNotifications: l
  }, children: t });
}, ye = () => {
  const t = M(ae);
  if (!t)
    throw new Error("useNotifications must be used within a NotificationProvider");
  return t;
}, b = re.create({
  baseURL: "/auth",
  headers: {
    "Content-Type": "application/json"
  },
  withCredentials: !0
  // Important for cookie-based auth if needed
});
b.interceptors.request.use(
  (t) => {
    const r = localStorage.getItem("accessToken");
    return r && (t.headers.Authorization = `Bearer ${r}`), t;
  },
  (t) => Promise.reject(t)
);
b.interceptors.response.use(
  (t) => t,
  async (t) => {
    const r = t.config;
    if (t.response?.status === 401 && r) {
      const c = localStorage.getItem("refreshToken");
      if (c && !r._retry) {
        r._retry = !0;
        try {
          const f = await re.post("/auth/token/refresh", {
            refreshToken: c
          }), { accessToken: l, refreshToken: p } = f.data;
          return localStorage.setItem("accessToken", l), localStorage.setItem("refreshToken", p), r.headers.Authorization = `Bearer ${l}`, b(r);
        } catch (f) {
          return localStorage.removeItem("accessToken"), localStorage.removeItem("refreshToken"), window.location.href = "/login", Promise.reject(f);
        }
      }
    }
    const o = t.config?.url?.endsWith("/login"), d = t.response?.data;
    if (t.response?.status === 401 && d?.code === "SESSION_REVOKED") {
      localStorage.removeItem("accessToken"), localStorage.removeItem("refreshToken"), window.dispatchEvent(new CustomEvent("session-revoked", {
        detail: { message: d.message }
      }));
      const c = {
        message: d.message,
        statusCode: 401
      };
      return Promise.reject(c);
    }
    const i = {
      message: t.response?.status === 401 && o ? "Invalid username or password" : d?.message || t.message || "An error occurred",
      statusCode: t.response?.status || 500
    };
    return Promise.reject(i);
  }
);
const ee = {
  /**
   * Login with username and password
   */
  login: async (t) => {
    const r = await b.post("/login", t);
    return localStorage.setItem("accessToken", r.data.accessToken), localStorage.setItem("refreshToken", r.data.refreshToken), r.data;
  },
  /**
   * Register a new user
   */
  register: async (t) => (await b.post("/register", t)).data,
  /**
   * Refresh access token
   */
  refreshToken: async (t) => {
    const r = await b.post("/token/refresh", t);
    return localStorage.setItem("accessToken", r.data.accessToken), localStorage.setItem("refreshToken", r.data.refreshToken), r.data;
  },
  /**
   * Logout and revoke refresh token
   */
  logout: async () => {
    const t = localStorage.getItem("refreshToken");
    if (t)
      try {
        await b.post("/logout", { refreshToken: t });
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
  list: async () => (await b.get("/users")).data,
  /**
   * Get roles for a specific user
   */
  getRoles: async (t) => (await b.get(`/users/${t}/roles`)).data,
  /**
   * Get permissions for a specific user
   */
  getPermissions: async (t) => (await b.get(`/users/${t}/permissions`)).data,
  /**
   * Assign a role to a user
   */
  assignRole: async (t, r) => {
    await b.post(`/users/${t}/roles`, r);
  },
  /**
   * Revoke a role from a user
   */
  revokeRole: async (t, r) => {
    await b.delete(`/users/${t}/roles/${r}`);
  },
  /**
   * Reset a user's password (admin only)
   * Returns the new generated password
   */
  resetPassword: async (t) => (await b.post(`/users/${t}/reset-password`)).data.password,
  /**
   * Reset own password (any authenticated user)
   * Returns the new generated password
   */
  resetOwnPassword: async () => (await b.post("/users/me/reset-password")).data.password,
  delete: async (t) => {
    await b.delete(`/users/${t}`);
  }
}, ie = U(void 0), ce = () => {
  const t = M(ie);
  if (!t)
    throw new Error("useAuth must be used within an AuthProvider");
  return t;
}, Ie = ({ children: t }) => {
  const [r, o] = k(null), [d, i] = k(!0);
  P(() => {
    c();
  }, []);
  const c = async () => {
    try {
      const n = localStorage.getItem("accessToken");
      if (n) {
        const u = f(n);
        console.log("Decoded payload:", u);
        const w = u.sub, x = u.unique_name || u.name || "User";
        let j = [];
        try {
          console.log("Fetching roles for userId:", w);
          const y = await oe.getRoles(w);
          console.log("Fetched userRoles:", y), j = y.map((_) => _.name), console.log("Mapped roles:", j);
        } catch (y) {
          console.error("Failed to fetch roles", y);
        }
        o({
          id: w,
          username: x,
          email: u.email || "",
          roles: j
        });
      } else
        o(null);
    } catch (n) {
      console.error("Auth check failed:", n), o(null);
    } finally {
      i(!1);
    }
  };
  function f(n) {
    var u = n.split(".")[1], w = u.replace(/-/g, "+").replace(/_/g, "/"), x = decodeURIComponent(window.atob(w).split("").map(function(j) {
      return "%" + ("00" + j.charCodeAt(0).toString(16)).slice(-2);
    }).join(""));
    return JSON.parse(x);
  }
  const m = {
    user: r,
    isAuthenticated: !!r,
    isLoading: d,
    login: async (n) => {
      i(!0);
      try {
        await ee.login(n), await c();
      } catch (u) {
        throw o(null), u;
      } finally {
        i(!1);
      }
    },
    logout: async () => {
      i(!0);
      try {
        await ee.logout();
      } catch (n) {
        console.error("Logout error:", n);
      } finally {
        o(null), i(!1);
      }
    }
  };
  return /* @__PURE__ */ s.jsx(ie.Provider, { value: m, children: t });
}, le = U(void 0), Ue = ({ children: t }) => {
  const [r, o] = k(null), [d, i] = k(!0), [c, f] = k([]), [l, p] = k([]);
  return P(() => {
    (async () => {
      try {
        const n = await fetch("/api/config/ui");
        if (n.ok) {
          const u = await n.json();
          o(u);
          const w = `theme-${u.theme || "dark"}`;
          document.documentElement.className = w;
        }
      } catch (n) {
        console.error("Failed to fetch UI settings:", n), document.documentElement.className = "theme-dark";
      } finally {
        i(!1);
      }
    })(), Array.isArray(window.__QUASAR_CUSTOM_MENU__) && f(window.__QUASAR_CUSTOM_MENU__), Array.isArray(window.__QUASAR_CUSTOM_ROUTES__) && p(window.__QUASAR_CUSTOM_ROUTES__);
  }, []), /* @__PURE__ */ s.jsx(le.Provider, { value: { settings: r, isLoading: d, customMenu: c, customRoutes: l }, children: t });
}, ue = () => {
  const t = M(le);
  if (!t)
    throw new Error("useUi must be used within a UiProvider");
  return t;
};
class _e {
  connection = null;
  listeners = [];
  async start() {
    if (!this.connection) {
      this.connection = new xe.HubConnectionBuilder().withUrl("/hubs/notifications").withAutomaticReconnect().build(), this.connection.on("ReceiveNotification", (r) => {
        this.listeners.forEach((o) => o(r));
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
      this.listeners = this.listeners.filter((o) => o !== r);
    };
  }
}
const se = new _e(), Re = () => {
  const { notifications: t, unreadCount: r, addNotification: o, markAsRead: d, markAllAsRead: i } = ye(), [c, f] = k(!1), l = te(null);
  P(() => {
    const n = se.subscribe((u) => {
      o({
        title: u.title,
        message: u.message,
        type: u.type
      });
    });
    return se.start(), () => {
      n();
    };
  }, [o]), P(() => {
    const n = (u) => {
      l.current && !l.current.contains(u.target) && f(!1);
    };
    return c && document.addEventListener("mousedown", n), () => {
      document.removeEventListener("mousedown", n);
    };
  }, [c]);
  const p = (n) => {
    const w = (/* @__PURE__ */ new Date()).getTime() - n.getTime(), x = Math.floor(w / 6e4);
    if (x < 1) return "Just now";
    if (x < 60) return `${x}m ago`;
    const j = Math.floor(x / 60);
    return j < 24 ? `${j}h ago` : n.toLocaleDateString();
  }, m = (n) => {
    d(n);
  };
  return /* @__PURE__ */ s.jsxs("div", { className: "notifications-container", ref: l, children: [
    /* @__PURE__ */ s.jsxs(
      "button",
      {
        className: "bell-button",
        onClick: () => f(!c),
        "aria-label": "Notifications",
        children: [
          /* @__PURE__ */ s.jsx("span", { children: "🔔" }),
          r > 0 && /* @__PURE__ */ s.jsx("span", { className: "notification-badge", children: r > 9 ? "9+" : r })
        ]
      }
    ),
    c && /* @__PURE__ */ s.jsxs("div", { className: "notifications-dropdown", children: [
      /* @__PURE__ */ s.jsxs("div", { className: "notifications-header", children: [
        /* @__PURE__ */ s.jsx("h3", { children: "Notifications" }),
        r > 0 && /* @__PURE__ */ s.jsx("button", { className: "mark-all-btn", onClick: i, children: "Mark all as read" })
      ] }),
      /* @__PURE__ */ s.jsx("div", { className: "notifications-list", children: t.length === 0 ? /* @__PURE__ */ s.jsx("div", { className: "empty-notifications", children: "No notifications yet" }) : t.map((n) => /* @__PURE__ */ s.jsxs(
        "div",
        {
          className: `notification-item ${n.read ? "read" : "unread"}`,
          onClick: () => m(n.id),
          children: [
            /* @__PURE__ */ s.jsxs("div", { className: "notification-item-header", children: [
              /* @__PURE__ */ s.jsx("span", { className: `notification-title type-${n.type}`, children: n.title }),
              /* @__PURE__ */ s.jsx("span", { className: "notification-time", children: p(n.timestamp) })
            ] }),
            /* @__PURE__ */ s.jsx("div", { className: "notification-message", children: n.message })
          ]
        },
        n.id
      )) })
    ] })
  ] });
}, Te = () => {
  const { user: t, logout: r } = ce(), [o, d] = k(!1), [i, c] = k(null), [f, l] = k(!1), [p, m] = k(!1), n = ne(), u = te(null), w = async () => {
    await r(), n("/login");
  }, x = async () => {
    l(!0), d(!1);
  }, j = async () => {
    try {
      const N = await oe.resetOwnPassword();
      c(N), l(!1);
    } catch (N) {
      console.error("Failed to change password:", N), alert("Failed to change password"), l(!1);
    }
  };
  P(() => {
    const N = (T) => {
      u.current && !u.current.contains(T.target) && d(!1);
    };
    return document.addEventListener("mousedown", N), () => {
      document.removeEventListener("mousedown", N);
    };
  }, []);
  const { pathname: y } = be(), { customMenu: _ } = ue(), $ = (() => {
    const T = (_.length > 0 ? _ : window.__QUASAR_CUSTOM_MENU__ || []).flatMap((S) => S.items).find((S) => S.path === y);
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
        /* @__PURE__ */ s.jsx(Re, {}),
        /* @__PURE__ */ s.jsxs("div", { className: "user-profile", children: [
          /* @__PURE__ */ s.jsxs("div", { className: "user-info", children: [
            /* @__PURE__ */ s.jsx("span", { className: "user-name", children: t?.username || "Guest" }),
            /* @__PURE__ */ s.jsx("span", { className: "user-role", children: t?.roles?.[0] || "User" })
          ] }),
          /* @__PURE__ */ s.jsx("div", { className: "user-avatar", children: t?.username?.charAt(0).toUpperCase() || "U" })
        ] }),
        /* @__PURE__ */ s.jsxs("div", { className: "menu-container", ref: u, children: [
          /* @__PURE__ */ s.jsx(
            "button",
            {
              className: "menu-button",
              "aria-label": "Menu",
              onClick: () => d(!o),
              children: /* @__PURE__ */ s.jsx("span", { className: "menu-icon", children: "⋮" })
            }
          ),
          o && /* @__PURE__ */ s.jsxs("div", { className: "dropdown-menu", children: [
            /* @__PURE__ */ s.jsx("button", { onClick: x, className: "dropdown-item", children: "Change Password" }),
            /* @__PURE__ */ s.jsx("button", { onClick: w, className: "dropdown-item danger", children: "Sign Out" })
          ] })
        ] })
      ] })
    ] }),
    f && /* @__PURE__ */ s.jsx("div", { className: "modal-overlay", onClick: () => l(!1), children: /* @__PURE__ */ s.jsxs("div", { className: "modal", onClick: (N) => N.stopPropagation(), children: [
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
              onClick: j,
              style: { flex: 1 },
              children: "Change Password"
            }
          )
        ] })
      ] })
    ] }) }),
    i && /* @__PURE__ */ s.jsx("div", { className: "modal-overlay", onClick: () => {
      c(null), w();
    }, children: /* @__PURE__ */ s.jsxs("div", { className: "modal", onClick: (N) => N.stopPropagation(), children: [
      /* @__PURE__ */ s.jsx("div", { className: "modal-header", children: /* @__PURE__ */ s.jsx("h2", { className: "modal-title", children: "Password Changed Successfully" }) }),
      /* @__PURE__ */ s.jsxs("div", { className: "modal-body", children: [
        /* @__PURE__ */ s.jsx("p", { children: "Your new password is:" }),
        /* @__PURE__ */ s.jsxs("div", { className: "password-display", style: { display: "flex", alignItems: "center", gap: "var(--spacing-md)", padding: "var(--spacing-md)", background: "var(--color-bg-secondary)", borderRadius: "var(--radius-md)" }, children: [
          /* @__PURE__ */ s.jsx("code", { style: { flex: 1, fontSize: "var(--font-size-base)", fontWeight: "bold" }, children: i }),
          /* @__PURE__ */ s.jsx(
            "button",
            {
              className: "btn btn-sm btn-secondary",
              onClick: () => {
                navigator.clipboard.writeText(i), m(!0), setTimeout(() => m(!1), 2e3);
              },
              children: p ? "✓ Copied!" : "📋 Copy"
            }
          )
        ] }),
        /* @__PURE__ */ s.jsx("p", { className: "warning", style: { marginTop: "var(--spacing-md)", padding: "var(--spacing-md)", background: "var(--color-warning-bg)", border: "1px solid var(--color-warning)", borderRadius: "var(--radius-md)" }, children: "⚠️ Save this password now. You will be logged out." }),
        /* @__PURE__ */ s.jsx(
          "button",
          {
            className: "btn btn-primary",
            onClick: () => {
              c(null), w();
            },
            style: { marginTop: "var(--spacing-lg)", width: "100%" },
            children: "Logout Now"
          }
        )
      ] })
    ] }) })
  ] });
}, Ee = U(void 0), Se = () => {
  const t = M(Ee);
  if (!t)
    throw new Error("useFeatures must be used within a FeatureProvider");
  return t;
}, Me = () => {
  const { user: t, logout: r } = ce(), { settings: o, customMenu: d } = ue(), { hasFeature: i } = Se(), c = ne(), f = async () => {
    await r(), c("/login");
  };
  return /* @__PURE__ */ s.jsxs("div", { className: "main-layout", children: [
    /* @__PURE__ */ s.jsxs("aside", { className: "sidebar", children: [
      /* @__PURE__ */ s.jsx("div", { className: "sidebar-header", children: /* @__PURE__ */ s.jsxs("div", { className: "logo", children: [
        /* @__PURE__ */ s.jsx("div", { className: "logo-icon", children: o?.logoSymbol || "Q" }),
        /* @__PURE__ */ s.jsx("span", { className: "logo-text", children: o?.applicationName || "Quasar" })
      ] }) }),
      /* @__PURE__ */ s.jsxs("nav", { className: "sidebar-nav", children: [
        d.map((l, p) => /* @__PURE__ */ s.jsxs("div", { className: "nav-section", children: [
          l.title && /* @__PURE__ */ s.jsx("h3", { className: "nav-section-title", children: l.title }),
          l.items.map((m, n) => m.roles && !m.roles.some((u) => t?.roles?.includes(u)) || m.feature && !i(m.feature) ? null : /* @__PURE__ */ s.jsx(R, { to: m.path, className: "nav-link", children: /* @__PURE__ */ s.jsx("span", { children: m.label }) }, `custom-item-${n}-${m.path}`))
        ] }, `custom-${p}`)),
        t?.roles?.includes("administrator") && o?.showAdminMenu !== !1 && /* @__PURE__ */ s.jsxs("div", { className: "nav-section", children: [
          /* @__PURE__ */ s.jsx("h3", { className: "nav-section-title", children: "Administration" }),
          /* @__PURE__ */ s.jsx(R, { to: "/users", className: "nav-link", children: /* @__PURE__ */ s.jsx("span", { children: "Users" }) }),
          /* @__PURE__ */ s.jsx(R, { to: "/roles", className: "nav-link", children: /* @__PURE__ */ s.jsx("span", { children: "Roles" }) }),
          /* @__PURE__ */ s.jsx(R, { to: "/features", className: "nav-link", children: /* @__PURE__ */ s.jsx("span", { children: "Features" }) }),
          /* @__PURE__ */ s.jsx(R, { to: "/logs", className: "nav-link", children: /* @__PURE__ */ s.jsx("span", { children: "Logs" }) }),
          /* @__PURE__ */ s.jsx(R, { to: "/sessions", className: "nav-link", children: /* @__PURE__ */ s.jsx("span", { children: "Sessions" }) }),
          i("scheduler") && /* @__PURE__ */ s.jsx(R, { to: "/jobs", className: "nav-link", children: /* @__PURE__ */ s.jsx("span", { children: "Jobs" }) }),
          i("telemetry") && /* @__PURE__ */ s.jsx(R, { to: "/metrics", className: "nav-link", children: /* @__PURE__ */ s.jsx("span", { children: "Metrics" }) })
        ] })
      ] }),
      /* @__PURE__ */ s.jsx("div", { className: "sidebar-footer", children: /* @__PURE__ */ s.jsx("button", { onClick: f, className: "btn btn-secondary w-full btn-sm", children: "Sign Out" }) })
    ] }),
    /* @__PURE__ */ s.jsxs("div", { className: "content-wrapper", children: [
      /* @__PURE__ */ s.jsx(Te, {}),
      /* @__PURE__ */ s.jsx("main", { className: `main-content ${X?.type?.layoutOptions?.noPadding ? "no-padding" : ""}`, children: /* @__PURE__ */ s.jsx(X, {}) })
    ] })
  ] });
};
export {
  Ie as AuthProvider,
  Te as Header,
  Me as MainLayout,
  Re as NotificationBell,
  Oe as NotificationProvider,
  Ue as UiProvider,
  ce as useAuth,
  ye as useNotifications,
  ue as useUi
};
