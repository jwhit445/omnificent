import { APIGatewayProxyEvent, APIGatewayProxyHandler, APIGatewayProxyResult } from 'aws-lambda';
import { createDbInst, DynamoDB } from '/opt/aws/dynamodb';
import { responseOk, responseBadRequest, responseServerError } from '/opt/utils/responses';
import { lambdaHandler } from '/opt/utils/lambdaHandler';
import { createLogger } from '/opt/utils/logger';

const logger = createLogger(`GetUserStats-${process.env.NODE_ENV}`);

export async function getStats(event: APIGatewayProxyEvent): Promise<any> {
  const userId = event.queryStringParameters?.id;
  const ladderId = event.queryStringParameters?.ladderId;
  const season = event.queryStringParameters?.season;
  const gameCode = event.queryStringParameters?.gameCode;
  if(!userId || !ladderId || !season || !gameCode) {
      return responseBadRequest({ message: `Missing a required parameter` });
  }
  try {
  }
  catch (error) {
    return responseServerError({ message: `Couldn\'t get stats for the user: ${userId}\n` + JSON.stringify(error) });
  }
}

export const handler: APIGatewayProxyHandler = lambdaHandler(
  async (event: APIGatewayProxyEvent): Promise<any> => {
    logger.info(JSON.stringify(event));
    return getStats(event);
  }
)