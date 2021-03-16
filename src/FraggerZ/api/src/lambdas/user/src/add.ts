import { APIGatewayProxyEvent, APIGatewayProxyHandler, APIGatewayProxyResult } from 'aws-lambda';
import * as DynamoDB from 'aws-sdk/clients/dynamodb';
import { createDbInst } from '/opt/aws/dynamodb';
import { responseOk, responseBadRequest, responseServerError } from '/opt/utils/responses';
import { lambdaHandler } from '/opt/utils/lambdaHandler';
import { createLogger } from '/opt/utils/logger';

const logger = createLogger(`AddUser-${process.env.NODE_ENV}`);

interface RegisterUserRequest {
  ServerId: string;
  UserId: string;
  Username: string;
}

export async function add(event: APIGatewayProxyEvent): Promise<any> {
  const req: RegisterUserRequest = JSON.parse(event.body ?? '');
  try {
    if(!process.env.DYNAMODB_TABLE) {
      return responseServerError({ message: 'Internal server error. Missing required configuration.' });
    }
    if(!req.ServerId || !req.UserId || !req.Username) {
      return responseBadRequest({ message: 'Missing required request data.' });
    }
    const params: DynamoDB.DocumentClient.UpdateItemInput = {
      TableName: process.env.DYNAMODB_TABLE,
      Key: {
        PK: `#USER#${req.ServerId}#${req.UserId}`,
        SK: `#USER#${req.ServerId}#${req.UserId}`
      },
      UpdateExpression: 'set ServerId = :serverid, UserId = :userid, Username = :uname',
      ExpressionAttributeValues: {
          ':serverid': `${req.ServerId}`,
          ':userid': `${req.UserId}`,
          ':uname': `${req.Username}`
      }
    };
    const ddb: DynamoDB.DocumentClient = createDbInst();
    await ddb.update(params).promise();
    return responseOk(req);
  }
  catch (error) {
    return responseServerError({ message: 'Couldn\'t register the user' + JSON.stringify(error) });
  }
}

export const handler: APIGatewayProxyHandler = lambdaHandler(
  async (event: APIGatewayProxyEvent): Promise<any> => {
    logger.info(JSON.stringify(event));
    return add(event);
  }
)
