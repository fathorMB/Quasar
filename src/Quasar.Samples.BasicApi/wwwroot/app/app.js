const state = {
    accessToken: null,
    refreshToken: null,
    subjectId: null,
    hubConnection: null,
    serverLogCursor: 0,
    serverLogErrorNotified: false,
    checkoutId: null
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

const checkoutIdInputs = () => Array.from(document.querySelectorAll('.checkout-id-input'));

const updateCheckoutHint = (id, status) => {
    const el = document.getElementById('checkoutSelected');
    if (!el) return;
    if (id) {
        const statusSuffix = status ? ` (${status})` : '';
        el.textContent = `Active checkout: ${id}${statusSuffix}`;
        el.classList.add('ok');
    } else {
        el.textContent = 'No checkout selected';
        el.classList.remove('ok');
    }
};

const setCheckoutSelection = (id, status) => {
    const normalized = id && id.trim ? id.trim() : null;
    state.checkoutId = normalized;
    checkoutIdInputs().forEach(input => {
        if (!input) return;
        input.value = normalized ?? '';
    });
    updateCheckoutHint(normalized, status);
};

const resolveCheckoutId = provided => {
    const trimmed = provided && provided.trim ? provided.trim() : '';
    const id = trimmed || state.checkoutId;
    if (!id) throw new Error('Checkout id required. Start a checkout first.');
    return id;
};

const updateCheckoutOutput = data => {
    const output = document.getElementById('checkoutOutput');
    if (!output) return;
    output.textContent = data ? JSON.stringify(data, null, 2) : '';
};

const fetchCheckoutStatus = async checkoutId => {
    const result = await apiFetch(`/checkout/${checkoutId}`);
    updateCheckoutOutput(result);
    const status = result?.status ?? result?.Status ?? null;
    setCheckoutSelection(checkoutId, status);
    log('ui:checkout status fetched', result);
    return result;
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

handleForm('checkoutStartForm', async payload => {
    const totalAmount = Number(payload.totalAmount);
    if (!Number.isFinite(totalAmount) || totalAmount <= 0) {
        throw new Error('Total amount must be greater than zero.');
    }

    const body = { totalAmount };
    if (payload.checkoutId && payload.checkoutId.trim()) body.checkoutId = payload.checkoutId.trim();
    if (payload.cartId && payload.cartId.trim()) body.cartId = payload.cartId.trim();

    const result = await apiFetch('/checkout/start', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(body)
    });

    setCheckoutSelection(result.checkoutId);
    log('ui:checkout started', result);

    try {
        await fetchCheckoutStatus(result.checkoutId);
    } catch (err) {
        log(`ui:checkout status fetch error: ${err.message}`);
        updateCheckoutOutput(result);
    }
});

handleForm('checkoutPaymentForm', async payload => {
    const checkoutId = resolveCheckoutId(payload.checkoutId);
    const paymentReference = payload.paymentReference ? payload.paymentReference.trim() : '';
    if (!paymentReference) {
        throw new Error('Payment reference is required.');
    }

    const result = await apiFetch(`/checkout/${checkoutId}/payment`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ paymentReference })
    });

    setCheckoutSelection(checkoutId);
    log('ui:checkout payment confirmed', result);
    await fetchCheckoutStatus(checkoutId);
});

handleForm('checkoutFulfillmentForm', async payload => {
    const checkoutId = resolveCheckoutId(payload.checkoutId);
    const trackingNumber = payload.trackingNumber && payload.trackingNumber.trim()
        ? payload.trackingNumber.trim()
        : null;

    const result = await apiFetch(`/checkout/${checkoutId}/fulfillment`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ trackingNumber })
    });

    setCheckoutSelection(checkoutId);
    log('ui:checkout fulfillment requested', result);
    await fetchCheckoutStatus(checkoutId);
});

handleForm('checkoutStatusForm', async payload => {
    const checkoutId = resolveCheckoutId(payload.checkoutId);
    setCheckoutSelection(checkoutId);
    await fetchCheckoutStatus(checkoutId);
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
setCheckoutSelection(null);
