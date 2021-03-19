import { APIGatewayProxyEvent, APIGatewayProxyHandler, APIGatewayProxyResult } from 'aws-lambda';
import { flushMetrics } from '../aws/metrics';
import { initXray } from '../aws/xray';
import { BasicAuth } from './authHeader';
import { createLogger } from './logger';
import { responseServerError, responseBadRequest } from './responses';
import { getBasicAuth } from './authHeader';

const logger = createLogger(`lambdaHandler-${process.env.NODE_ENV}`);

export function lambdaHandler(handlerFunc: (event: APIGatewayProxyEvent, basicAuth: BasicAuth) => Promise<any>): APIGatewayProxyHandler {
    const retVal: APIGatewayProxyHandler = async (
        event: APIGatewayProxyEvent,
        // eslint-disable-next-line @typescript-eslint/no-unused-vars
        context: any
    ): Promise<APIGatewayProxyResult> => {
        try {
            initXray();
            let basicAuth: BasicAuth;
            try {
              basicAuth = getBasicAuth(event.headers.Authorization ?? '');
            }
            catch (error) {
              return responseBadRequest({ message: `Couldn't authenticate the calling identity` });
            }
            return await handlerFunc(event, basicAuth);
        } catch (err) {
            logger.error(`UNHANDLED EXCEPTION during request. Error: ${err.message}`,{
                stack: err.stack,
                event,
            });
            return responseServerError({message: 'A server error has occurred'});
        } finally {
            await flushMetrics();
        }
    }
    return retVal;
}