import { APIGatewayProxyEvent, APIGatewayProxyHandler, APIGatewayProxyResult } from 'aws-lambda';
import { createDbInst, DynamoDB } from '/opt/aws/dynamodb';
import { responseOk, responseBadRequest, responseServerError } from '/opt/utils/responses';
import { lambdaHandler } from '/opt/utils/lambdaHandler';
import { createLogger } from '/opt/utils/logger';
import { BasicAuth } from '/opt/utils/authHeader';

const logger = createLogger(`UpdateMatch-${process.env.NODE_ENV}`);

interface UpdateMatchRequest {
}

export async function update(event: APIGatewayProxyEvent, basicAuth: BasicAuth): Promise<any> {
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
  async (event: APIGatewayProxyEvent, basicAuth: BasicAuth): Promise<any> => {
    logger.info(JSON.stringify(event));
    return update(event, basicAuth);
  }
)