export interface CompanyDashboard {
    companyId: string;
    name: string;
    balance: number;
    oil: number;
    gas: number;
    uranium: number;
    createdAt: string;
    recentMovements: BalanceMovement[];
}

export interface BalanceMovement {
    id: string;
    amount: number;
    description: string;
    timestamp: string;
}

export interface CreateCompanyRequest {
    name: string;
}

export interface CustomNavSection {
    title?: string;
    items: Array<{
        label: string;
        path: string;
        roles?: string[];
        feature?: string;
    }>;
}

export interface CustomRoute {
    path: string;
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    component: React.ComponentType<any>;
    index?: boolean;
    roles?: string[];
    feature?: string;
}
