import { DynamoDB } from '/opt/aws/dynamodb';
import { UserPK } from '/opt/datainterface/models/UserPK';

export async function getOneUser(ddb: DynamoDB.DocumentClient, pk: UserPK): Promise<DynamoDB.GetItemOutput> {
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

export async function update(ddb: DynamoDB.DocumentClient, attributes: any, key: DynamoDB.DocumentClient.Key): Promise<DynamoDB.UpdateItemOutput> {
  if(!process.env.DYNAMODB_TABLE) {
    throw new Error('Internal server error. Missing required configuration');
  }
  let params: DynamoDB.DocumentClient.UpdateItemInput = {
    TableName: process.env.DYNAMODB_TABLE,
    Key: key
  };
  params.UpdateExpression = '';
  params.ExpressionAttributeValues = {};
  for (let k in attributes) {
    if (k === undefined) {
      continue;
    }
    params.UpdateExpression += `${(params.UpdateExpression.length === 0) ? 'set' : ','} ${k} = :${k}`
    params.ExpressionAttributeValues[`:${k}`] = attributes[k];
  }
  return await ddb.update(params).promise();
}

export async function get(ddb: DynamoDB.DocumentClient, key: DynamoDB.DocumentClient.Key): Promise<DynamoDB.GetItemOutput> {
  if(!process.env.DYNAMODB_TABLE) {
    throw new Error('Internal server error. Missing required configuration');
  }
  let params: DynamoDB.DocumentClient.GetItemInput = {
    TableName: process.env.DYNAMODB_TABLE,
    Key: key
  };
  return await ddb.get(params).promise();
}