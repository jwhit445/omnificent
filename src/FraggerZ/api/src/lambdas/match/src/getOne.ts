import { APIGatewayProxyEvent, APIGatewayProxyHandler, APIGatewayProxyResult } from 'aws-lambda';
import { createDbInst, DynamoDB } from '/opt/aws/dynamodb';
import { responseOk, responseBadRequest, responseServerError } from '/opt/utils/responses';
import { lambdaHandler } from '/opt/utils/lambdaHandler';
import { createLogger } from '/opt/utils/logger';
import { BasicAuth } from '/opt/utils/authHeader';

const logger = createLogger(`MatchGetOne-${process.env.NODE_ENV}`);

export async function getOne(event: APIGatewayProxyEvent, basicAuth: BasicAuth): Promise<any> {
  const matchId = event.queryStringParameters?.id;
  if(!matchId) {
      return responseBadRequest({ message: `Can't find match ID` });
  }
  try {
  }
  catch (error) {
    return responseServerError({ message: `Couldn\'t get the match: ${matchId}\n` + JSON.stringify(error) });
  }
}

export const handler: APIGatewayProxyHandler = lambdaHandler(
  async (event: APIGatewayProxyEvent, basicAuth: BasicAuth): Promise<any> => {
    logger.info(JSON.stringify(event));
    return getOne(event, basicAuth);
  }
)