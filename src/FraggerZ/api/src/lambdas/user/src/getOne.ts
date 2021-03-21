import { APIGatewayProxyEvent, APIGatewayProxyHandler, APIGatewayProxyResult } from 'aws-lambda';
import { createDbInst, DynamoDB } from '/opt/aws/dynamodb';
import { responseOk, responseBadRequest, responseServerError } from '/opt/utils/responses';
import { lambdaHandler } from '/opt/utils/lambdaHandler';
import { createLogger } from '/opt/utils/logger';
import { BasicAuth } from '/opt/utils/authHeader';
import * as user from './user';

const logger = createLogger(`UserGetOne-${process.env.NODE_ENV}`);

export async function getOne(event: APIGatewayProxyEvent, basicAuth: BasicAuth): Promise<any> {
  const userId = event.queryStringParameters?.id;
  const serverId: string = basicAuth.username;
  if(!userId) {
      return responseBadRequest({ message: `Can't find user ID` });
  }
  try {
    const ddb: DynamoDB.DocumentClient = createDbInst();
    const result = await user.getOneUser(ddb, { ServerId: serverId, UserId: userId });
    if(!result.Item) {
      return responseBadRequest({ message: `Can't find user: ${userId}` });
    }
    return responseOk(result.Item);
  }
  catch (error) {
    return responseServerError({ message: `Couldn\'t get the user: ${userId}\n` + JSON.stringify(error) });
  }
}

export const handler: APIGatewayProxyHandler = lambdaHandler(
  async (event: APIGatewayProxyEvent, basicAuth: BasicAuth): Promise<any> => {
    logger.info(JSON.stringify(event));
    return getOne(event, basicAuth);
  }
)