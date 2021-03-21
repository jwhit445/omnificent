import { APIGatewayProxyEvent, APIGatewayProxyHandler, APIGatewayProxyResult } from 'aws-lambda';
import { createDbInst, DynamoDB, dbUpdate } from '/opt/aws/dynamodb';
import { responseOk, responseBadRequest, responseServerError } from '/opt/utils/responses';
import { lambdaHandler } from '/opt/utils/lambdaHandler';
import { createLogger } from '/opt/utils/logger';
import { BasicAuth } from '/opt/utils/authHeader';
import * as game from './game';

const logger = createLogger(`GameUpdate-${process.env.NODE_ENV}`);

interface GameUpdateRequest {
    GameCode: string;
    ListGameModes?: string[],
    ListCharacterNames?: string[];
    ListMaps?: string[];
}

export async function update(event: APIGatewayProxyEvent, basicAuth: BasicAuth): Promise<any> {
  const serverId: string = basicAuth.username;
  const req: GameUpdateRequest = JSON.parse(event.body ?? '');
  if(!req.GameCode) {
    return responseBadRequest({ message: `Can't find game code` });
  }
  try {
    const ddb: DynamoDB.DocumentClient = createDbInst();
    const existingItemQuery = await game.getOneGame(ddb, { ServerId: serverId, GameCode: req.GameCode });
    if(!existingItemQuery.Item) {
      return responseBadRequest({ message: `Game doesn't exist.` });
    }
    let attributesToChange = Object.fromEntries(Object.entries(req).filter(([_, v]) => v !== undefined));
    await dbUpdate(ddb, attributesToChange, {
      PK: `#GAME#${serverId}#${req.GameCode}`,
      SK: `#GAME#${serverId}#${req.GameCode}`
    });
    return responseOk({ message: `Game successfully updated` });
  }
  catch (error) {
    return responseServerError({ message: `Couldn\'t update the game with code: ${req.GameCode}\n` + JSON.stringify(error) });
  }
}

export const handler: APIGatewayProxyHandler = lambdaHandler(
  async (event: APIGatewayProxyEvent, basicAuth: BasicAuth): Promise<any> => {
    logger.info(JSON.stringify(event));
    return update(event, basicAuth);
  }
)