import { DynamoDB } from '/opt/aws/dynamodb';
import { UserPK } from '/opt/datainterface/models/UserPK';

export async function getOne(ddb: DynamoDB.DocumentClient, pk: UserPK): Promise<DynamoDB.GetItemOutput> {
    if(!process.env.DYNAMODB_TABLE) {
      throw new Error('Internal server error. Missing required configuration');
    }
    const params: DynamoDB.DocumentClient.GetItemInput = {
      TableName: process.env.DYNAMODB_TABLE,
      Key: {
        PK: `#USER#${pk.ServerId}#${pk.UserId}`,
        SK: `#USER#${pk.ServerId}#${pk.UserId}`
      }
    };
    return await ddb.get(params).promise();
  }