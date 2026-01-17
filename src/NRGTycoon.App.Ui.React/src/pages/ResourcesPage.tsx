import React, { useState, useEffect } from "react";
import { MapContainer, TileLayer, Marker, Popup } from "react-leaflet";
import { getCompanyDashboard } from "../api/company";
import type { CompanyDashboard } from "../types";
import L from "leaflet";

// Fix default marker icon issue with Leaflet
delete (L.Icon.Default.prototype as any)._getIconUrl;
L.Icon.Default.mergeOptions({
    iconUrl: "https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png",
    iconRetinaUrl: "https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon-2x.png",
    shadowUrl: "https://unpkg.com/leaflet@1.9.4/dist/images/marker-shadow.png",
});

export const ResourcesPage: React.FC = () => {
    const [dashboard, setDashboard] = useState<CompanyDashboard | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        loadDashboard();
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

    // Sample resource locations for demonstration
    const resourceLocations = [
        { lat: 29.7604, lng: -95.3698, name: "Houston Oil Field", type: "oil" },
        { lat: 33.4484, lng: -112.0740, name: "Phoenix Gas Reserve", type: "gas" },
        { lat: 47.6062, lng: -122.3321, name: "Seattle Uranium Mine", type: "uranium" },
        { lat: 51.5074, lng: -0.1278, name: "London Energy Hub", type: "oil" },
        { lat: 35.6762, lng: 139.6503, name: "Tokyo Power Plant", type: "uranium" },
        { lat: -33.8688, lng: 151.2093, name: "Sydney Gas Field", type: "gas" },
    ];

    if (loading) {
        return (
            <div className="flex items-center justify-center h-64">
                <div className="spinner" style={{ width: "40px", height: "40px" }}></div>
            </div>
        );
    }

    return (
        <div style={{ position: "relative", width: "100%", height: "calc(100vh - 100px)", overflow: "hidden" }}>
            {/* Map Container */}
            <MapContainer
                center={[20, 0]}
                zoom={2}
                style={{ width: "100%", height: "100%" }}
                zoomControl={true}
            >
                <TileLayer
                    attribution='&copy; <a href="https://carto.com/">CartoDB</a>'
                    url="https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png"
                />
                {resourceLocations.map((location, idx) => (
                    <Marker key={idx} position={[location.lat, location.lng]}>
                        <Popup>
                            <div style={{ color: "#1e293b" }}>
                                <strong>{location.name}</strong>
                                <br />
                                Type: {location.type}
                            </div>
                        </Popup>
                    </Marker>
                ))}
            </MapContainer>

            {/* Resources HUD Overlay */}
            <div style={{
                position: "absolute",
                top: "20px",
                left: "20px",
                right: "20px",
                display: "flex",
                justifyContent: "space-between",
                pointerEvents: "none",
                zIndex: 1000
            }}>
                <div style={{ display: "flex", gap: "16px", pointerEvents: "auto" }}>
                    {/* Oil Counter */}
                    <div className="card" style={{
                        background: "rgba(15, 23, 42, 0.9)",
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
                        background: "rgba(15, 23, 42, 0.9)",
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
                        background: "rgba(15, 23, 42, 0.9)",
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

                {/* Map Legend */}
                <div className="card" style={{
                    background: "rgba(15, 23, 42, 0.9)",
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
                <div style={{ position: "absolute", bottom: "20px", left: "50%", transform: "translateX(-50%)", zIndex: 1000 }}>
                    <div className="alert alert-error">{error}</div>
                </div>
            )}
        </div>
    );
};

export default ResourcesPage;
