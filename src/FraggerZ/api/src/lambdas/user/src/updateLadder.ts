import { APIGatewayProxyEvent, APIGatewayProxyHandler, APIGatewayProxyResult } from 'aws-lambda';
import * as DynamoDB from 'aws-sdk/clients/dynamodb';
import { createDbInst } from '/opt/aws/dynamodb';
import { responseOk, responseBadRequest, responseServerError } from '/opt/utils/responses';
import { lambdaHandler } from '/opt/utils/lambdaHandler';
import { createLogger } from '/opt/utils/logger';
import { BasicAuth } from '/opt/utils/authHeader';
import * as user from './user';

const logger = createLogger(`UpdateUserLadder-${process.env.NODE_ENV}`);

interface UpdateUserLadderRequest {
  GameCode: string;
  LadderName: string;
  SeasonId: number;
  MMR: number;
  Sigma: number;
  Wins: number;
  Losses: number;
}

export async function updateLadder(event: APIGatewayProxyEvent, basicAuth: BasicAuth): Promise<any> {
  const userId = event.queryStringParameters?.id;
  const serverId: string = basicAuth.username;
  if(!userId) {
      return responseBadRequest({ message: `Can't find user ID` });
  }
  try {
    const req: UpdateUserLadderRequest = JSON.parse(event.body ?? '');
    const ddb: DynamoDB.DocumentClient = createDbInst();
    const existingItemQuery = await user.getOneUser(ddb, { ServerId: serverId, UserId: userId });
    if(!existingItemQuery.Item) {
      return responseBadRequest({ message: `User doesn't exist.` });
    }
    let attributesToChange = Object.fromEntries(Object.entries(req).filter(([_, v]) => v !== undefined));
    await user.update(ddb, attributesToChange, {
      PK: `#USER#${serverId}#${userId}`,
      SK: `LADDER#${req.GameCode}#${req.LadderName}#${req.SeasonId}`
    });
    return responseOk({ message: `User's ladder successfully updated` });
  }
  catch (error) {
    return responseServerError({ message: `Couldn\'t update the ladder for user: ${userId}\n` + JSON.stringify(error) });
  }
}

export const handler: APIGatewayProxyHandler = lambdaHandler(
  async (event: APIGatewayProxyEvent, basicAuth: BasicAuth): Promise<any> => {
    logger.info(JSON.stringify(event));
    return updateLadder(event, basicAuth);
  }
)