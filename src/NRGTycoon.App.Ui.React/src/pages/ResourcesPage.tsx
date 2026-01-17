import React, { useState, useEffect, Suspense, lazy, useCallback, useRef } from "react";
import { getCompanyDashboard } from "../api/company";
import type { CompanyDashboard } from "../types";

// Lazy load Globe to avoid SSR/Initial load issues with heavy 3D libs
const Globe = lazy(() => import("react-globe.gl"));

export const ResourcesPage: React.FC = () => {
    const [dashboard, setDashboard] = useState<CompanyDashboard | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [countries, setCountries] = useState<any[]>([]);
    const [cities, setCities] = useState<any[]>([]);
    const [altitude, setAltitude] = useState(2.5);
    const lastAltitudeRef = useRef(2.5);
    const requestRef = useRef<number | null>(null);

    // Throttled altitude update to prevent excessive re-renders during zoom/interaction
    const throttledSetAltitude = useCallback((newAlt: number) => {
        lastAltitudeRef.current = newAlt;
        if (requestRef.current !== null) return;

        requestRef.current = requestAnimationFrame(() => {
            setAltitude(lastAltitudeRef.current);
            requestRef.current = null;
        });
    }, []);

    useEffect(() => {
        return () => {
            if (requestRef.current !== null) cancelAnimationFrame(requestRef.current);
        };
    }, []);

    useEffect(() => {
        loadDashboard();

        // Use Official unpkg datasets for better reliability
        fetch("https://unpkg.com/globe.gl/example/datasets/ne_110m_admin_0_countries.geojson")
            .then(res => res.json())
            .then(data => {
                if (data.features) {
                    setCountries(data.features);
                }
            })
            .catch(err => console.error("Failed to load borders:", err));

        // Load More Cities (Lower pop filter for more density)
        fetch("https://unpkg.com/globe.gl/example/datasets/ne_110m_populated_places_simple.geojson")
            .then(res => res.json())
            .then(data => {
                if (data.features) {
                    const cityList = data.features
                        .filter((c: any) => c.properties.pop_max > 500000) // Lower filter (500k) for density
                        .map((c: any) => ({
                            lat: c.geometry.coordinates[1],
                            lng: c.geometry.coordinates[0],
                            name: c.properties.name,
                            size: 0.15, // Baseline size
                            dotRadius: 0.08
                        }));
                    setCities(cityList);
                }
            })
            .catch(err => console.error("Failed to load cities:", err));
    }, []);

    async function loadDashboard() {
        try {
            setLoading(true);
            const data = await getCompanyDashboard();
            if (data) {
                setDashboard(data);
            }
        } catch (err) {
            setError(err instanceof Error ? err.message : "Failed to load company data");
        } finally {
            setLoading(false);
        }
    }

    const formatAmount = (val: number) => {
        return new Intl.NumberFormat("en-US", {
            maximumFractionDigits: 0,
        }).format(val);
    };

    // Scaling labels based on altitude
    const getLabelSize = () => Math.max(0.1, 0.3 * altitude);

    if (loading) {
        return (
            <div className="flex items-center justify-center h-64">
                <div className="spinner" style={{ width: "40px", height: "40px" }}></div>
            </div>
        );
    }

    return (
        <div style={{ position: "relative", width: "100%", height: "calc(100vh - 100px)", overflow: "hidden", background: "#000" }}>
            {/* 3D Globe Container */}
            <div style={{ position: "absolute", top: 0, left: 0, width: "100%", height: "100%" }}>
                <Suspense fallback={<div className="flex items-center justify-center h-full">Loading Earth...</div>}>
                    <Globe
                        globeTileEngineUrl={(x, y, l) => `https://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/${l}/${y}/${x}`}
                        backgroundImageUrl="//unpkg.com/three-globe/example/img/night-sky.png"
                        atmosphereColor="#3a228a"
                        atmosphereAltitude={0.15}
                        onZoom={({ altitude }) => throttledSetAltitude(altitude)}

                        // Country Borders
                        polygonsData={countries}
                        polygonAltitude={0.006} // Keep borders nearly flush with surface
                        polygonSideColor={() => "rgba(0, 0, 0, 0)"}
                        polygonCapColor={() => "rgba(255, 255, 255, 0.04)"}
                        polygonStrokeColor={() => "#ffffff"}
                        polygonsTransitionDuration={0} // Performance boost
                        polygonLabel={({ properties: d }: any) => `
                            <b>${d.ADMIN} (${d.ISO_A2})</b>
                        `}

                        // Cities
                        labelsData={cities}
                        labelLat={(d: any) => d.lat}
                        labelLng={(d: any) => d.lng}
                        labelText={(d: any) => d.name}
                        labelSize={getLabelSize}
                        labelDotRadius={0.1}
                        labelColor={() => "rgba(255, 255, 255, 1.0)"}
                        labelResolution={2}
                        labelIncludeDot={true}
                        labelsTransitionDuration={0} // Performance boost
                    />
                </Suspense>
            </div>

            {/* Resources HUD Overlay */}
            <div style={{
                position: "absolute",
                top: "20px",
                left: "20px",
                right: "20px",
                display: "flex",
                justifyContent: "space-between",
                pointerEvents: "none"
            }}>
                <div style={{ display: "flex", gap: "16px", pointerEvents: "auto" }}>
                    {/* Oil Counter */}
                    <div className="card" style={{
                        background: "rgba(15, 23, 42, 0.7)",
                        backdropFilter: "blur(8px)",
                        border: "1px solid rgba(148, 163, 184, 0.2)",
                        padding: "12px 20px",
                        borderRadius: "12px",
                        display: "flex",
                        alignItems: "center",
                        gap: "12px",
                        minWidth: "140px"
                    }}>
                        <div style={{ fontSize: "24px" }}>🛢️</div>
                        <div>
                            <div style={{ fontSize: "12px", color: "#94a3b8", textTransform: "uppercase", letterSpacing: "1px" }}>Oil</div>
                            <div style={{ fontSize: "20px", fontWeight: "bold", color: "#e2e8f0" }}>
                                {dashboard ? formatAmount(dashboard.oil) : "0"}
                            </div>
                        </div>
                    </div>

                    {/* Gas Counter */}
                    <div className="card" style={{
                        background: "rgba(15, 23, 42, 0.7)",
                        backdropFilter: "blur(8px)",
                        border: "1px solid rgba(148, 163, 184, 0.2)",
                        padding: "12px 20px",
                        borderRadius: "12px",
                        display: "flex",
                        alignItems: "center",
                        gap: "12px",
                        minWidth: "140px"
                    }}>
                        <div style={{ fontSize: "24px" }}>💨</div>
                        <div>
                            <div style={{ fontSize: "12px", color: "#94a3b8", textTransform: "uppercase", letterSpacing: "1px" }}>Gas</div>
                            <div style={{ fontSize: "20px", fontWeight: "bold", color: "#38bdf8" }}>
                                {dashboard ? formatAmount(dashboard.gas) : "0"}
                            </div>
                        </div>
                    </div>

                    {/* Uranium Counter */}
                    <div className="card" style={{
                        background: "rgba(15, 23, 42, 0.7)",
                        backdropFilter: "blur(8px)",
                        border: "1px solid rgba(148, 163, 184, 0.2)",
                        padding: "12px 20px",
                        borderRadius: "12px",
                        display: "flex",
                        alignItems: "center",
                        gap: "12px",
                        minWidth: "140px"
                    }}>
                        <div style={{ fontSize: "24px" }}>☢️</div>
                        <div>
                            <div style={{ fontSize: "12px", color: "#94a3b8", textTransform: "uppercase", letterSpacing: "1px" }}>Uranium</div>
                            <div style={{ fontSize: "20px", fontWeight: "bold", color: "#4ade80" }}>
                                {dashboard ? formatAmount(dashboard.uranium) : "0"}
                            </div>
                        </div>
                    </div>
                </div>

                {/* Map Legend/Focus Info */}
                <div className="card" style={{
                    background: "rgba(15, 23, 42, 0.7)",
                    backdropFilter: "blur(8px)",
                    border: "1px solid rgba(148, 163, 184, 0.2)",
                    padding: "12px 20px",
                    borderRadius: "12px",
                    color: "#94a3b8",
                    fontSize: "14px",
                    display: "flex",
                    alignItems: "center",
                    pointerEvents: "auto"
                }}>
                    🌐 Global Resource Map
                </div>
            </div>

            {/* Error Message */}
            {error && (
                <div style={{ position: "absolute", bottom: "20px", left: "50%", transform: "translateX(-50%)" }}>
                    <div className="alert alert-error">{error}</div>
                </div>
            )}
        </div>
    );
};

export default ResourcesPage;
