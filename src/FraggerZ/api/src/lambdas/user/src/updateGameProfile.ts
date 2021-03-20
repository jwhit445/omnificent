import { APIGatewayProxyEvent, APIGatewayProxyHandler, APIGatewayProxyResult } from 'aws-lambda';
import * as DynamoDB from 'aws-sdk/clients/dynamodb';
import { createDbInst } from '/opt/aws/dynamodb';
import { responseOk, responseBadRequest, responseServerError } from '/opt/utils/responses';
import { lambdaHandler } from '/opt/utils/lambdaHandler';
import { createLogger } from '/opt/utils/logger';
import { BasicAuth } from '/opt/utils/authHeader';
import * as user from './user';

const logger = createLogger(`UpdateUserGameProfile-${process.env.NODE_ENV}`);

interface UpdateUserGameProfileRequest {
  GameCode: string;
  IGN: string;
  DateTimeLastStatReset: Date;
}

export async function updateGameProfile(event: APIGatewayProxyEvent, basicAuth: BasicAuth): Promise<any> {
  const userId = event.queryStringParameters?.id;
  const serverId: string = basicAuth.username;
  if(!userId) {
      return responseBadRequest({ message: `Can't find user ID` });
  }
  try {
    const req: UpdateUserGameProfileRequest = JSON.parse(event.body ?? '');
    const ddb: DynamoDB.DocumentClient = createDbInst();
    const existingItemQuery = await user.getOneUser(ddb, { ServerId: serverId, UserId: userId });
    if(!existingItemQuery.Item) {
      return responseBadRequest({ message: `User doesn't exist.` });
    }
    let attributesToChange = Object.fromEntries(Object.entries(req).filter(([_, v]) => v !== undefined));
    await user.update(ddb, attributesToChange, {
      PK: `#USER#${serverId}#${userId}`,
      SK: `GAMEPROFILE#${req.GameCode}`
    });
    return responseOk({ message: `User's game profile successfully updated` });
  }
  catch (error) {
    return responseServerError({ message: `Couldn\'t update the gameprofile for user: ${userId}\n` + JSON.stringify(error) });
  }
}

export const handler: APIGatewayProxyHandler = lambdaHandler(
  async (event: APIGatewayProxyEvent, basicAuth: BasicAuth): Promise<any> => {
    logger.info(JSON.stringify(event));
    return updateGameProfile(event, basicAuth);
  }
)