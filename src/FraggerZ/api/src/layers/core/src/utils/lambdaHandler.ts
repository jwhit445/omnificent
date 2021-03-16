import { APIGatewayProxyEvent, APIGatewayProxyHandler, APIGatewayProxyResult } from 'aws-lambda';
import { flushMetrics } from '../aws/metrics';
import { initXray } from '../aws/xray';
import { createLogger } from './logger';
import { responseServerError } from './responses';

const logger = createLogger(`lambdaHandler-${process.env.NODE_ENV}`);

export function lambdaHandler(handlerFunc: any): APIGatewayProxyHandler {
    const retVal: APIGatewayProxyHandler = async (
        event: APIGatewayProxyEvent,
        // eslint-disable-next-line @typescript-eslint/no-unused-vars
        context: any
    ): Promise<APIGatewayProxyResult> => {
        try {
            initXray();
            return await handlerFunc(event);
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