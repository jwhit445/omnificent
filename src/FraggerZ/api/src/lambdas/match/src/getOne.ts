import { APIGatewayProxyEvent, APIGatewayProxyHandler, APIGatewayProxyResult } from 'aws-lambda';
import * as DynamoDB from 'aws-sdk/clients/dynamodb';
import { createDbInst } from '/opt/aws/dynamodb';
import { responseOk, responseBadRequest, responseServerError } from '/opt/utils/responses';
import { lambdaHandler } from '/opt/utils/lambdaHandler';
import { createLogger } from '/opt/utils/logger';

const logger = createLogger(`GetMatch-${process.env.NODE_ENV}`);

export async function getOne(event: APIGatewayProxyEvent): Promise<any> {
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
  async (event: APIGatewayProxyEvent): Promise<any> => {
    logger.info(JSON.stringify(event));
    return getOne(event);
  }
)