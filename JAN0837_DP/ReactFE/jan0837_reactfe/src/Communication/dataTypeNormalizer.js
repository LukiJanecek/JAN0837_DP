// schemaNormalizer.js
const isObj = (v) => v && typeof v === 'object' && !Array.isArray(v);

const toBool = (v, d=false) => {
    if (typeof v === 'boolean') return v;
    if (typeof v === 'number') return v !== 0;
    if (typeof v === 'string') {
        const s = v.trim().toLowerCase();
        if (['true','1','yes','on'].includes(s)) return true;
        if (['false','0','no','off',''].includes(s)) return false;
    }
    return d;
};

const toNum = (v, d=0) => {
    if (v == null || v === '') return d;
    const n = typeof v === 'number' ? v : Number(String(v).replace(',', '.'));
    return Number.isFinite(n) ? n : d;
};

const toInt = (v, d=0) => Math.trunc(toNum(v, d));

const toStr = (v, d='') => {
    if (v == null) return d;
    return typeof v === 'string' ? v : String(v);
};

const toDate = (v, d=null) => {
    if (v == null || v === '') return d;
    if (v instanceof Date && !isNaN(v)) return v;
    const n = typeof v === 'number' ? v : Date.parse(v);
    const dt = new Date(n);
    return isNaN(dt) ? d : dt;
};

const toEnum = (v, values, d) => values.includes(v) ? v : d;

export function normalizeBySchema(input, schema) {
    const out = {};

    for (const key of Object.keys(schema)) {
        if (key === '*') continue; // catch-all řešíme níž
        const rule = schema[key];
        const val  = input?.[key];

        out[key] = coerce(val, rule);
    }

    // catch-all: nevyjmenovaná pole
    if (schema['*']) {
        for (const key of Object.keys(input || {})) {
        if (out[key] !== undefined) continue;
        out[key] = coerce(input[key], schema['*']);
        }
    }

    return out;
}

function coerce(val, rule) {
    const r = typeof rule === 'string' ? { type: rule } : rule || {};

    switch (r.type) {
        case 'boolean': return toBool(val, r.default ?? false);
        case 'number':  return toNum(val,  r.default ?? 0);
        case 'int':     return toInt(val,  r.default ?? 0);
        case 'string':  return toStr(val,  r.default ?? '');
        case 'date':    return toDate(val, r.default ?? null);
        case 'enum':    return toEnum(val, r.values || [], r.default);
        case 'array': {
        const arr = Array.isArray(val) ? val : (r.default ?? []);
        const elemRule = r.element || { type: 'any' };
        return arr.map(item => coerce(item, elemRule));
        }
        case 'object': {
        const shape = r.shape || {};
        const v = isObj(val) ? val : {};
        return normalizeBySchema(v, shape);
        }
        case 'any':
        default:
        return val ?? (r.default ?? null);
    }
}
