import { APIGatewayProxyEvent, APIGatewayProxyHandler, APIGatewayProxyResult } from 'aws-lambda';
import { createDbInst, DynamoDB, dbUpdate } from '/opt/aws/dynamodb';
import { responseOk, responseBadRequest, responseServerError } from '/opt/utils/responses';
import { lambdaHandler } from '/opt/utils/lambdaHandler';
import { createLogger } from '/opt/utils/logger';
import { BasicAuth } from '/opt/utils/authHeader';
import * as user from './user';

const logger = createLogger(`AddUser-${process.env.NODE_ENV}`);

interface AddUserRequest {
  UserId: string;
  Username: string;
}

export async function add(event: APIGatewayProxyEvent, basicAuth: BasicAuth): Promise<any> {
  const req: AddUserRequest = JSON.parse(event.body ?? '');
  let serverId: string = basicAuth.username;
  try {
    if(!req.UserId || !req.Username) {
      return responseBadRequest({ message: 'Missing required request data.' });
    }
    const ddb: DynamoDB.DocumentClient = createDbInst();
    const existingItemQuery = await user.getOneUser(ddb, req);
    if(existingItemQuery.Item) {
      return responseOk({ message: `User successfully added` });
    }
    await dbUpdate(ddb, { ServerId: serverId, UserId: req.UserId, Username: req.Username }, {
      PK: `#USER#${serverId}#${req.UserId}`,
      SK: `#USER#${serverId}#${req.UserId}`
    });
    return responseOk({ message: `User successfully added` });
  }
  catch (error) {
    return responseServerError({ message: 'Couldn\'t register the user' + JSON.stringify(error) });
  }
}


export const handler: APIGatewayProxyHandler = lambdaHandler(
  async (event: APIGatewayProxyEvent, basicAuth: BasicAuth): Promise<any> => {
    logger.info(JSON.stringify(event));
    return add(event, basicAuth);
  }
)
