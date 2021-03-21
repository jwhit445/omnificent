import { APIGatewayProxyEvent, APIGatewayProxyHandler, APIGatewayProxyResult } from 'aws-lambda';
import { createDbInst, DynamoDB } from '/opt/aws/dynamodb';
import { responseOk, responseBadRequest, responseServerError } from '/opt/utils/responses';
import { lambdaHandler } from '/opt/utils/lambdaHandler';
import { createLogger } from '/opt/utils/logger';
import { BasicAuth } from '/opt/utils/authHeader';

const logger = createLogger(`AddMatch-${process.env.NODE_ENV}`);

interface AddMatchRequest {
}

export async function add(event: APIGatewayProxyEvent, basicAuth: BasicAuth): Promise<any> {
  const req: AddMatchRequest = JSON.parse(event.body ?? '');
  try {
    return responseOk(req);
  }
  catch (error) {
    return responseServerError({ message: 'Couldn\'t add the match' + JSON.stringify(error) });
  }
}

export const handler: APIGatewayProxyHandler = lambdaHandler(
  async (event: APIGatewayProxyEvent, basicAuth: BasicAuth): Promise<any> => {
    logger.info(JSON.stringify(event));
    return add(event, basicAuth);
  }
)
