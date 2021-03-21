import { APIGatewayProxyEvent, APIGatewayProxyHandler, APIGatewayProxyResult } from 'aws-lambda';
import { createDbInst, DynamoDB } from '/opt/aws/dynamodb';
import { responseOk, responseBadRequest, responseServerError } from '/opt/utils/responses';
import { lambdaHandler } from '/opt/utils/lambdaHandler';
import { createLogger } from '/opt/utils/logger';
import { BasicAuth } from '/opt/utils/authHeader';
import * as game from './game';

const logger = createLogger(`GameGetOne-${process.env.NODE_ENV}`);

export async function getOne(event: APIGatewayProxyEvent, basicAuth: BasicAuth): Promise<any> {
  const gameCode = event.queryStringParameters?.gameCode;
  const serverId: string = basicAuth.username;
  if(!gameCode) {
      return responseBadRequest({ message: `Can't find game code` });
  }
  try {
    const ddb: DynamoDB.DocumentClient = createDbInst();
    const result = await game.getOneGame(ddb, { ServerId: serverId, GameCode: gameCode });
    if(!result.Item) {
      return responseBadRequest({ message: `Can't find game with game code: ${gameCode}` });
    }
    return responseOk(result.Item);
  }
  catch (error) {
    return responseServerError({ message: `Couldn\'t get the game with game code: ${gameCode}\n` + JSON.stringify(error) });
  }
}

export const handler: APIGatewayProxyHandler = lambdaHandler(
  async (event: APIGatewayProxyEvent, basicAuth: BasicAuth): Promise<any> => {
    logger.info(JSON.stringify(event));
    return getOne(event, basicAuth);
  }
)