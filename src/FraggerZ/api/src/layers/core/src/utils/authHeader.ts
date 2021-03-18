import { decode } from './base64Helper';

export interface BasicAuth {
    username: string;
    password: string;
}

export function getBasicAuth(authHeader: string): BasicAuth {
    if(!authHeader.startsWith('Basic')) {
        throw new Error(`Provided header is not in Basic format`);
    }
    const creds = decode(authHeader.split(' ')[1]);
    if(creds.indexOf(':') === -1) {
        throw new Error(`Provided header is in an invalid format`);
    }
    const parts = creds.split(':');
    if(parts.length > 2) {
        throw new Error(`Provided header is in an invalid format`);
    }
    return {
        username: parts[0],
        password: parts[1],
    };
}