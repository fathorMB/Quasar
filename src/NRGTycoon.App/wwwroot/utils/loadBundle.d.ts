declare global {
    interface Window {
        __QUASAR_BUNDLE_LOADED__?: boolean;
    }
}
export declare function fetchUiConfig(): Promise<void>;
