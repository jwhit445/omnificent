import { APIGatewayProxyEvent, APIGatewayProxyHandler, APIGatewayProxyResult } from 'aws-lambda';
import { createDbInst, DynamoDB, dbUpdate } from '/opt/aws/dynamodb';
import { responseOk, responseBadRequest, responseServerError } from '/opt/utils/responses';
import { lambdaHandler } from '/opt/utils/lambdaHandler';
import { createLogger } from '/opt/utils/logger';
import { BasicAuth } from '/opt/utils/authHeader';
import * as ladder from './ladder';

const logger = createLogger(`AddLadder-${process.env.NODE_ENV}`);

interface AddLadderMatchRequest {
  GameCode: string;
  LadderName: string;
}

export async function add(event: APIGatewayProxyEvent, basicAuth: BasicAuth): Promise<any> {
  const req: AddLadderMatchRequest = JSON.parse(event.body ?? '');
  let serverId: string = basicAuth.username;
  try {
    if(!req.GameCode || !req.LadderName) {
      return responseBadRequest({ message: 'Missing required request data.' });
    }
    const ddb: DynamoDB.DocumentClient = createDbInst();
    const existingItemQuery = await ladder.getOneLadder(ddb, { ServerId: serverId, ...req });
    if(!existingItemQuery.Item) {
      return responseBadRequest({ message: `Ladder doesn't exist` });
    }
    if(!existingItemQuery.Item.CurrentSeasonId) {
      return responseBadRequest({ message: `Ladder doesn't have a valid current season` });
    }
    const currentSeasonId = Number(existingItemQuery.Item.CurrentSeasonId);
    const latestMatchRes = await ladder.getLatestMatchNum(ddb, {ServerId: serverId, SeasonId: currentSeasonId, ...req});
    let latestMatchNum: number = 0;
    if(latestMatchRes.Items && latestMatchRes.Items[0].MatchNumber) {
      // If there is any matches for the ladder/season, this will return the latest match
      latestMatchNum = Number(latestMatchRes.Items[0].MatchNumber);
    }
    const matchNum = latestMatchNum + 1;
    const matchItem = {
      ServerId: serverId,
      MatchNumber: matchNum,
      DateTimeStarted: new Date().toISOString(),
      MatchStatus: 'Created',
      SeasonId: currentSeasonId,
      ...req
    }
    await dbUpdate(ddb, matchItem, {
      PK: `#LADDER#${serverId}#${req.GameCode}#${req.LadderName}`,
      SK: `SEASON#${currentSeasonId}#MATCH#${matchNum}`
    });
    return responseOk({ message: `Ladder match successfully added` });
  }
  catch (error) {
    return responseServerError({ message: 'Couldn\'t add the ladder match' + JSON.stringify(error) });
  }
}


export const handler: APIGatewayProxyHandler = lambdaHandler(
  async (event: APIGatewayProxyEvent, basicAuth: BasicAuth): Promise<any> => {
    logger.info(JSON.stringify(event));
    return add(event, basicAuth);
  }
)
