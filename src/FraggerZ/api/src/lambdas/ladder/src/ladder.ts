import { DynamoDB } from '/opt/aws/dynamodb';
import { LadderPK } from '/opt/datainterface/models/LadderPK';

export async function getOneLadder(ddb: DynamoDB.DocumentClient, pk: LadderPK): Promise<DynamoDB.GetItemOutput> {
    if(!process.env.DYNAMODB_TABLE) {
      throw new Error('Internal server error. Missing required configuration');
    }
    const params: DynamoDB.DocumentClient.GetItemInput = {
      TableName: process.env.DYNAMODB_TABLE,
      Key: {
        PK: `#LADDER#${pk.ServerId}#${pk.GameCode}#${pk.LadderName}`,
        SK: `#LADDER#${pk.ServerId}#${pk.GameCode}#${pk.LadderName}`
      }
    };
    return await ddb.get(params).promise();
}