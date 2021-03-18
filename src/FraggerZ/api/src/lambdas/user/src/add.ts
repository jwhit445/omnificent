import { APIGatewayProxyEvent, APIGatewayProxyHandler, APIGatewayProxyResult } from 'aws-lambda';
import * as DynamoDB from 'aws-sdk/clients/dynamodb';
import { createDbInst } from '/opt/aws/dynamodb';
import { responseOk, responseBadRequest, responseServerError } from '/opt/utils/responses';
import { lambdaHandler } from '/opt/utils/lambdaHandler';
import { createLogger } from '/opt/utils/logger';
import { UserPK } from '/opt/datainterface/models/UserPK';
import { BasicAuth, getBasicAuth } from '/opt/utils/authHeader';

const logger = createLogger(`AddUser-${process.env.NODE_ENV}`);

interface AddUserRequest {
  UserId: string;
  Username: string;
}

export async function add(event: APIGatewayProxyEvent): Promise<any> {
  const req: AddUserRequest = JSON.parse(event.body ?? '');
  let serverId: string;
  try {
    const basicAuth: BasicAuth = getBasicAuth(event.headers.Authorization ?? '');
    serverId = basicAuth.username;
  }
  catch (error) {
    return responseBadRequest({ message: `Couldn't authenticate the calling identity` });
  }
  try {
    if(!process.env.DYNAMODB_TABLE) {
      return responseServerError({ message: 'Internal server error. Missing required configuration.' });
    }
    if(!req.UserId || !req.Username) {
      return responseBadRequest({ message: 'Missing required request data.' });
    }
    const ddb: DynamoDB.DocumentClient = createDbInst();
    const existingItemQuery = await getOne(ddb, req);
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

async function getOne(ddb: DynamoDB.DocumentClient, pk: UserPK): Promise<DynamoDB.GetItemOutput> {
  if(!process.env.DYNAMODB_TABLE) {
    throw new Error('Internal server error. Missing required configuration');
  }
  const params: DynamoDB.DocumentClient.GetItemInput = {
    TableName: process.env.DYNAMODB_TABLE,
    Key: {
      PK: `#USER#${pk.ServerId}#${pk.UserId}`,
      SK: `#USER#${pk.ServerId}#${pk.UserId}`
    }
  };
  return await ddb.get(params).promise();
}

export const handler: APIGatewayProxyHandler = lambdaHandler(
  async (event: APIGatewayProxyEvent): Promise<any> => {
    logger.info(JSON.stringify(event));
    return add(event);
  }
)
