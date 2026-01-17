import { DashboardPage } from "./pages/DashboardPage";
import { ResourcesPage } from "./pages/ResourcesPage";
import type { CustomNavSection, CustomRoute } from "./types";

declare global {
    interface Window {
        __QUASAR_CUSTOM_MENU__?: CustomNavSection[];
        __QUASAR_CUSTOM_ROUTES__?: CustomRoute[];
        NRGTycoonComponents?: {
            DashboardPage: typeof DashboardPage;
            ResourcesPage: typeof ResourcesPage;
        };
    }
}

// Make components available globally
window.NRGTycoonComponents = {
    DashboardPage,
    ResourcesPage,
};

// Register custom navigation menu - visible to players and admins
window.__QUASAR_CUSTOM_MENU__ = [
    {
        title: "NRG Tycoon",
        items: [
            { label: "Company", path: "/", roles: ["player", "administrator"] },
            { label: "Resources", path: "/resources", roles: ["player", "administrator"] },
        ],
    },
];

// Register custom routes - accessible to players and admins
window.__QUASAR_CUSTOM_ROUTES__ = [
    {
        path: "/",
        component: DashboardPage,
        index: true,
        roles: ["player", "administrator"],
    },
    {
        path: "/resources",
        component: ResourcesPage,
        roles: ["player", "administrator"],
    },
];
