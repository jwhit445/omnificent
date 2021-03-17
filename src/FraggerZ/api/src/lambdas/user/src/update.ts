import { APIGatewayProxyEvent, APIGatewayProxyHandler, APIGatewayProxyResult } from 'aws-lambda';
import * as DynamoDB from 'aws-sdk/clients/dynamodb';
import { createDbInst } from '/opt/aws/dynamodb';
import { responseOk, responseBadRequest, responseServerError } from '/opt/utils/responses';
import { lambdaHandler } from '/opt/utils/lambdaHandler';
import { createLogger } from '/opt/utils/logger';

const logger = createLogger(`UpdateUser-${process.env.NODE_ENV}`);

interface UpdateUserRequest {
  Username?: string;
  StreamURL?: string;
  DateTimeSuspended?: Date;
  DateTimePremiumExpire?: Date;
}

export async function update(event: APIGatewayProxyEvent): Promise<any> {
  const userId = event.queryStringParameters?.id;
  if(!userId) {
      return responseBadRequest({ message: `Can't find user ID` });
  }
  const req: UpdateUserRequest = JSON.parse(event.body ?? '');
  try {
  }
  catch (error) {
    return responseServerError({ message: `Couldn\'t update the user: ${userId}\n` + JSON.stringify(error) });
  }
}

export const handler: APIGatewayProxyHandler = lambdaHandler(
  async (event: APIGatewayProxyEvent): Promise<any> => {
    logger.info(JSON.stringify(event));
    return update(event);
  }
)