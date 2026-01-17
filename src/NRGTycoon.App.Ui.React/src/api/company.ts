import type { CompanyDashboard, CreateCompanyRequest } from "../types";

const API_BASE = "";

async function getAuthHeaders(): Promise<HeadersInit> {
    const token = localStorage.getItem("accessToken");
    return {
        "Content-Type": "application/json",
        ...(token ? { Authorization: `Bearer ${token}` } : {}),
    };
}

export async function getCompanyDashboard(): Promise<CompanyDashboard | null> {
    const response = await fetch(`${API_BASE}/api/company/dashboard`, {
        headers: await getAuthHeaders(),
    });

    if (response.status === 404) {
        return null; // No company yet
    }

    if (!response.ok) {
        throw new Error("Failed to fetch dashboard");
    }

    return response.json();
}

export async function createCompany(request: CreateCompanyRequest): Promise<{ companyId: string }> {
    const response = await fetch(`${API_BASE}/api/company`, {
        method: "POST",
        headers: await getAuthHeaders(),
        body: JSON.stringify(request),
    });

    if (!response.ok) {
        const error = await response.json();
        throw new Error(error.error || "Failed to create company");
    }

    return response.json();
}

export async function updateCompanyName(companyId: string, newName: string): Promise<void> {
    const response = await fetch(`${API_BASE}/api/company/name`, {
        method: "PUT",
        headers: await getAuthHeaders(),
        body: JSON.stringify({ companyId, newName }),
    });

    if (!response.ok) {
        const error = await response.json();
        throw new Error(error.error || "Failed to update company name");
    }
}
