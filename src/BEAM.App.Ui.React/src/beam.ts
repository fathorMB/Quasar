import { DashboardPage } from "./pages/DashboardPage";
import { DeviceConfigPage } from "./pages/DeviceConfigPage";
import type { CustomNavSection, CustomRoute } from "./types";

declare global {
  interface Window {
    __QUASAR_CUSTOM_MENU__?: CustomNavSection[];
    __QUASAR_CUSTOM_ROUTES__?: CustomRoute[];
    BeamComponents?: {
      DashboardPage: typeof DashboardPage;
      DeviceConfigPage: typeof DeviceConfigPage;
    };
  }
}

// Make components available globally
window.BeamComponents = {
  DashboardPage,
  DeviceConfigPage,
};

window.__QUASAR_CUSTOM_MENU__ = [
  {
    title: "Devices",
    items: [
      { label: "Dashboard", path: "/" },
      { label: "Configuration", path: "/device" }
    ],
  },
];

window.__QUASAR_CUSTOM_ROUTES__ = [
  {
    path: "/",
    component: DashboardPage,
    index: true,
  },
  {
    path: "/device/:id",
    component: DeviceConfigPage,
  },
  {
    path: "/device",
    component: DeviceConfigPage,
  },
];
