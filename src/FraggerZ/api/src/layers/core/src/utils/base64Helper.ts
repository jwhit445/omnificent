export function decode(encodedStr: string): string {
    return Buffer.from(encodedStr, 'base64').toString('binary');
}

export function encode(str: string): string {
    return Buffer.from(str, 'binary').toString('base64');
}