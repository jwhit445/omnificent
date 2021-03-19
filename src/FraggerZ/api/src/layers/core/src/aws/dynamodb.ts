import * as DynamoDB from 'aws-sdk/clients/dynamodb';
import { createLogger } from '../utils/logger';

const logger = createLogger(`DynamoDB-${process.env.NODE_ENV}`);

// eslint-disable-next-line import/prefer-default-export
export function createDbInst(): DynamoDB.DocumentClient {
    try {
        logger.info('Called createDbInst');
        const ddbOptions: DynamoDB.Types.ClientConfiguration = {
            apiVersion: '2012-08-10',
        };
        if(process.env.AWS_SAM_LOCAL) {
            ddbOptions.endpoint = 'http://dynamodb:8000';
        }
        const ddb: DynamoDB.DocumentClient = new DynamoDB.DocumentClient(ddbOptions);
        return ddb;
    } catch (err) {
        logger.error(`Error creating DynamoDB instance: ${err.message}`);
        throw err;
    }
}

export { DynamoDB }
