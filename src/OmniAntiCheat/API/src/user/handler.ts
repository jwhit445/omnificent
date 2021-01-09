import { Context } from 'aws-lambda';
import { DynamoDB } from 'aws-sdk'

const dynamoDb = new DynamoDB.DocumentClient()

export const upsert = async (event: any, context: Context): Promise<any> => {
    try {
        const data = JSON.parse(event.body)
        const user = {
            Username: data.User.Username,
            EpicID: data.User.EpicID,
        };
        try {
            await dynamoDb.put({
                TableName: process.env.DYNAMODB_TABLE,
                Item: {
                    PK: `#USER#${user.EpicID}`,
                    SK: `PROFILE`,
                    EpicID: user.EpicID,
                    Username: user.Username
                }
            }).promise();
            // create a response
            const response = {
                statusCode: 200,
                body: `Successfully created or modified user: ${user.Username}`
            }
            return response;
        } catch (error) {
            throw new Error('Couldn\'t create the match for Username:' + user.Username);
        }
    } catch (error) {
        return {
            statusCode: error.statusCode || 501,
            headers: { 'Content-Type': 'text/plain' },
            body: 'Couldn\'t upsert the user.',
        }
    }
};

export const updateInfo = async (event: any, context: Context): Promise<any> => {
    try {
        // todo stuff
        const response = {
            statusCode: 200,
            body: 'SUCCESS updateInfo!',
        };
        return response;
    } catch (error) {
        return {
            statusCode: error.statusCode || 501,
            headers: { 'Content-Type': 'text/plain' },
            body: 'Couldn\'t update the user\'s info.',
        }
    }
};

export const getStatuses = async (event: any, context: Context): Promise<any> => {
    try {
        // todo stuff
        const response = {
            statusCode: 200,
            body: 'SUCCESS getStatuses!',
        };
        return response;
    } catch (error) {
        return {
            statusCode: error.statusCode || 501,
            headers: { 'Content-Type': 'text/plain' },
            body: 'Couldn\'t get the statuses for the given user.',
        }
    }
};