import React, { useEffect, useMemo, useState } from 'react';
import { featuresApi, type Feature } from '../api';
import './FeaturesPage.css';

export const FeaturesPage: React.FC = () => {
    const [features, setFeatures] = useState<Feature[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState('');

    useEffect(() => {
        const load = async () => {
            setIsLoading(true);
            setError('');
            try {
                const data = await featuresApi.list();
                setFeatures(data);
            } catch (err: any) {
                setError(err.message || 'Failed to load features');
            } finally {
                setIsLoading(false);
            }
        };

        load();
    }, []);

    const grouped = useMemo(() => {
        const map = new Map<string, Feature[]>();
        features.forEach((f) => {
            const bucket = map.get(f.category) ?? [];
            bucket.push(f);
            map.set(f.category, bucket);
        });
        return Array.from(map.entries()).map(([category, items]) => ({
            category,
            items: items.sort((a, b) => a.name.localeCompare(b.name))
        }));
    }, [features]);

    if (isLoading) {
        return (
            <div className="page-container">
                <div className="flex items-center justify-center" style={{ minHeight: '300px' }}>
                    <div className="spinner" style={{ width: '40px', height: '40px' }}></div>
                </div>
            </div>
        );
    }

    return (
        <div className="page-container">
            <div className="page-header">
                <div>
                    <h1>Features</h1>
                    <p className="text-muted">Quasar modules currently active in this service</p>
                </div>
            </div>

            {error && (
                <div className="alert alert-error">
                    {error}
                </div>
            )}

            {grouped.length === 0 && !error && (
                <div className="card">
                    <div className="empty-state">
                        <p className="text-muted">No features reported.</p>
                    </div>
                </div>
            )}

            <div className="feature-grid">
                {grouped.map(({ category, items }) => (
                    <div key={category} className="card">
                        <div className="card-header">
                            <div>
                                <h3 style={{ margin: 0 }}>{category}</h3>
                                <p className="text-muted" style={{ margin: 0 }}>Active modules in this area</p>
                            </div>
                        </div>
                        <div className="card-body">
                            <ul className="feature-list">
                                {items.map((feature) => (
                                    <li key={feature.id} className="feature-list-item">
                                        <div className="feature-list-item__title">
                                            <span className="badge badge-success" style={{ minWidth: '80px', textAlign: 'center' }}>
                                                {feature.status || 'enabled'}
                                            </span>
                                            <div>
                                                <div className="feature-name">{feature.name}</div>
                                                <div className="text-muted" style={{ fontSize: 'var(--font-size-sm)' }}>
                                                    {feature.description}
                                                </div>
                                                {feature.details && (
                                                    <div className="text-muted" style={{ fontSize: 'var(--font-size-sm)' }}>
                                                        {feature.details}
                                                    </div>
                                                )}
                                            </div>
                                        </div>
                                    </li>
                                ))}
                            </ul>
                        </div>
                    </div>
                ))}
            </div>
        </div>
    );
};
