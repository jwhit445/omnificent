import { APIGatewayProxyEvent, APIGatewayProxyHandler, APIGatewayProxyResult } from 'aws-lambda';
import * as DynamoDB from 'aws-sdk/clients/dynamodb';
import { createDbInst } from '/opt/aws/dynamodb';
import { responseOk, responseBadRequest, responseServerError } from '/opt/utils/responses';
import { lambdaHandler } from '/opt/utils/lambdaHandler';
import { createLogger } from '/opt/utils/logger';
import { BasicAuth } from '/opt/utils/authHeader';
import * as user from './user';

const logger = createLogger(`GetUserLadder-${process.env.NODE_ENV}`);

export async function getLadder(event: APIGatewayProxyEvent, basicAuth: BasicAuth): Promise<any> {
  const userId = event.queryStringParameters?.id;
  const gameCode = event.queryStringParameters?.gameCode;
  const ladderName = event.queryStringParameters?.ladderName;
  const seasonId = event.queryStringParameters?.seasonId;
  const serverId: string = basicAuth.username;
  if(!userId || !gameCode || !ladderName || !seasonId) {
      return responseBadRequest({ message: `Can't find user ID or required query parameter` });
  }
  try {
    const ddb: DynamoDB.DocumentClient = createDbInst();
    const results = await user.get(ddb, {
      PK: `#USER#${serverId}#${userId}`,
      SK: `LADDER#${gameCode}#${ladderName}#${seasonId}`
    });
    if(!results.Item) {
      return responseBadRequest({ message: `Couldn't find user: ${userId}`});
    }
    return responseOk({
      gameProfile: results.Item
    })
  }
  catch (error) {
    return responseServerError({ message: `Couldn\'t get the ladder for user: ${userId}\n` + JSON.stringify(error) });
  }
}

export const handler: APIGatewayProxyHandler = lambdaHandler(
  async (event: APIGatewayProxyEvent, basicAuth: BasicAuth): Promise<any> => {
    logger.info(JSON.stringify(event));
    return getLadder(event, basicAuth);
  }
)