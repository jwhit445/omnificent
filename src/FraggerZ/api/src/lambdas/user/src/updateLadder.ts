import { APIGatewayProxyEvent, APIGatewayProxyHandler, APIGatewayProxyResult } from 'aws-lambda';
import * as DynamoDB from 'aws-sdk/clients/dynamodb';
import { createDbInst } from '/opt/aws/dynamodb';
import { responseOk, responseBadRequest, responseServerError } from '/opt/utils/responses';
import { lambdaHandler } from '/opt/utils/lambdaHandler';
import { createLogger } from '/opt/utils/logger';

const logger = createLogger(`UpdateUserLadder-${process.env.NODE_ENV}`);

interface UpdateUserLadderRequest {
  GameCode: string;
  LadderName: string;
  CurrentSeason: number;
}

export async function updateLadder(event: APIGatewayProxyEvent): Promise<any> {
  const userId = event.queryStringParameters?.id;
  if(!userId) {
      return responseBadRequest({ message: `Can't find user ID` });
  }
  const req: UpdateUserLadderRequest = JSON.parse(event.body ?? '');
  try {
  }
  catch (error) {
    return responseServerError({ message: `Couldn\'t update the ladder for user: ${userId}\n` + JSON.stringify(error) });
  }
}

export const handler: APIGatewayProxyHandler = lambdaHandler(
  async (event: APIGatewayProxyEvent): Promise<any> => {
    logger.info(JSON.stringify(event));
    return updateLadder(event);
  }
)