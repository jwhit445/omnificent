import { APIGatewayProxyEvent, APIGatewayProxyHandler, APIGatewayProxyResult } from 'aws-lambda';
import * as DynamoDB from 'aws-sdk/clients/dynamodb';
import { createDbInst } from '/opt/aws/dynamodb';
import { responseOk, responseBadRequest, responseServerError } from '/opt/utils/responses';
import { lambdaHandler } from '/opt/utils/lambdaHandler';
import { createLogger } from '/opt/utils/logger';
import * as user from './user';
import { BasicAuth } from '/opt/utils/authHeader';

const logger = createLogger(`GetUserGameProfile-${process.env.NODE_ENV}`);

export async function getGameProfile(event: APIGatewayProxyEvent, basicAuth: BasicAuth): Promise<any> {
  const userId = event.queryStringParameters?.id;
  const gameCode = event.queryStringParameters?.gameCode;
  const serverId: string = basicAuth.username;
  if(!userId || !gameCode) {
      return responseBadRequest({ message: `Can't find user ID or gameCode` });
  }
  try {
    const ddb: DynamoDB.DocumentClient = createDbInst();
    const results = await user.get(ddb, {
      PK: `#USER#${serverId}#${userId}`,
      SK: `GAMEPROFILE#${gameCode}`
    });
    if(!results.Item) {
      return responseBadRequest({ message: `Couldn't find user: ${userId}`});
    }
    return responseOk({
      gameProfile: results.Item
    })
  }
  catch (error) {
    return responseServerError({ message: `Couldn\'t get the gameprofile for user: ${userId}\n` + JSON.stringify(error) });
  }
}

export const handler: APIGatewayProxyHandler = lambdaHandler(
  async (event: APIGatewayProxyEvent, basicAuth: BasicAuth): Promise<any> => {
    logger.info(JSON.stringify(event));
    return getGameProfile(event, basicAuth);
  }
)