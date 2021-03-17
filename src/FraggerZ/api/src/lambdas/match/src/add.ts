import { APIGatewayProxyEvent, APIGatewayProxyHandler, APIGatewayProxyResult } from 'aws-lambda';
import * as DynamoDB from 'aws-sdk/clients/dynamodb';
import { createDbInst } from '/opt/aws/dynamodb';
import { responseOk, responseBadRequest, responseServerError } from '/opt/utils/responses';
import { lambdaHandler } from '/opt/utils/lambdaHandler';
import { createLogger } from '/opt/utils/logger';

const logger = createLogger(`AddMatch-${process.env.NODE_ENV}`);

interface AddMatchRequest {
}

export async function add(event: APIGatewayProxyEvent): Promise<any> {
  const req: AddMatchRequest = JSON.parse(event.body ?? '');
  try {
    return responseOk(req);
  }
  catch (error) {
    return responseServerError({ message: 'Couldn\'t add the match' + JSON.stringify(error) });
  }
}

export const handler: APIGatewayProxyHandler = lambdaHandler(
  async (event: APIGatewayProxyEvent): Promise<any> => {
    logger.info(JSON.stringify(event));
    return add(event);
  }
)
