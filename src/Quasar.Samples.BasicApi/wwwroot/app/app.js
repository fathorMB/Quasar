const state = {
    accessToken: null,
    refreshToken: null,
    subjectId: null,
    hubConnection: null,
    serverLogCursor: 0,
    serverLogErrorNotified: false
};

const log = (message, data) => {
    const logEl = document.getElementById('log');
    const time = new Date().toISOString();
    const payload = data !== undefined ? `\n${JSON.stringify(data, null, 2)}` : '';
    logEl.textContent = `[${time}] ${message}${payload}\n` + logEl.textContent;
};

const setStatus = () => {
    const authEl = document.getElementById('authStatus');
    if (state.accessToken) {
        authEl.textContent = `Authenticated as ${state.subjectId}`;
        authEl.classList.add('ok');
    } else {
        authEl.textContent = 'Not authenticated';
        authEl.classList.remove('ok');
    }
};

const parseJwt = token => {
    const [, payload] = token.split('.');
    const json = atob(payload.replace(/-/g, '+').replace(/_/g, '/'));
    return JSON.parse(json);
};

const buildHeaders = (extra = {}) => {
    const headers = { 'Accept': 'application/json', ...extra };
    if (state.accessToken) headers['Authorization'] = `Bearer ${state.accessToken}`;
    if (state.subjectId) headers['X-Subject'] = state.subjectId;
    return headers;
};

const apiFetch = async (path, options = {}) => {
    const finalOptions = {
        method: 'GET',
        ...options,
        headers: buildHeaders(options.headers)
    };
    const res = await fetch(path, finalOptions);
    const text = await res.text();
    let data = null;
    if (text) {
        try { data = JSON.parse(text); }
        catch { data = text; }
    }
    if (!res.ok) {
        const err = typeof data === 'string' ? data : JSON.stringify(data);
        throw new Error(err || res.statusText);
    }
    return data;
};

const handleForm = (formId, handler) => {
    const form = document.getElementById(formId);
    form.addEventListener('submit', async evt => {
        evt.preventDefault();
        const formData = new FormData(form);
        const payload = Object.fromEntries(formData.entries());
        try {
            await handler(payload, form);
        } catch (err) {
            log(`ui:${formId} error: ${err.message}`);
        }
    });
};

document.querySelector('#loginForm input[name="username"]').value = 'swagger-demo';
document.querySelector('#loginForm input[name="password"]').value = 'Passw0rd!';

handleForm('registerForm', async payload => {
    const result = await apiFetch('/auth/register', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
    });
    log('ui:registered user', result);
});

handleForm('loginForm', async payload => {
    const result = await apiFetch('/auth/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
    });
    state.accessToken = result.accessToken;
    state.refreshToken = result.refreshToken;
    const token = parseJwt(result.accessToken);
    state.subjectId = token.sub;
    setStatus();
    log('ui:logged in', result);
});

handleForm('counterForm', async payload => {
    const amount = Number(payload.amount || 1);
    const result = await apiFetch(`/counter/increment?amount=${amount}`, { method: 'POST' });
    document.getElementById('counterOutput').textContent = JSON.stringify(result, null, 2);
    log('ui:counter incremented', result);
});

document.getElementById('refreshCounter').addEventListener('click', async () => {
    try {
        const result = await apiFetch('/counter');
        document.getElementById('counterOutput').textContent = JSON.stringify(result, null, 2);
        log('ui:counter fetched', result);
    } catch (err) {
        log(`ui:counter fetch error: ${err.message}`);
    }
});

handleForm('cartForm', async payload => {
    const params = new URLSearchParams({
        productId: payload.productId,
        quantity: payload.quantity
    });
    const result = await apiFetch(`/cart/add?${params.toString()}`, { method: 'POST' });
    document.getElementById('cartOutput').textContent = JSON.stringify(result, null, 2);
    log('ui:item added to cart', result);
});

document.getElementById('refreshCart').addEventListener('click', async () => {
    try {
        const result = await apiFetch('/cart');
        document.getElementById('cartOutput').textContent = JSON.stringify(result, null, 2);
        log('ui:cart fetched', result);
    } catch (err) {
        log(`ui:cart fetch error: ${err.message}`);
    }
});

handleForm('sensorIngestForm', async payload => {
    const body = {
        deviceId: payload.deviceId,
        sensorType: payload.sensorType,
        value: Number(payload.value),
        timestampUtc: payload.timestampUtc ? new Date(payload.timestampUtc).toISOString() : undefined
    };
    const result = await apiFetch('/sensors/ingest', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(body)
    });
    log('ui:sensor reading ingested', result);
});

handleForm('sensorQueryForm', async payload => {
    const params = new URLSearchParams({ deviceId: payload.deviceId });
    if (payload.fromUtc) params.append('fromUtc', new Date(payload.fromUtc).toISOString());
    if (payload.toUtc) params.append('toUtc', new Date(payload.toUtc).toISOString());
    const result = await apiFetch(`/sensors/${payload.deviceId}/readings?${params.toString()}`);
    document.getElementById('sensorQueryOutput').textContent = JSON.stringify(result, null, 2);
    log('ui:sensor history fetched', result);
});

const hubStatus = document.getElementById('hubStatus');
const liveOutput = document.getElementById('sensorLiveOutput');

const updateHubStatus = connected => {
    hubStatus.textContent = connected ? 'Connected' : 'Disconnected';
    hubStatus.classList.toggle('ok', connected);
};

const connectHub = async () => {
    if (state.hubConnection) return;
    const builder = new signalR.HubConnectionBuilder().withUrl('/hubs/sensors');
    state.hubConnection = builder.build();
    state.hubConnection.on('ReceiveSensorReading', payload => {
        liveOutput.textContent = `${JSON.stringify(payload)}\n` + liveOutput.textContent;
    });
    try {
        await state.hubConnection.start();
        updateHubStatus(true);
        log('ui:signalR connected');
    } catch (err) {
        state.hubConnection = null;
        updateHubStatus(false);
        log(`ui:signalR connect error: ${err.message}`);
    }
};

const disconnectHub = async () => {
    if (!state.hubConnection) return;
    await state.hubConnection.stop();
    state.hubConnection = null;
    updateHubStatus(false);
    log('ui:signalR disconnected');
};

const pollServerLogs = async () => {
    try {
        const result = await apiFetch(`/debug/logs/recent?since=${state.serverLogCursor}`);
        if (Array.isArray(result) && result.length) {
            result.forEach(entry => {
                log(`server:${entry.level} ${entry.message}`, entry);
            });
            state.serverLogCursor = result[result.length - 1].sequence;
        }
        state.serverLogErrorNotified = false;
    } catch (err) {
        if (!state.serverLogErrorNotified) {
            log(`server log stream error: ${err.message}`);
            state.serverLogErrorNotified = true;
        }
    }
};

document.getElementById('connectHub').addEventListener('click', connectHub);
document.getElementById('disconnectHub').addEventListener('click', disconnectHub);

document.getElementById('clearLogs').addEventListener('click', () => {
    document.getElementById('log').textContent = '';
});

setInterval(pollServerLogs, 4000);
pollServerLogs();
setStatus();
