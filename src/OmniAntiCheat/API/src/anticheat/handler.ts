import { Context } from 'aws-lambda';
import { DynamoDB } from 'aws-sdk'
import { HeaderInfo, validateAuthHeader } from '../user/handler';

const dynamoDb = new DynamoDB.DocumentClient();

export const getVersion = async (event: any, context: Context): Promise<any> => {
    try {
        const results = await dynamoDb.get({
            TableName: process.env.DYNAMODB_TABLE,
            Key: {
                PK: `#ANTICHEAT#`,
                SK: `INFO#`
            },
        }).promise();
        if(results.Item === null || results.Item === undefined) {
            throw new Error(`Anti-cheat info record doesn't exist`);
        }
        const response = {
            statusCode: 200,
            body: JSON.stringify({ Version: results.Item.Version }),
        };
        return response;
    } catch (error) {
        return {
            statusCode: error.statusCode || 501,
            headers: { 'Content-Type': 'text/plain' },
            body: `Couldn't retrieve version: \r\nError: ${error.message}`,
        }
    }
};