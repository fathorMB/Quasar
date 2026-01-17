import React, { useState, useEffect } from "react";
import { getCompanyDashboard, createCompany } from "../api/company";
import type { CompanyDashboard, BalanceMovement } from "../types";

export const DashboardPage: React.FC = () => {
    const [dashboard, setDashboard] = useState<CompanyDashboard | null>(null);
    const [loading, setLoading] = useState(true);
    const [showSetup, setShowSetup] = useState(false);
    const [companyName, setCompanyName] = useState("");

    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        loadDashboard();
    }, []);

    async function loadDashboard() {
        try {
            setLoading(true);
            setError(null);
            const data = await getCompanyDashboard();
            if (data) {
                setDashboard(data);
                setShowSetup(false);
            } else {
                setShowSetup(true);
            }
        } catch (err) {
            setError(err instanceof Error ? err.message : "Failed to load dashboard");
        } finally {
            setLoading(false);
        }
    }

    async function handleCreateCompany(e: React.FormEvent) {
        e.preventDefault();
        console.log("HandleCreateCompany triggered, name:", companyName);
        if (!companyName.trim()) return;

        try {
            setError(null);
            console.log("Calling createCompany API...");
            await createCompany({ name: companyName.trim() });
            console.log("Company created successfully, clearing name and reloading dashboard");
            setCompanyName("");
            await loadDashboard();
        } catch (err) {
            setError(err instanceof Error ? err.message : "Failed to create company");
        }
    }



    function formatCurrency(amount: number): string {
        return new Intl.NumberFormat("en-US", {
            style: "currency",
            currency: "USD",
            minimumFractionDigits: 0,
            maximumFractionDigits: 0,
        }).format(amount);
    }

    function formatDate(dateString: string): string {
        return new Date(dateString).toLocaleDateString("en-US", {
            year: "numeric",
            month: "short",
            day: "numeric",
            hour: "2-digit",
            minute: "2-digit",
        });
    }

    if (loading) {
        return (
            <div className="flex items-center justify-center h-64">
                <div className="spinner" style={{ width: "40px", height: "40px" }}></div>
            </div>
        );
    }

    // Company Setup Form
    if (showSetup) {
        return (
            <div style={{ maxWidth: "500px", margin: "60px auto", padding: "0 20px" }}>
                <div className="card" style={{ background: "rgba(30, 41, 59, 0.8)", borderRadius: "12px", overflow: "hidden" }}>
                    <div style={{ padding: "24px 24px 0" }}>
                        <h2 style={{ fontSize: "24px", fontWeight: "bold", marginBottom: "8px", display: "flex", alignItems: "center", gap: "8px" }}>
                            ⚡ Welcome to NRG Tycoon!
                        </h2>
                        <p style={{ color: "#94a3b8", marginBottom: "24px", lineHeight: "1.6" }}>
                            Start your energy empire by creating your company. You'll receive
                            <strong style={{ color: "#4ade80" }}> $10,000</strong> as initial capital.
                        </p>
                    </div>

                    {error && (
                        <div style={{ padding: "0 24px" }}>
                            <div className="alert alert-error" style={{ marginBottom: "16px" }}>{error}</div>
                        </div>
                    )}

                    <form onSubmit={handleCreateCompany} style={{ padding: "0 24px 24px" }}>
                        <div style={{ marginBottom: "20px" }}>
                            <label style={{ display: "block", marginBottom: "8px", fontWeight: "500", color: "#e2e8f0" }}>
                                Company Name
                            </label>
                            <input
                                type="text"
                                className="form-input"
                                placeholder="Enter your company name..."
                                value={companyName}
                                onChange={(e) => setCompanyName(e.target.value)}
                                required
                                maxLength={200}
                                style={{
                                    width: "100%",
                                    padding: "12px 16px",
                                    borderRadius: "8px",
                                    background: "rgba(15, 23, 42, 0.8)",
                                    border: "1px solid rgba(100, 116, 139, 0.5)",
                                    color: "#fff",
                                    fontSize: "16px"
                                }}
                            />
                        </div>
                        <button
                            type="submit"
                            className="btn btn-primary"
                            style={{
                                width: "100%",
                                padding: "14px 24px",
                                fontSize: "16px",
                                fontWeight: "600",
                                borderRadius: "8px"
                            }}
                        >
                            🚀 Create Company
                        </button>
                    </form>
                </div>
            </div>
        );
    }

    // Dashboard View
    return (
        <div style={{ padding: "20px", maxWidth: "1200px", margin: "0 auto" }}>
            <div style={{ marginBottom: "24px" }}>
                <div style={{ display: "flex", alignItems: "center", justifyContent: "space-between" }}>
                    <div>
                        <h1 style={{ fontSize: "28px", fontWeight: "bold", display: "flex", alignItems: "center", gap: "12px", marginBottom: "4px" }}>
                            ⚡ {dashboard?.name}
                        </h1>
                        <p style={{ color: "#94a3b8", fontSize: "14px" }}>
                            Founded {dashboard && formatDate(dashboard.createdAt)}
                        </p>
                    </div>
                </div>
            </div>

            {error && (
                <div className="alert alert-error" style={{ marginBottom: "16px" }}>{error}</div>
            )}

            {/* Balance Card */}
            <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fit, minmax(280px, 1fr))", gap: "16px", marginBottom: "24px" }}>
                <div className="card" style={{ background: "rgba(30, 41, 59, 0.8)", borderRadius: "12px", padding: "24px" }}>
                    <div style={{ color: "#94a3b8", fontSize: "14px", marginBottom: "8px" }}>Current Balance</div>
                    <div style={{ fontSize: "36px", fontWeight: "bold", color: "#4ade80" }}>
                        {dashboard && formatCurrency(dashboard.balance)}
                    </div>
                </div>
            </div>

            {/* Recent Transactions */}
            <div className="card" style={{ background: "rgba(30, 41, 59, 0.8)", borderRadius: "12px", overflow: "hidden" }}>
                <div style={{ padding: "20px 24px", borderBottom: "1px solid rgba(100, 116, 139, 0.3)" }}>
                    <h3 style={{ fontSize: "18px", fontWeight: "bold" }}>Recent Transactions</h3>
                </div>
                <div>
                    {dashboard?.recentMovements.length === 0 ? (
                        <div style={{ padding: "24px", textAlign: "center", color: "#94a3b8" }}>No transactions yet</div>
                    ) : (
                        <table style={{ width: "100%", borderCollapse: "collapse" }}>
                            <thead>
                                <tr style={{ borderBottom: "1px solid rgba(100, 116, 139, 0.3)" }}>
                                    <th style={{ padding: "12px 24px", textAlign: "left", color: "#94a3b8", fontWeight: "500" }}>Date</th>
                                    <th style={{ padding: "12px 24px", textAlign: "left", color: "#94a3b8", fontWeight: "500" }}>Description</th>
                                    <th style={{ padding: "12px 24px", textAlign: "right", color: "#94a3b8", fontWeight: "500" }}>Amount</th>
                                </tr>
                            </thead>
                            <tbody>
                                {dashboard?.recentMovements.map((movement: BalanceMovement) => (
                                    <tr key={movement.id} style={{ borderBottom: "1px solid rgba(100, 116, 139, 0.2)" }}>
                                        <td style={{ padding: "16px 24px", color: "#94a3b8" }}>{formatDate(movement.timestamp)}</td>
                                        <td style={{ padding: "16px 24px" }}>{movement.description}</td>
                                        <td style={{
                                            padding: "16px 24px",
                                            textAlign: "right",
                                            fontFamily: "monospace",
                                            fontWeight: "600",
                                            color: movement.amount >= 0 ? "#4ade80" : "#f87171"
                                        }}>
                                            {movement.amount >= 0 ? "+" : ""}{formatCurrency(movement.amount)}
                                        </td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    )}
                </div>
            </div>
        </div>
    );
};

export default DashboardPage;
