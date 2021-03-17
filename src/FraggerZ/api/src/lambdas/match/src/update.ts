import { APIGatewayProxyEvent, APIGatewayProxyHandler, APIGatewayProxyResult } from 'aws-lambda';
import * as DynamoDB from 'aws-sdk/clients/dynamodb';
import { createDbInst } from '/opt/aws/dynamodb';
import { responseOk, responseBadRequest, responseServerError } from '/opt/utils/responses';
import { lambdaHandler } from '/opt/utils/lambdaHandler';
import { createLogger } from '/opt/utils/logger';

const logger = createLogger(`UpdateMatch-${process.env.NODE_ENV}`);

interface UpdateMatchRequest {
}

export async function update(event: APIGatewayProxyEvent): Promise<any> {
  const matchNumber = event.queryStringParameters?.id;
  if(!matchNumber) {
      return responseBadRequest({ message: `Can't find match ID` });
  }
  const req: UpdateMatchRequest = JSON.parse(event.body ?? '');
  try {
  }
  catch (error) {
    return responseServerError({ message: `Couldn\'t update the match: ${matchNumber}\n` + JSON.stringify(error) });
  }
}

export const handler: APIGatewayProxyHandler = lambdaHandler(
  async (event: APIGatewayProxyEvent): Promise<any> => {
    logger.info(JSON.stringify(event));
    return update(event);
  }
)