import { APIGatewayProxyEvent, APIGatewayProxyHandler, APIGatewayProxyResult } from 'aws-lambda';
import * as DynamoDB from 'aws-sdk/clients/dynamodb';
import * as AWSXRay from 'aws-xray-sdk-core';
import { responseOk } from '/opt/utils/responses';
import { lambdaHandler } from '/opt/utils/lambdaHandler';
import { createLogger } from '/opt/utils/logger';

const logger = createLogger(`AddUser-${process.env.NODE_ENV}`);

const ddbOptions: DynamoDB.Types.ClientConfiguration = {
  apiVersion: '2012-08-10',
};
if (process.env.AWS_SAM_LOCAL) {
  ddbOptions.endpoint = 'http://dynamodb:8000';
}
//Create the underlying DDB service to wrap in X-Ray. We will use the DocumentClient for easy POCO -> Attribute marshalling
const ddbService: DynamoDB = new DynamoDB(ddbOptions);
const ddb: DynamoDB.DocumentClient = new DynamoDB.DocumentClient({ service: ddbService });
AWSXRay.captureAWSClient(ddbService);

interface RegisterUserRequest {
  Id: string;
  Username: string;
}

export async function add(event: APIGatewayProxyEvent): Promise<any> {
  const req: RegisterUserRequest = JSON.parse(event.body ?? '');
  return responseOk({ message: 'Hello World!' });
  // try {
  //   if(!process.env.DYNAMODB_TABLE) {
  //     return {
  //       statusCode: 500,
  //       headers: { 'Content-Type': 'application/json' },
  //       body: JSON.stringify({ message: 'Internal server error. Missing required configuration.' }),
  //     };
  //   }
  //   const params: DynamoDB.DocumentClient.UpdateItemInput = {
  //     TableName: process.env.DYNAMODB_TABLE,
  //     Key: {
  //       PK: `${req.Id}`,
  //       SK: `PROFILE#`
  //     },
  //     UpdateExpression: 'set Username = :un',
  //     ExpressionAttributeValues: {
  //         ':un': `${req.Username}`
  //     }
  //   };
  //   await ddb.update(params).promise();
  //   return {
  //     statusCode: 200,
  //     body: JSON.stringify(req),
  //   };
  // } catch (error) {
  //   return {
  //     statusCode: 501,
  //     headers: { 'Content-Type': 'application/json' },
  //     body: JSON.stringify({ message: 'Couldn\'t register the user' + JSON.stringify(error) }),
  //   };
  // }
}

export const handler: APIGatewayProxyHandler = lambdaHandler(
  async (event: APIGatewayProxyEvent): Promise<any> => {
    logger.info(JSON.stringify(event));
    return add(event);
  }
)
