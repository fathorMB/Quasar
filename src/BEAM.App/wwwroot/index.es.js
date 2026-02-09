import A, { createContext as L, useState as j, useEffect as P, useContext as D, useRef as ne } from "react";
import B from "axios";
import * as xe from "@microsoft/signalr";
import { useNavigate as oe, useLocation as ke, Link as _, Outlet as ee } from "react-router-dom";
var M = { exports: {} }, U = {};
var se;
function je() {
  if (se) return U;
  se = 1;
  var t = Symbol.for("react.transitional.element"), r = Symbol.for("react.fragment");
  function o(d, l, n) {
    var m = null;
    if (n !== void 0 && (m = "" + n), l.key !== void 0 && (m = "" + l.key), "key" in l) {
      n = {};
      for (var u in l)
        u !== "key" && (n[u] = l[u]);
    } else n = l;
    return l = n.ref, {
      $$typeof: t,
      type: d,
      key: m,
      ref: l !== void 0 ? l : null,
      props: n
    };
  }
  return U.Fragment = r, U.jsx = o, U.jsxs = o, U;
}
var $ = {};
var te;
function ye() {
  return te || (te = 1, process.env.NODE_ENV !== "production" && (function() {
    function t(e) {
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
          case E:
            return e.displayName || "Context";
          case y:
            return (e._context.displayName || "Context") + ".Consumer";
          case I:
            var i = e.render;
            return e = e.displayName, e || (e = i.displayName || i.name || "", e = e !== "" ? "ForwardRef(" + e + ")" : "ForwardRef"), e;
          case pe:
            return i = e.displayName || null, i !== null ? i : t(e.type) || "Memo";
          case Y:
            i = e._payload, e = e._init;
            try {
              return t(e(i));
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
        var i = !1;
      } catch {
        i = !0;
      }
      if (i) {
        i = console;
        var g = i.error, w = typeof Symbol == "function" && Symbol.toStringTag && e[Symbol.toStringTag] || e.constructor.name || "Object";
        return g.call(
          i,
          "The provided key is an unsupported type %s. This value must be coerced to a string before using it here.",
          w
        ), r(e);
      }
    }
    function d(e) {
      if (e === T) return "<>";
      if (typeof e == "object" && e !== null && e.$$typeof === Y)
        return "<...>";
      try {
        var i = t(e);
        return i ? "<" + i + ">" : "<...>";
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
    function m(e) {
      if (G.call(e, "key")) {
        var i = Object.getOwnPropertyDescriptor(e, "key").get;
        if (i && i.isReactWarning) return !1;
      }
      return e.key !== void 0;
    }
    function u(e, i) {
      function g() {
        Q || (Q = !0, console.error(
          "%s: `key` is not a prop. Trying to access it will result in `undefined` being returned. If you need to access the same value within the child component, you should pass it as a different prop. (https://react.dev/link/special-props)",
          i
        ));
      }
      g.isReactWarning = !0, Object.defineProperty(e, "key", {
        get: g,
        configurable: !0
      });
    }
    function x() {
      var e = t(this.type);
      return H[e] || (H[e] = !0, console.error(
        "Accessing element.ref was removed in React 19. ref is now a regular prop. It will be removed from the JSX Element type in a future release."
      )), e = this.props.ref, e !== void 0 ? e : null;
    }
    function h(e, i, g, w, F, W) {
      var v = g.ref;
      return e = {
        $$typeof: p,
        type: e,
        key: i,
        props: g,
        _owner: w
      }, (v !== void 0 ? v : null) !== null ? Object.defineProperty(e, "ref", {
        enumerable: !1,
        get: x
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
    function a(e, i, g, w, F, W) {
      var v = i.children;
      if (v !== void 0)
        if (w)
          if (ve(v)) {
            for (w = 0; w < v.length; w++)
              c(v[w]);
            Object.freeze && Object.freeze(v);
          } else
            console.error(
              "React.jsx: Static children should always be an array. You are likely explicitly calling React.jsxs or React.jsxDEV. Use the Babel transform instead."
            );
        else c(v);
      if (G.call(i, "key")) {
        v = t(e);
        var S = Object.keys(i).filter(function(be) {
          return be !== "key";
        });
        w = 0 < S.length ? "{key: someKey, " + S.join(": ..., ") + ": ...}" : "{key: someKey}", K[v + w] || (S = 0 < S.length ? "{" + S.join(": ..., ") + ": ...}" : "{}", console.error(
          `A props object containing a "key" prop is being spread into JSX:
  let props = %s;
  <%s {...props} />
React keys must be passed directly to JSX without using spread:
  let props = %s;
  <%s key={someKey} {...props} />`,
          w,
          v,
          S,
          v
        ), K[v + w] = !0);
      }
      if (v = null, g !== void 0 && (o(g), v = "" + g), m(i) && (o(i.key), v = "" + i.key), "key" in i) {
        g = {};
        for (var q in i)
          q !== "key" && (g[q] = i[q]);
      } else g = i;
      return v && u(
        g,
        typeof e == "function" ? e.displayName || e.name || "Unknown" : e
      ), h(
        e,
        v,
        g,
        l(),
        F,
        W
      );
    }
    function c(e) {
      f(e) ? e._store && (e._store.validated = 1) : typeof e == "object" && e !== null && e.$$typeof === Y && (e._payload.status === "fulfilled" ? f(e._payload.value) && e._payload.value._store && (e._payload.value._store.validated = 1) : e._store && (e._store.validated = 1));
    }
    function f(e) {
      return typeof e == "object" && e !== null && e.$$typeof === p;
    }
    var b = A, p = Symbol.for("react.transitional.element"), N = Symbol.for("react.portal"), T = Symbol.for("react.fragment"), R = Symbol.for("react.strict_mode"), O = Symbol.for("react.profiler"), y = Symbol.for("react.consumer"), E = Symbol.for("react.context"), I = Symbol.for("react.forward_ref"), fe = Symbol.for("react.suspense"), he = Symbol.for("react.suspense_list"), pe = Symbol.for("react.memo"), Y = Symbol.for("react.lazy"), ge = Symbol.for("react.activity"), we = Symbol.for("react.client.reference"), z = b.__CLIENT_INTERNALS_DO_NOT_USE_OR_WARN_USERS_THEY_CANNOT_UPGRADE, G = Object.prototype.hasOwnProperty, ve = Array.isArray, J = console.createTask ? console.createTask : function() {
      return null;
    };
    b = {
      react_stack_bottom_frame: function(e) {
        return e();
      }
    };
    var Q, H = {}, X = b.react_stack_bottom_frame.bind(
      b,
      n
    )(), Z = J(d(n)), K = {};
    $.Fragment = T, $.jsx = function(e, i, g) {
      var w = 1e4 > z.recentlyCreatedOwnerStacks++;
      return a(
        e,
        i,
        g,
        !1,
        w ? Error("react-stack-top-frame") : X,
        w ? J(d(e)) : Z
      );
    }, $.jsxs = function(e, i, g) {
      var w = 1e4 > z.recentlyCreatedOwnerStacks++;
      return a(
        e,
        i,
        g,
        !0,
        w ? Error("react-stack-top-frame") : X,
        w ? J(d(e)) : Z
      );
    };
  })()), $;
}
var re;
function Ne() {
  return re || (re = 1, process.env.NODE_ENV === "production" ? M.exports = je() : M.exports = ye()), M.exports;
}
var s = Ne();
const k = B.create({
  baseURL: "/auth",
  headers: {
    "Content-Type": "application/json"
  },
  withCredentials: !0
  // Important for cookie-based auth if needed
});
k.interceptors.request.use(
  (t) => {
    const r = localStorage.getItem("accessToken");
    return r && (t.headers.Authorization = `Bearer ${r}`), t;
  },
  (t) => Promise.reject(t)
);
k.interceptors.response.use(
  (t) => t,
  async (t) => {
    const r = t.config;
    if (t.response?.status === 401 && r) {
      const n = localStorage.getItem("refreshToken");
      if (n && !r._retry) {
        r._retry = !0;
        try {
          const m = await B.post("/auth/token/refresh", {
            refreshToken: n
          }), { accessToken: u, refreshToken: x } = m.data;
          return localStorage.setItem("accessToken", u), localStorage.setItem("refreshToken", x), r.headers.Authorization = `Bearer ${u}`, k(r);
        } catch (m) {
          return localStorage.removeItem("accessToken"), localStorage.removeItem("refreshToken"), window.location.href = "/login", Promise.reject(m);
        }
      }
    }
    const o = t.config?.url?.endsWith("/login"), d = t.response?.data;
    if (t.response?.status === 401 && d?.code === "SESSION_REVOKED") {
      localStorage.removeItem("accessToken"), localStorage.removeItem("refreshToken"), window.dispatchEvent(new CustomEvent("session-revoked", {
        detail: { message: d.message }
      }));
      const n = {
        message: d.message,
        statusCode: 401
      };
      return Promise.reject(n);
    }
    const l = {
      message: t.response?.status === 401 && o ? "Invalid username or password" : d?.message || t.message || "An error occurred",
      statusCode: t.response?.status || 500
    };
    return Promise.reject(l);
  }
);
const ae = {
  /**
   * Login with username and password
   */
  login: async (t) => {
    const r = await k.post("/login", t);
    return localStorage.setItem("accessToken", r.data.accessToken), localStorage.setItem("refreshToken", r.data.refreshToken), r.data;
  },
  /**
   * Register a new user
   */
  register: async (t) => (await k.post("/register", t)).data,
  /**
   * Refresh access token
   */
  refreshToken: async (t) => {
    const r = await k.post("/token/refresh", t);
    return localStorage.setItem("accessToken", r.data.accessToken), localStorage.setItem("refreshToken", r.data.refreshToken), r.data;
  },
  /**
   * Logout and revoke refresh token
   */
  logout: async () => {
    const t = localStorage.getItem("refreshToken");
    if (t)
      try {
        await k.post("/logout", { refreshToken: t });
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
  list: async () => (await k.get("/users")).data,
  /**
   * Get roles for a specific user
   */
  getRoles: async (t) => (await k.get(`/users/${t}/roles`)).data,
  /**
   * Get permissions for a specific user
   */
  getPermissions: async (t) => (await k.get(`/users/${t}/permissions`)).data,
  /**
   * Assign a role to a user
   */
  assignRole: async (t, r) => {
    await k.post(`/users/${t}/roles`, r);
  },
  /**
   * Revoke a role from a user
   */
  revokeRole: async (t, r) => {
    await k.delete(`/users/${t}/roles/${r}`);
  },
  /**
   * Reset a user's password (admin only)
   * Returns the new generated password
   */
  resetPassword: async (t) => (await k.post(`/users/${t}/reset-password`)).data.password,
  /**
   * Reset own password (any authenticated user)
   * Returns the new generated password
   */
  resetOwnPassword: async () => (await k.post("/users/me/reset-password")).data.password,
  delete: async (t) => {
    await k.delete(`/users/${t}`);
  }
}, Te = {
  async list() {
    return (await k.get("/api/features", { baseURL: "/" })).data;
  },
  async listDirect() {
    return (await B.get("/api/features", {
      headers: Re(),
      withCredentials: !0
    })).data;
  }
};
function Re() {
  const t = localStorage.getItem("accessToken");
  return t ? { Authorization: `Bearer ${t}` } : {};
}
const ce = L(void 0), V = () => {
  const t = D(ce);
  if (!t)
    throw new Error("useAuth must be used within an AuthProvider");
  return t;
}, Me = ({ children: t }) => {
  const [r, o] = j(null), [d, l] = j(!0);
  P(() => {
    n();
  }, []);
  const n = async () => {
    try {
      const a = localStorage.getItem("accessToken");
      if (a) {
        const c = m(a);
        console.log("Decoded payload:", c);
        const f = c.sub, b = c.unique_name || c.name || "User";
        let p = [];
        try {
          console.log("Fetching roles for userId:", f);
          const N = await ie.getRoles(f);
          console.log("Fetched userRoles:", N), p = N.map((T) => T.name), console.log("Mapped roles:", p);
        } catch (N) {
          console.error("Failed to fetch roles", N);
        }
        o({
          id: f,
          username: b,
          email: c.email || "",
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
  function m(a) {
    var c = a.split(".")[1], f = c.replace(/-/g, "+").replace(/_/g, "/"), b = decodeURIComponent(window.atob(f).split("").map(function(p) {
      return "%" + ("00" + p.charCodeAt(0).toString(16)).slice(-2);
    }).join(""));
    return JSON.parse(b);
  }
  const h = {
    user: r,
    isAuthenticated: !!r,
    isLoading: d,
    login: async (a) => {
      l(!0);
      try {
        await ae.login(a), await n();
      } catch (c) {
        throw o(null), c;
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
  return /* @__PURE__ */ s.jsx(ce.Provider, { value: h, children: t });
};
class _e {
  connection = null;
  listeners = [];
  async start() {
    if (!this.connection) {
      this.connection = new xe.HubConnectionBuilder().withUrl("/hubs/notifications", {
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
  stop() {
    this.connection && (this.connection.stop(), this.connection = null);
  }
  subscribe(r) {
    return this.listeners.push(r), () => {
      this.listeners = this.listeners.filter((o) => o !== r);
    };
  }
}
const C = new _e(), Ee = async () => {
  const t = localStorage.getItem("accessToken");
  if (!t) return [];
  try {
    const r = await fetch("/api/player/notifications", {
      headers: {
        Authorization: `Bearer ${t}`
      }
    });
    if (r.status === 401) return [];
    if (!r.ok) throw new Error("Failed to fetch notifications");
    return await r.json();
  } catch (r) {
    return console.error("Error fetching notifications:", r), [];
  }
}, Se = async (t) => {
  const r = localStorage.getItem("accessToken");
  if (r)
    try {
      await fetch(`/api/player/notifications/${t}/read`, {
        method: "POST",
        headers: {
          Authorization: `Bearer ${r}`
        }
      });
    } catch (o) {
      console.error("Error marking notification as read:", o);
    }
}, Ae = async () => {
  const t = localStorage.getItem("accessToken");
  if (t)
    try {
      await fetch("/api/player/notifications/read-all", {
        method: "POST",
        headers: {
          Authorization: `Bearer ${t}`
        }
      });
    } catch (r) {
      console.error("Error marking all notifications as read:", r);
    }
}, le = L(void 0), Le = ({ children: t }) => {
  const [r, o] = j([]), { isAuthenticated: d } = V(), l = r.filter((h) => !h.read).length, n = A.useCallback((h) => {
    const a = {
      ...h,
      id: Math.random().toString(36).substring(2, 9),
      timestamp: /* @__PURE__ */ new Date(),
      read: !1
    };
    o((c) => [a, ...c].slice(0, 50));
  }, []);
  A.useEffect(() => {
    let h = !0;
    (async () => {
      if (!d) {
        o([]), C.stop();
        return;
      }
      C.stop(), await C.start();
      const f = await Ee();
      if (h) {
        const b = f.map((p) => ({
          id: p.id,
          title: p.title,
          message: p.message,
          type: p.type,
          timestamp: new Date(p.createdAt),
          read: p.isRead
        }));
        o((p) => {
          const N = new Set(p.map((R) => R.id));
          return [...b.filter((R) => !N.has(R.id)), ...p].sort((R, O) => O.timestamp.getTime() - R.timestamp.getTime());
        });
      }
    })();
    const c = C.subscribe((f) => {
      n({
        title: f.title,
        message: f.message,
        type: f.type
      });
    });
    return () => {
      h = !1, c();
    };
  }, [d, n]);
  const m = A.useCallback((h) => {
    Se(h), o((a) => a.map((c) => c.id === h ? { ...c, read: !0 } : c));
  }, []), u = A.useCallback(() => {
    Ae(), o((h) => h.map((a) => ({ ...a, read: !0 })));
  }, []), x = A.useCallback(() => {
    o([]);
  }, []);
  return /* @__PURE__ */ s.jsx(le.Provider, { value: {
    notifications: r,
    unreadCount: l,
    addNotification: n,
    markAsRead: m,
    markAllAsRead: u,
    clearNotifications: x
  }, children: t });
}, Ce = () => {
  const t = D(le);
  if (!t)
    throw new Error("useNotifications must be used within a NotificationProvider");
  return t;
}, ue = L(void 0), De = ({ children: t }) => {
  const [r, o] = j(null), [d, l] = j(!0), [n, m] = j([]), [u, x] = j([]);
  return P(() => {
    (async () => {
      try {
        const a = await fetch("/api/config/ui");
        if (a.ok) {
          const c = await a.json();
          o(c);
          const f = `theme-${c.theme || "dark"}`;
          document.documentElement.className = f;
        }
      } catch (a) {
        console.error("Failed to fetch UI settings:", a), document.documentElement.className = "theme-dark";
      } finally {
        l(!1);
      }
    })(), Array.isArray(window.__QUASAR_CUSTOM_MENU__) && m(window.__QUASAR_CUSTOM_MENU__), Array.isArray(window.__QUASAR_CUSTOM_ROUTES__) && x(window.__QUASAR_CUSTOM_ROUTES__);
  }, []), /* @__PURE__ */ s.jsx(ue.Provider, { value: { settings: r, isLoading: d, customMenu: n, customRoutes: u }, children: t });
}, de = () => {
  const t = D(ue);
  if (!t)
    throw new Error("useUi must be used within a UiProvider");
  return t;
}, me = L(void 0), Ye = ({ children: t }) => {
  const [r, o] = j([]), [d, l] = j(!0);
  P(() => {
    (async () => {
      try {
        const u = await Te.list();
        o(u);
      } catch (u) {
        console.error("Failed to fetch features:", u);
      } finally {
        l(!1);
      }
    })();
  }, []);
  const n = (m) => r.some((u) => u.id === m);
  return /* @__PURE__ */ s.jsx(me.Provider, { value: { features: r, isLoading: d, hasFeature: n }, children: t });
}, Pe = () => {
  const t = D(me);
  if (!t)
    throw new Error("useFeatures must be used within a FeatureProvider");
  return t;
}, Oe = () => {
  const { notifications: t, unreadCount: r, addNotification: o, markAsRead: d, markAllAsRead: l } = Ce(), [n, m] = j(!1), u = ne(null);
  P(() => {
    const a = C.subscribe((c) => {
      o({
        title: c.title,
        message: c.message,
        type: c.type
      });
    });
    return C.start(), () => {
      a();
    };
  }, [o]), P(() => {
    const a = (c) => {
      u.current && !u.current.contains(c.target) && m(!1);
    };
    return n && document.addEventListener("mousedown", a), () => {
      document.removeEventListener("mousedown", a);
    };
  }, [n]);
  const x = (a) => {
    const f = (/* @__PURE__ */ new Date()).getTime() - a.getTime(), b = Math.floor(f / 6e4);
    if (b < 1) return "Just now";
    if (b < 60) return `${b}m ago`;
    const p = Math.floor(b / 60);
    return p < 24 ? `${p}h ago` : a.toLocaleDateString();
  }, h = (a) => {
    d(a);
  };
  return /* @__PURE__ */ s.jsxs("div", { className: "notifications-container", ref: u, children: [
    /* @__PURE__ */ s.jsxs(
      "button",
      {
        className: "bell-button",
        onClick: () => m(!n),
        "aria-label": "Notifications",
        children: [
          /* @__PURE__ */ s.jsx("span", { children: "🔔" }),
          r > 0 && /* @__PURE__ */ s.jsx("span", { className: "notification-badge", children: r > 9 ? "9+" : r })
        ]
      }
    ),
    n && /* @__PURE__ */ s.jsxs("div", { className: "notifications-dropdown", children: [
      /* @__PURE__ */ s.jsxs("div", { className: "notifications-header", children: [
        /* @__PURE__ */ s.jsx("h3", { children: "Notifications" }),
        r > 0 && /* @__PURE__ */ s.jsx("button", { className: "mark-all-btn", onClick: l, children: "Mark all as read" })
      ] }),
      /* @__PURE__ */ s.jsx("div", { className: "notifications-list", children: t.length === 0 ? /* @__PURE__ */ s.jsx("div", { className: "empty-notifications", children: "No notifications yet" }) : t.map((a) => /* @__PURE__ */ s.jsxs(
        "div",
        {
          className: `notification-item ${a.read ? "read" : "unread"}`,
          onClick: () => h(a.id),
          children: [
            /* @__PURE__ */ s.jsxs("div", { className: "notification-item-header", children: [
              /* @__PURE__ */ s.jsx("span", { className: `notification-title type-${a.type}`, children: a.title }),
              /* @__PURE__ */ s.jsx("span", { className: "notification-time", children: x(a.timestamp) })
            ] }),
            /* @__PURE__ */ s.jsx("div", { className: "notification-message", children: a.message })
          ]
        },
        a.id
      )) })
    ] })
  ] });
}, Ie = () => {
  const { user: t, logout: r } = V(), [o, d] = j(!1), [l, n] = j(null), [m, u] = j(!1), [x, h] = j(!1), a = oe(), c = ne(null), f = async () => {
    await r(), a("/login");
  }, b = async () => {
    u(!0), d(!1);
  }, p = async () => {
    try {
      const y = await ie.resetOwnPassword();
      n(y), u(!1);
    } catch (y) {
      console.error("Failed to change password:", y), alert("Failed to change password"), u(!1);
    }
  };
  P(() => {
    const y = (E) => {
      c.current && !c.current.contains(E.target) && d(!1);
    };
    return document.addEventListener("mousedown", y), () => {
      document.removeEventListener("mousedown", y);
    };
  }, []);
  const { pathname: N } = ke(), { customMenu: T } = de(), O = (() => {
    const E = (T.length > 0 ? T : window.__QUASAR_CUSTOM_MENU__ || []).flatMap((I) => I.items).find((I) => I.path === N);
    if (E) return E.label;
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
  return /* @__PURE__ */ s.jsxs(s.Fragment, { children: [
    /* @__PURE__ */ s.jsxs("header", { className: "app-header", children: [
      /* @__PURE__ */ s.jsx("div", { className: "header-left", children: /* @__PURE__ */ s.jsx("h2", { className: "page-title", children: O }) }),
      /* @__PURE__ */ s.jsxs("div", { className: "header-right", children: [
        /* @__PURE__ */ s.jsx(Oe, {}),
        /* @__PURE__ */ s.jsxs("div", { className: "user-profile", children: [
          /* @__PURE__ */ s.jsxs("div", { className: "user-info", children: [
            /* @__PURE__ */ s.jsx("span", { className: "user-name", children: t?.username || "Guest" }),
            /* @__PURE__ */ s.jsx("span", { className: "user-role", children: t?.roles?.[0] || "User" })
          ] }),
          /* @__PURE__ */ s.jsx("div", { className: "user-avatar", children: t?.username?.charAt(0).toUpperCase() || "U" })
        ] }),
        /* @__PURE__ */ s.jsxs("div", { className: "menu-container", ref: c, children: [
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
            /* @__PURE__ */ s.jsx("button", { onClick: b, className: "dropdown-item", children: "Change Password" }),
            /* @__PURE__ */ s.jsx("button", { onClick: f, className: "dropdown-item danger", children: "Sign Out" })
          ] })
        ] })
      ] })
    ] }),
    m && /* @__PURE__ */ s.jsx("div", { className: "modal-overlay", onClick: () => u(!1), children: /* @__PURE__ */ s.jsxs("div", { className: "modal", onClick: (y) => y.stopPropagation(), children: [
      /* @__PURE__ */ s.jsxs("div", { className: "modal-header", children: [
        /* @__PURE__ */ s.jsx("h2", { className: "modal-title", children: "Confirm Password Change" }),
        /* @__PURE__ */ s.jsx("button", { className: "modal-close", onClick: () => u(!1), children: "×" })
      ] }),
      /* @__PURE__ */ s.jsxs("div", { className: "modal-body", children: [
        /* @__PURE__ */ s.jsx("p", { children: "Generate a new password?" }),
        /* @__PURE__ */ s.jsx("p", { className: "text-muted", style: { marginTop: "var(--spacing-md)" }, children: "You will be logged out and need to login with the new password." }),
        /* @__PURE__ */ s.jsxs("div", { style: { display: "flex", gap: "var(--spacing-md)", marginTop: "var(--spacing-lg)" }, children: [
          /* @__PURE__ */ s.jsx(
            "button",
            {
              className: "btn btn-secondary",
              onClick: () => u(!1),
              style: { flex: 1 },
              children: "Cancel"
            }
          ),
          /* @__PURE__ */ s.jsx(
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
    l && /* @__PURE__ */ s.jsx("div", { className: "modal-overlay", onClick: () => {
      n(null), f();
    }, children: /* @__PURE__ */ s.jsxs("div", { className: "modal", onClick: (y) => y.stopPropagation(), children: [
      /* @__PURE__ */ s.jsx("div", { className: "modal-header", children: /* @__PURE__ */ s.jsx("h2", { className: "modal-title", children: "Password Changed Successfully" }) }),
      /* @__PURE__ */ s.jsxs("div", { className: "modal-body", children: [
        /* @__PURE__ */ s.jsx("p", { children: "Your new password is:" }),
        /* @__PURE__ */ s.jsxs("div", { className: "password-display", style: { display: "flex", alignItems: "center", gap: "var(--spacing-md)", padding: "var(--spacing-md)", background: "var(--color-bg-secondary)", borderRadius: "var(--radius-md)" }, children: [
          /* @__PURE__ */ s.jsx("code", { style: { flex: 1, fontSize: "var(--font-size-base)", fontWeight: "bold" }, children: l }),
          /* @__PURE__ */ s.jsx(
            "button",
            {
              className: "btn btn-sm btn-secondary",
              onClick: () => {
                navigator.clipboard.writeText(l), h(!0), setTimeout(() => h(!1), 2e3);
              },
              children: x ? "✓ Copied!" : "📋 Copy"
            }
          )
        ] }),
        /* @__PURE__ */ s.jsx("p", { className: "warning", style: { marginTop: "var(--spacing-md)", padding: "var(--spacing-md)", background: "var(--color-warning-bg)", border: "1px solid var(--color-warning)", borderRadius: "var(--radius-md)" }, children: "⚠️ Save this password now. You will be logged out." }),
        /* @__PURE__ */ s.jsx(
          "button",
          {
            className: "btn btn-primary",
            onClick: () => {
              n(null), f();
            },
            style: { marginTop: "var(--spacing-lg)", width: "100%" },
            children: "Logout Now"
          }
        )
      ] })
    ] }) })
  ] });
}, ze = ({ children: t }) => {
  const { user: r, logout: o } = V(), { settings: d, customMenu: l } = de(), { hasFeature: n } = Pe(), m = oe(), u = async () => {
    await o(), m("/login");
  };
  return /* @__PURE__ */ s.jsxs("div", { className: "main-layout", children: [
    /* @__PURE__ */ s.jsxs("aside", { className: "sidebar", children: [
      /* @__PURE__ */ s.jsx("div", { className: "sidebar-header", children: /* @__PURE__ */ s.jsxs("div", { className: "logo", children: [
        /* @__PURE__ */ s.jsx("div", { className: "logo-icon", children: d?.logoSymbol || "Q" }),
        /* @__PURE__ */ s.jsx("span", { className: "logo-text", children: d?.applicationName || "Quasar" })
      ] }) }),
      /* @__PURE__ */ s.jsxs("nav", { className: "sidebar-nav", children: [
        l.map((x, h) => /* @__PURE__ */ s.jsxs("div", { className: "nav-section", children: [
          x.title && /* @__PURE__ */ s.jsx("h3", { className: "nav-section-title", children: x.title }),
          x.items.map((a, c) => a.roles && !a.roles.some((f) => r?.roles?.includes(f)) || a.feature && !n(a.feature) ? null : /* @__PURE__ */ s.jsx(_, { to: a.path, className: "nav-link", children: /* @__PURE__ */ s.jsx("span", { children: a.label }) }, `custom-item-${c}-${a.path}`))
        ] }, `custom-${h}`)),
        r?.roles?.includes("administrator") && d?.showAdminMenu !== !1 && /* @__PURE__ */ s.jsxs("div", { className: "nav-section", children: [
          /* @__PURE__ */ s.jsx("h3", { className: "nav-section-title", children: "Administration" }),
          /* @__PURE__ */ s.jsx(_, { to: "/users", className: "nav-link", children: /* @__PURE__ */ s.jsx("span", { children: "Users" }) }),
          /* @__PURE__ */ s.jsx(_, { to: "/roles", className: "nav-link", children: /* @__PURE__ */ s.jsx("span", { children: "Roles" }) }),
          /* @__PURE__ */ s.jsx(_, { to: "/features", className: "nav-link", children: /* @__PURE__ */ s.jsx("span", { children: "Features" }) }),
          /* @__PURE__ */ s.jsx(_, { to: "/logs", className: "nav-link", children: /* @__PURE__ */ s.jsx("span", { children: "Logs" }) }),
          /* @__PURE__ */ s.jsx(_, { to: "/sessions", className: "nav-link", children: /* @__PURE__ */ s.jsx("span", { children: "Sessions" }) }),
          n("scheduler") && /* @__PURE__ */ s.jsx(_, { to: "/jobs", className: "nav-link", children: /* @__PURE__ */ s.jsx("span", { children: "Jobs" }) }),
          n("telemetry") && /* @__PURE__ */ s.jsx(_, { to: "/metrics", className: "nav-link", children: /* @__PURE__ */ s.jsx("span", { children: "Metrics" }) })
        ] })
      ] }),
      /* @__PURE__ */ s.jsx("div", { className: "sidebar-footer", children: /* @__PURE__ */ s.jsx("button", { onClick: u, className: "btn btn-secondary w-full btn-sm", children: "Sign Out" }) })
    ] }),
    /* @__PURE__ */ s.jsxs("div", { className: "content-wrapper", children: [
      /* @__PURE__ */ s.jsx(Ie, {}),
      /* @__PURE__ */ s.jsx("main", { className: `main-content ${ee?.type?.layoutOptions?.noPadding ? "no-padding" : ""}`, children: t || /* @__PURE__ */ s.jsx(ee, {}) })
    ] })
  ] });
};
export {
  Me as AuthProvider,
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
