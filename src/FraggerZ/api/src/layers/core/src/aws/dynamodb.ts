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

export async function dbUpdate(ddb: DynamoDB.DocumentClient, attributes: any, key: DynamoDB.DocumentClient.Key): Promise<DynamoDB.UpdateItemOutput> {
  if(!process.env.DYNAMODB_TABLE) {
    throw new Error('Internal server error. Missing required configuration');
  }
  let params: DynamoDB.DocumentClient.UpdateItemInput = {
    TableName: process.env.DYNAMODB_TABLE,
    Key: key
  };
  params.UpdateExpression = '';
  params.ExpressionAttributeValues = {};
  for (let k in attributes) {
    if (k === undefined) {
      continue;
    }
    params.UpdateExpression += `${(params.UpdateExpression.length === 0) ? 'set' : ','} ${k} = :${k}`
    params.ExpressionAttributeValues[`:${k}`] = attributes[k];
  }
  return await ddb.update(params).promise();
}

export async function dbGet(ddb: DynamoDB.DocumentClient, key: DynamoDB.DocumentClient.Key): Promise<DynamoDB.GetItemOutput> {
  if(!process.env.DYNAMODB_TABLE) {
    throw new Error('Internal server error. Missing required configuration');
  }
  let params: DynamoDB.DocumentClient.GetItemInput = {
    TableName: process.env.DYNAMODB_TABLE,
    Key: key
  };
  return await ddb.get(params).promise();
}

export { DynamoDB }
