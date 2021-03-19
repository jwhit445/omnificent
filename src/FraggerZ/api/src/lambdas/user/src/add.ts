import { APIGatewayProxyEvent, APIGatewayProxyHandler, APIGatewayProxyResult } from 'aws-lambda';
import { createDbInst, DynamoDB } from '/opt/aws/dynamodb';
import { responseOk, responseBadRequest, responseServerError } from '/opt/utils/responses';
import { lambdaHandler } from '/opt/utils/lambdaHandler';
import { createLogger } from '/opt/utils/logger';
import { BasicAuth } from '/opt/utils/authHeader';
import * as user from './user';

const logger = createLogger(`AddUser-${process.env.NODE_ENV}`);

interface AddUserRequest {
  UserId: string;
  Username: string;
}

export async function add(event: APIGatewayProxyEvent, basicAuth: BasicAuth): Promise<any> {
  const req: AddUserRequest = JSON.parse(event.body ?? '');
  let serverId: string = basicAuth.username;
  try {
    if(!process.env.DYNAMODB_TABLE) {
      return responseServerError({ message: 'Internal server error. Missing required configuration.' });
    }
    if(!req.UserId || !req.Username) {
      return responseBadRequest({ message: 'Missing required request data.' });
    }
    const ddb: DynamoDB.DocumentClient = createDbInst();
    const existingItemQuery = await user.getOne(ddb, req);
    if(existingItemQuery.Item) {
      return responseOk({ message: `User successfully added` });
    }
    const params: DynamoDB.DocumentClient.UpdateItemInput = {
      TableName: process.env.DYNAMODB_TABLE,
      Key: {
        PK: `#USER#${serverId}#${req.UserId}`,
        SK: `#USER#${serverId}#${req.UserId}`
      },
      UpdateExpression: 'set ServerId = :serverid, UserId = :userid, Username = :uname',
      ExpressionAttributeValues: {
          ':serverid': `${serverId}`,
          ':userid': `${req.UserId}`,
          ':uname': `${req.Username}`
      }
    };
    await ddb.update(params).promise();
    return responseOk({ message: `User successfully added` });
  }
  catch (error) {
    return responseServerError({ message: 'Couldn\'t register the user' + JSON.stringify(error) });
  }
}


export const handler: APIGatewayProxyHandler = lambdaHandler(
  async (event: APIGatewayProxyEvent, basicAuth: BasicAuth): Promise<any> => {
    logger.info(JSON.stringify(event));
    return add(event, basicAuth);
  }
)
