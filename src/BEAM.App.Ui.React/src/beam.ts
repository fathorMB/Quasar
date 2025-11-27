import { DashboardPage } from "./pages/DashboardPage";
import type { CustomNavSection, CustomRoute } from "./types";

declare global {
  interface Window {
    __QUASAR_CUSTOM_MENU__?: CustomNavSection[];
    __QUASAR_CUSTOM_ROUTES__?: CustomRoute[];
    BeamComponents?: {
      DashboardPage: typeof DashboardPage;
    };
  }
}

// Make components available globally
window.BeamComponents = {
  DashboardPage,
};

window.__QUASAR_CUSTOM_MENU__ = [
  {
    title: "Devices",
    items: [{ label: "Dashboard", path: "/" }],
  },
];

window.__QUASAR_CUSTOM_ROUTES__ = [
  {
    path: "/",
    component: DashboardPage,
    index: true,
  },
];
