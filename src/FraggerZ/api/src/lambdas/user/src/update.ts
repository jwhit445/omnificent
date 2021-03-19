import { APIGatewayProxyEvent, APIGatewayProxyHandler, APIGatewayProxyResult } from 'aws-lambda';
import { createDbInst, DynamoDB } from '/opt/aws/dynamodb';
import { responseOk, responseBadRequest, responseServerError } from '/opt/utils/responses';
import { lambdaHandler } from '/opt/utils/lambdaHandler';
import { createLogger } from '/opt/utils/logger';
import { BasicAuth } from '/opt/utils/authHeader';
import * as user from './user';

const logger = createLogger(`UpdateUser-${process.env.NODE_ENV}`);

interface UpdateUserRequest {
  Username?: string;
  StreamURL?: string;
  DateTimeSuspensionEnd?: Date;
  DateTimePremiumExpire?: Date;
}

export async function update(event: APIGatewayProxyEvent, basicAuth: BasicAuth): Promise<any> {
  if(!process.env.DYNAMODB_TABLE) {
    return responseServerError({ message: 'Internal server error. Missing required configuration.' });
  }
  const userId = event.queryStringParameters?.id;
  const serverId: string = basicAuth.username;
  if(!userId) {
      return responseBadRequest({ message: `Can't find user ID` });
  }
  try {
    const req: UpdateUserRequest = JSON.parse(event.body ?? '');
    const ddb: DynamoDB.DocumentClient = createDbInst();
    const existingItemQuery = await user.getOne(ddb, { ServerId: serverId, UserId: userId });
    if(!existingItemQuery.Item) {
      return responseBadRequest({ message: `User doesn't exist.` });
    }
    let params: DynamoDB.DocumentClient.UpdateItemInput = {
      TableName: process.env.DYNAMODB_TABLE,
      Key: {
        PK: `#USER#${serverId}#${userId}`,
        SK: `#USER#${serverId}#${userId}`
      }
    };
    params.UpdateExpression = '';
    params.ExpressionAttributeValues = {};
    let k: keyof UpdateUserRequest;
    for (k in req) {
      if (!k) {
        continue;
      }
      params.UpdateExpression += `${(params.UpdateExpression.length === 0) ? 'set' : ','} ${k} = :${k}`
      params.ExpressionAttributeValues[`:${k}`] = req[k];
    }
    await ddb.update(params).promise();
    return responseOk({ message: `User successfully updated` });
  }
  catch (error) {
    return responseServerError({ message: `Couldn\'t update the user: ${userId}\n` + JSON.stringify(error) });
  }
}

export const handler: APIGatewayProxyHandler = lambdaHandler(
  async (event: APIGatewayProxyEvent, basicAuth: BasicAuth): Promise<any> => {
    logger.info(JSON.stringify(event));
    return update(event, basicAuth);
  }
)