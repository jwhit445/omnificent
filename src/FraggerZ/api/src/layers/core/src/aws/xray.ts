import { createLogger } from '../utils/logger';

const logger = createLogger('X-Ray Tracing');
let hasInit = false;

// eslint-disable-next-line import/prefer-default-export
export function initXray() {
    try {
        logger.info('Called initXray');
        if(hasInit) {
            logger.info('Already initialized');
            return;
        }
        hasInit = true;
        const AWSXRay = require('aws-xray-sdk-core');
        if(process.env.AWS_SAM_LOCAL) {
            // SAM Local fails to set the _X_AMZN_TRACE_ID environment variable
            // Removing this line of code results in the following error:
            // 'Error: Missing AWS Lambda trace data for X-Ray. Expected _X_AMZN_TRACE_ID to be set.'
            AWSXRay.setContextMissingStrategy(() => {});
        }

        // eslint-disable-next-line import/no-extraneous-dependencies
        AWSXRay.captureAWS(require('aws-sdk'));
        AWSXRay.captureHTTPsGlobal(require('https'));
        AWSXRay.captureHTTPsGlobal(require('http'));
        
    } catch (err) {
        logger.error(`Error initializing X-Ray Tracing: ${err.message}`);
    }
} 