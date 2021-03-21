import { DynamoDB } from '/opt/aws/dynamodb';
import { GamePK } from '/opt/datainterface/models/GamePK';

export async function getOneGame(ddb: DynamoDB.DocumentClient, pk: GamePK): Promise<DynamoDB.GetItemOutput> {
    if(!process.env.DYNAMODB_TABLE) {
      throw new Error('Internal server error. Missing required configuration');
    }
    const params: DynamoDB.DocumentClient.GetItemInput = {
      TableName: process.env.DYNAMODB_TABLE,
      Key: {
        PK: `#GAME#${pk.ServerId}#${pk.GameCode}`,
        SK: `#GAME#${pk.ServerId}#${pk.GameCode}`
      }
    };
    return await ddb.get(params).promise();
}