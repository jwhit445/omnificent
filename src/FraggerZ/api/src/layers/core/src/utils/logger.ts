import { Logger, createLogger as createWinstonLogger, transports, format } from 'winston'

const APP_NAME = 'FraggerZ';

export function createLogger(label: string): Logger {
    const levels: string[] = ['verbose', 'info', 'debug', 'warn', 'error'];
    const log_level: string = process.env.LOG_LEVEL || 'debug';
    return createWinstonLogger({
        level: levels.find((l) => l === log_level) ?? 'debug',
        format: format.combine(
            format.splat(),
            format.label({ label }),
            format.timestamp({ format: 'YYYY-MM-DD HH:mm:ss' }),
            format.printf((info) => `${JSON.stringify(info)}`)
        ),
        transports: [new transports.Console()],
    });
}
const loggerBase = createLogger(APP_NAME);

export default loggerBase;