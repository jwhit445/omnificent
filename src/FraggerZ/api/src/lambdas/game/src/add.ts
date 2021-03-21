import { APIGatewayProxyEvent, APIGatewayProxyHandler, APIGatewayProxyResult } from 'aws-lambda';
import { createDbInst, DynamoDB, dbUpdate } from '/opt/aws/dynamodb';
import { responseOk, responseBadRequest, responseServerError } from '/opt/utils/responses';
import { lambdaHandler } from '/opt/utils/lambdaHandler';
import { createLogger } from '/opt/utils/logger';
import { BasicAuth } from '/opt/utils/authHeader';
import * as game from './game';

const logger = createLogger(`GameAdd-${process.env.NODE_ENV}`);

interface AddGameRequest {
  GameCode: string;
  ListGameModes?: string[],
  ListCharacterNames?: string[];
  ListMaps?: string[];
}

export async function add(event: APIGatewayProxyEvent, basicAuth: BasicAuth): Promise<any> {
  const req: AddGameRequest = JSON.parse(event.body ?? '');
  let serverId: string = basicAuth.username;
  try {
    if(!req.GameCode) {
      return responseBadRequest({ message: 'Missing required request data.' });
    }
    const ddb: DynamoDB.DocumentClient = createDbInst();
    const existingItemQuery = await game.getOneGame(ddb, { ServerId: serverId, ...req });
    if(existingItemQuery.Item) {
      return responseOk({ message: `Game successfully added` });
    }
    await dbUpdate(ddb, { ServerId: serverId, ...req }, {
      PK: `#GAME#${serverId}#${req.GameCode}`,
      SK: `#GAME#${serverId}#${req.GameCode}`
    });
    return responseOk({ message: `Game successfully added` });
  }
  catch (error) {
    return responseServerError({ message: 'Couldn\'t add the game' + JSON.stringify(error) });
  }
}


export const handler: APIGatewayProxyHandler = lambdaHandler(
  async (event: APIGatewayProxyEvent, basicAuth: BasicAuth): Promise<any> => {
    logger.info(JSON.stringify(event));
    return add(event, basicAuth);
  }
)
