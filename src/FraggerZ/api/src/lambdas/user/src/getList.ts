import { APIGatewayProxyEvent, APIGatewayProxyHandler, APIGatewayProxyResult } from 'aws-lambda';
import { createDbInst, DynamoDB } from '/opt/aws/dynamodb';
import { responseOk, responseBadRequest, responseServerError } from '/opt/utils/responses';
import { lambdaHandler } from '/opt/utils/lambdaHandler';
import { createLogger } from '/opt/utils/logger';

const logger = createLogger(`GetUserList-${process.env.NODE_ENV}`);

export async function getList(event: APIGatewayProxyEvent): Promise<any> {
  const matchNumber = event.queryStringParameters?.matchNumber;
  const gameCode = event.queryStringParameters?.gameCode;
  const teamName = event.queryStringParameters?.teamName;
  if(teamName && !gameCode) {
    return responseBadRequest({ message: `gameCode required when querying by teamName` });
  }
  try {
  }
  catch (error) {
    return responseServerError({ message: `Couldn\'t get list of users\n` + JSON.stringify(error) });
  }
}

export const handler: APIGatewayProxyHandler = lambdaHandler(
  async (event: APIGatewayProxyEvent): Promise<any> => {
    logger.info(JSON.stringify(event));
    return getList(event);
  }
)
