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

export async function getLatestMatchNum(ddb: DynamoDB.DocumentClient, ladder: {ServerId: string, GameCode: string, LadderName: string, SeasonId: number}): Promise<DynamoDB.QueryOutput> {
    return await dbQuery(ddb, `PK = :pk AND begins_with(SK, :sk)`, {
        ':pk': `#LADDER#${ladder.ServerId}#${ladder.GameCode}#${ladder.LadderName}`,
        ':sk': `SEASON#${ladder.SeasonId}#MATCH#`
    }, { Limit: 1 });
}

export async function dbQuery(ddb: DynamoDB.DocumentClient, keyCondition: string, expressionValues: DynamoDB.DocumentClient.ExpressionAttributeValueMap, queryProps?: object, index?: string): Promise<DynamoDB.QueryOutput> {
    if(!process.env.DYNAMODB_TABLE) {
      throw new Error('Internal server error. Missing required configuration');
    }
    const params: DynamoDB.DocumentClient.QueryInput = {
      TableName: process.env.DYNAMODB_TABLE,
      IndexName: index,
      KeyConditionExpression: keyCondition,
      ExpressionAttributeValues: expressionValues,
      ScanIndexForward: false,
      ...queryProps
    };
    return await ddb.query(params).promise();
}