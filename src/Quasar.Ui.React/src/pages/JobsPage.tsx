import React, { useEffect, useState } from 'react';
import { quartzApi, type QuartzJob } from '../api/quartz';
import { useFeatures } from '../context/FeatureContext';
import './JobsPage.css';

export const JobsPage: React.FC = () => {
    const [jobs, setJobs] = useState<QuartzJob[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [actioning, setActioning] = useState<string | null>(null);
    const [error, setError] = useState('');
    const [message, setMessage] = useState('');
    const [notificationVisible, setNotificationVisible] = useState(false);
    const { hasFeature } = useFeatures();

    const fetchJobs = async () => {
        try {
            setError('');
            setMessage('');
            const data = await quartzApi.listJobs();
            setJobs(data);
        } catch (error) {
            console.error('Failed to fetch jobs:', error);
            setError('Failed to load jobs');
        } finally {
            setIsLoading(false);
        }
    };

    useEffect(() => {
        if (hasFeature('scheduler')) {
            fetchJobs();
        } else {
            setIsLoading(false);
        }
    }, [hasFeature]);

    const handleTrigger = async (group: string, name: string) => {
        try {
            setError('');
            setMessage('');
            setActioning(`${group}:${name}:trigger`);
            await quartzApi.triggerJob(group, name);
            await fetchJobs();
            setMessage('Job triggered successfully');
            setNotificationVisible(true);
        } catch (error) {
            console.error('Failed to trigger job:', error);
            setError('Failed to trigger job');
        } finally {
            setActioning(null);
        }
    };

    const handlePause = async (group: string, name: string) => {
        try {
            setError('');
            setMessage('');
            setActioning(`${group}:${name}:pause`);
            await quartzApi.pauseJob(group, name);
            await fetchJobs(); // Refresh state
            setMessage('Job paused successfully');
            setNotificationVisible(true);
        } catch (error) {
            console.error('Failed to pause job:', error);
            setError('Failed to pause job');
        } finally {
            setActioning(null);
        }
    };

    const handleResume = async (group: string, name: string) => {
        try {
            setError('');
            setMessage('');
            setActioning(`${group}:${name}:resume`);
            await quartzApi.resumeJob(group, name);
            await fetchJobs(); // Refresh state
            setMessage('Job resumed successfully');
            setNotificationVisible(true);
        } catch (error) {
            console.error('Failed to resume job:', error);
            setError('Failed to resume job');
        } finally {
            setActioning(null);
        }
    };

    useEffect(() => {
        if (!notificationVisible || !message) return;
        const timer = setTimeout(() => {
            setNotificationVisible(false);
        }, 10000);
        return () => clearTimeout(timer);
    }, [notificationVisible, message]);

    if (!hasFeature('scheduler')) {
        return (
            <div className="container">
                <h1>Jobs Management</h1>
                <div className="alert alert-warning">
                    The Scheduler feature is not enabled on the server.
                </div>
            </div>
        );
    }

    if (isLoading) {
        return <div className="container">Loading jobs...</div>;
    }

    return (
        <div className="page-container jobs-layout">
            <div className="page-header">
                <div>
                    <h1>Jobs</h1>
                    <p className="text-muted">Manage scheduled jobs and triggers</p>
                </div>
                <button className="btn btn-primary" onClick={fetchJobs} disabled={isLoading}>
                    {isLoading ? 'Refreshing...' : 'Refresh'}
                </button>
            </div>

            <div className="card jobs-card">
                <table className="table">
                    <thead>
                        <tr>
                            <th>Group</th>
                            <th>Name</th>
                            <th>Triggers</th>
                            <th>Next Run</th>
                            <th>Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        {jobs.map((job) => (
                            <tr key={`${job.job.group}-${job.job.name}`}>
                                <td>{job.job.group}</td>
                                <td>{job.job.name}</td>
                                <td>
                                    {job.triggers.map(t => (
                                        <div key={`${t.trigger.group}-${t.trigger.name}`}>
                                            {t.trigger.name}
                                        </div>
                                    ))}
                                </td>
                                <td>
                                    {job.triggers.map(t => (
                                        <div key={`${t.trigger.group}-${t.trigger.name}`}>
                                            {t.nextFireTimeUtc ? new Date(t.nextFireTimeUtc).toLocaleString() : '-'}
                                        </div>
                                    ))}
                                </td>
                                <td>
                                    <div className="jobs-actions">
                                        <button
                                            className="btn btn-sm btn-primary"
                                            onClick={() => handleTrigger(job.job.group, job.job.name)}
                                            disabled={actioning === `${job.job.group}:${job.job.name}:trigger`}
                                        >
                                            {actioning === `${job.job.group}:${job.job.name}:trigger` ? 'Triggering...' : 'Trigger'}
                                        </button>
                                        <button
                                            className="btn btn-sm btn-secondary"
                                            onClick={() => handlePause(job.job.group, job.job.name)}
                                            disabled={actioning === `${job.job.group}:${job.job.name}:pause`}
                                        >
                                            {actioning === `${job.job.group}:${job.job.name}:pause` ? 'Pausing...' : 'Pause'}
                                        </button>
                                        <button
                                            className="btn btn-sm btn-secondary"
                                            onClick={() => handleResume(job.job.group, job.job.name)}
                                            disabled={actioning === `${job.job.group}:${job.job.name}:resume`}
                                        >
                                            {actioning === `${job.job.group}:${job.job.name}:resume` ? 'Resuming...' : 'Resume'}
                                        </button>
                                    </div>
                                </td>
                            </tr>
                        ))}
                        {jobs.length === 0 && (
                            <tr>
                                <td colSpan={5} className="text-center">No jobs found.</td>
                            </tr>
                        )}
                    </tbody>
                </table>
            </div>

            {(error || (message && notificationVisible)) && (
                <div className="floating-alert-wrapper">
                    {error && (
                        <div className="alert alert-error floating-alert">
                            <span>{error}</span>
                            <button className="alert-close" onClick={() => setError('')}>×</button>
                        </div>
                    )}
                    {!error && message && notificationVisible && (
                        <div className="alert alert-success floating-alert">
                            <span>{message}</span>
                            <button className="alert-close" onClick={() => setNotificationVisible(false)}>×</button>
                        </div>
                    )}
                </div>
            )}
        </div>
    );
};
