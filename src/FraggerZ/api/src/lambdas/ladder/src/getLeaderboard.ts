import { APIGatewayProxyEvent, APIGatewayProxyHandler, APIGatewayProxyResult } from 'aws-lambda';
import * as DynamoDB from 'aws-sdk/clients/dynamodb';
import { createDbInst } from '/opt/aws/dynamodb';
import { responseOk, responseBadRequest, responseServerError } from '/opt/utils/responses';
import { lambdaHandler } from '/opt/utils/lambdaHandler';
import { createLogger } from '/opt/utils/logger';

const logger = createLogger(`GetLadderLeaderboard-${process.env.NODE_ENV}`);

export async function getLeaderboard(event: APIGatewayProxyEvent): Promise<any> {
  const ladderId = event.queryStringParameters?.id;
  if(!ladderId) {
      return responseBadRequest({ message: `Can't find ladder ID` });
  }
  try {
  }
  catch (error) {
    return responseServerError({ message: `Couldn\'t get the ladder for user: ${ladderId}\n` + JSON.stringify(error) });
  }
}

export const handler: APIGatewayProxyHandler = lambdaHandler(
  async (event: APIGatewayProxyEvent): Promise<any> => {
    logger.info(JSON.stringify(event));
    return getLeaderboard(event);
  }
)