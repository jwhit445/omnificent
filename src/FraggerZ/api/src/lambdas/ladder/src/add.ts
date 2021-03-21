import { APIGatewayProxyEvent, APIGatewayProxyHandler, APIGatewayProxyResult } from 'aws-lambda';
import { createDbInst, DynamoDB, dbUpdate } from '/opt/aws/dynamodb';
import { responseOk, responseBadRequest, responseServerError } from '/opt/utils/responses';
import { lambdaHandler } from '/opt/utils/lambdaHandler';
import { createLogger } from '/opt/utils/logger';
import { BasicAuth } from '/opt/utils/authHeader';
import * as ladder from './ladder';

const logger = createLogger(`AddLadder-${process.env.NODE_ENV}`);

interface AddLadderRequest {
  GameCode: string;
  LadderName: string;
  CurrentSeasonId: string;
}

export async function add(event: APIGatewayProxyEvent, basicAuth: BasicAuth): Promise<any> {
  const req: AddLadderRequest = JSON.parse(event.body ?? '');
  let serverId: string = basicAuth.username;
  try {
    if(!req.GameCode || !req.LadderName || !req.CurrentSeasonId) {
      return responseBadRequest({ message: 'Missing required request data.' });
    }
    const ddb: DynamoDB.DocumentClient = createDbInst();
    const existingItemQuery = await ladder.getOneLadder(ddb, { ServerId: serverId, ...req });
    if(existingItemQuery.Item) {
      return responseOk({ message: `Ladder successfully added` });
    }
    await dbUpdate(ddb, { ServerId: serverId, ...req }, {
      PK: `#LADDER#${serverId}#${req.GameCode}#${req.LadderName}`,
      SK: `#LADDER#${serverId}#${req.GameCode}#${req.LadderName}`
    });
    return responseOk({ message: `Ladder successfully added` });
  }
  catch (error) {
    return responseServerError({ message: 'Couldn\'t add the ladder' + JSON.stringify(error) });
  }
}


export const handler: APIGatewayProxyHandler = lambdaHandler(
  async (event: APIGatewayProxyEvent, basicAuth: BasicAuth): Promise<any> => {
    logger.info(JSON.stringify(event));
    return add(event, basicAuth);
  }
)
